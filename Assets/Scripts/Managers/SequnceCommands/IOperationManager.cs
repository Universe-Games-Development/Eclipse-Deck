using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

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

public struct OperationResult {
    public bool IsSuccess { get; }
    public string Message { get; }

    private OperationResult(bool isSuccess, string resultMessage = null) {
        IsSuccess = isSuccess;
        Message = resultMessage;
    }

    public static OperationResult Success() => new(true, null);
    public static OperationResult Failure(string errorMessage = null) => new(false, errorMessage);

    public static implicit operator bool(OperationResult result) => result.IsSuccess;
    public static implicit operator OperationResult(bool IsSuccess) => new(IsSuccess);
}


public enum Priority {
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}
