using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface IOperationManager {
    bool IsRunning { get; }
    int QueueCount { get; }

    event Action<GameOperation, ExecutionResult> OnOperationEnd;
    event Action OnQueueEmpty;

    UniTask CancelAllAsync();
    void CancelCurrent();
    int ClearQueue();
    bool IsQueueEmpty();
    void Push(GameOperation operation, Priority priority = Priority.Normal);
    void PushRange(IEnumerable<GameOperation> operations, Priority priority = Priority.Normal);
}

public class OperationManager : TaskQueueManager<GameOperation>, IOperationManager {
    public event Action<GameOperation, ExecutionResult> OnOperationEnd;

    protected override LogCategory LogCategory => LogCategory.OperationManager;
    protected override string TaskTypeName => "operation";

    protected override void OnTaskCompleted(GameOperation task, ExecutionResult result) {
        OnOperationEnd?.Invoke(task, result);
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
    UniTask<ExecutionResult> ExecuteAsync(
        OperationData operationData,
        UnitModel source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an operation with pre-filled targets
    /// </summary>
    UniTask<ExecutionResult> ExecuteAsync(
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

    private UniTaskCompletionSource<ExecutionResult> _completionSource;
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

    public async UniTask<ExecutionResult> ExecuteAsync(
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
                return ExecutionResult.Failure("Target fill failed");
            }

            TargetRegistry targetRegistry = fillResult.TargetRegistry;

            // Add source to registry if it's a card
            if (source is Card cardSource) {
                targetRegistry.Add(TargetKeys.SourceCard, cardSource);
            }

            // Execute with filled targets
            return await ExecuteAsync(operationData, targetRegistry, cancellationToken);

        } catch (OperationCanceledException) {
            return ExecutionResult.Failure("Canceled");
        } catch (Exception ex) {
            Debug.LogError($"[OperationExecutor] Unexpected error: {ex.Message}");
            return ExecutionResult.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async UniTask<ExecutionResult> ExecuteAsync(
        OperationData operationData,
        TargetRegistry targetRegistry,
        CancellationToken cancellationToken = default) {

        if (IsExecuting) {
            return ExecutionResult.Failure("Another operation is already executing");
        }
        if (operationData == null) {
            return ExecutionResult.Failure("Data is null");
        }

        try {
            _completionSource = new UniTaskCompletionSource<ExecutionResult>();

            // Register cancellation
            _cancellationRegistration = cancellationToken.Register(() => {
                _completionSource?.TrySetResult(ExecutionResult.Failure("Canceled"));
                Cleanup();
            });

            // Create operation
            _currentOperation = operationData.CreateOperation(_operationFactory, targetRegistry);

            // Subscribe to operation completion
            _operationManager.OnQueueEmpty += HandleOperationComplete;

            // Push to operation manager
            _operationManager.Push(_currentOperation);

            // Wait for completion
            ExecutionResult result = await _completionSource.Task;

            return result;

        } catch (OperationCanceledException) {
            return ExecutionResult.Failure("Canceled");
        } finally {
            Cleanup();
        }
    }

    private void HandleOperationComplete() {
        // Operation completed successfully
        _completionSource?.TrySetResult(ExecutionResult.Success());
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

