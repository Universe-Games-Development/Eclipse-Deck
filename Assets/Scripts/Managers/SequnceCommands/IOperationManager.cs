using Cysharp.Threading.Tasks;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum Priority {
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public interface IOperationManager {
    bool IsRunning { get; }
    int QueueCount { get; }

    event Action<GameOperation, OperationResult> OnOperationEnd;
    event Action OnQueueEmpty;

    UniTask CancelAllAsync();
    void CancelCurrent();
    void ClearQueue();
    bool IsQueueEmpty();
    void Push(GameOperation operation, Priority priority = Priority.Normal);
    void PushRange(IEnumerable<GameOperation> operations, Priority priority = Priority.Normal);
}

public class OperationManager : BaseQueueManager<GameOperation>, IOperationManager {
    public event Action<GameOperation, OperationResult> OnOperationEnd;

    protected override LogCategory LogCategory => LogCategory.OperationManager;
    protected override string TaskTypeName => "operation";

    protected override async UniTask<OperationResult> ProcessTaskAsync(GameOperation operation, CancellationToken cancellationToken) {
        try {
            logger.LogInfo($"Beginning operation: {operation}", LogCategory);
            bool success = await operation.ExecuteAsync();

            return success ? OperationResult.Success() : OperationResult.Failure("Failed execution");

        } catch (OperationCanceledException) {
            logger.LogInfo($"Operation {operation} was cancelled", LogCategory);
            throw;
        }
    }

    protected override void OnTaskCompleted(GameOperation task, OperationResult result) {
        OnOperationEnd?.Invoke(task, result);
    }
}

public readonly struct ReadLock : IDisposable {
    private readonly ReaderWriterLockSlim _lock;

    public ReadLock(ReaderWriterLockSlim @lock) {
        _lock = @lock;
        _lock.EnterReadLock();
    }

    public void Dispose() {
        _lock.ExitReadLock();
    }
}

public readonly struct WriteLock : IDisposable {
    private readonly ReaderWriterLockSlim _lock;

    public WriteLock(ReaderWriterLockSlim @lock) {
        _lock = @lock;
        _lock.EnterWriteLock();
    }

    public void Dispose() {
        _lock.ExitWriteLock();
    }
}

public interface IOperationExecutor {
    /// <summary>
    /// Executes an operation asynchronously with target filling and operation management
    /// </summary>
    /// <param name="operationData">The operation data to execute</param>
    /// <param name="source">The source unit (Card, Creature, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation execution</returns>
    UniTask<OperationResult> ExecuteAsync(
        OperationData operationData,
        UnitModel source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an operation with pre-filled targets
    /// </summary>
    UniTask<OperationResult> ExecuteAsync(
        OperationData operationData,
        TargetRegistry targetRegistry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an operation is currently executing
    /// </summary>
    bool IsExecuting { get; }
}

public class OperationExecutor : IOperationExecutor, IDisposable {
    private readonly IOperationFactory _operationFactory;
    private readonly IOperationManager _operationManager;
    private readonly ITargetFiller _targetFiller;

    private UniTaskCompletionSource<OperationResult> _completionSource;
    private GameOperation _currentOperation;
    private CancellationTokenRegistration _cancellationRegistration;

    public bool IsExecuting => _currentOperation != null;

    public OperationExecutor(
        IOperationFactory operationFactory,
        IOperationManager operationManager,
        ITargetFiller targetFiller) {
        _operationFactory = operationFactory ?? throw new ArgumentNullException(nameof(operationFactory));
        _operationManager = operationManager ?? throw new ArgumentNullException(nameof(operationManager));
        _targetFiller = targetFiller ?? throw new ArgumentNullException(nameof(targetFiller));
    }

    public async UniTask<OperationResult> ExecuteAsync(
        OperationData operationData,
        UnitModel source,
        CancellationToken cancellationToken = default) {

        if (operationData == null) {
            throw new ArgumentNullException(nameof(operationData));
        }

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        try {
            // Fill targets
            TargetsFillResult fillResult = await _targetFiller.FillTargetsAsync(
                operationData,
                source,
                cancellationToken);

            // If player cancelled or failed to fill targets
            if (!fillResult.Status) {
                return OperationResult.Failure("Target fill failed");
            }

            TargetRegistry targetRegistry = fillResult.TargetRegistry;

            // Add source to registry if it's a card
            if (source is Card cardSource) {
                targetRegistry.Add(TargetKeys.SourceCard, cardSource);
            }

            // Execute with filled targets
            return await ExecuteAsync(operationData, targetRegistry, cancellationToken);

        } catch (OperationCanceledException) {
            return OperationResult.Failure("Canceled");
        } catch (Exception ex) {
            Debug.LogError($"[OperationExecutor] Unexpected error: {ex.Message}");
            return OperationResult.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async UniTask<OperationResult> ExecuteAsync(
        OperationData operationData,
        TargetRegistry targetRegistry,
        CancellationToken cancellationToken = default) {

        if (IsExecuting) {
            return OperationResult.Failure("Another operation is already executing");
        }
        if (operationData == null) {
            return OperationResult.Failure("Data is null");
        }

        try {
            _completionSource = new UniTaskCompletionSource<OperationResult>();

            // Register cancellation
            _cancellationRegistration = cancellationToken.Register(() => {
                _completionSource?.TrySetResult(OperationResult.Failure("Canceled"));
                Cleanup();
            });

            // Create operation
            _currentOperation = operationData.CreateOperation(_operationFactory, targetRegistry);

            // Subscribe to operation completion
            _operationManager.OnQueueEmpty += HandleOperationComplete;

            // Push to operation manager
            _operationManager.Push(_currentOperation);

            // Wait for completion
            OperationResult result = await _completionSource.Task;

            return result;

        } catch (OperationCanceledException) {
            return OperationResult.Failure("Canceled");
        } finally {
            Cleanup();
        }
    }

    private void HandleOperationComplete() {
        // Operation completed successfully
        _completionSource?.TrySetResult(OperationResult.Success());
    }

    private void Cleanup() {
        _operationManager.OnQueueEmpty -= HandleOperationComplete;
        _cancellationRegistration.Dispose();
        _currentOperation = null;
        _completionSource = null;
    }

    public void Dispose() {
        Cleanup();
    }
}

public enum OperationStatus {
    Success,
    Failure,
    Canceled
}

public struct OperationResult {
    public OperationStatus Status { get; }
    public bool IsSuccess => Status == OperationStatus.Success;
    public bool Cancelled => Status == OperationStatus.Canceled;
    public bool Failed => Status == OperationStatus.Failure;

    const string cancelledMessage = "Operation was cancelled";
    public string Message { get; }

    private OperationResult(OperationStatus status, string resultMessage = null) {
        Status = status;
        Message = resultMessage;
    }

    public static OperationResult Success() => new(OperationStatus.Success);
    public static OperationResult Failure(string errorMessage = null) => new(OperationStatus.Failure, errorMessage);
    public static OperationResult Canceled() => new(OperationStatus.Canceled, cancelledMessage);

    public static implicit operator bool(OperationResult result) => result.Status == OperationStatus.Success;
}
