using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Zenject;

public class OperationManager : BaseQueueManager<GameOperation>, IOperationManager {
    [Inject] private ITargetFiller operationFiller;

    public event Action<GameOperation, OperationResult> OnOperationEnd;

    protected override LogCategory LogCategory => LogCategory.OperationManager;
    protected override string TaskTypeName => "operation";

    protected override async UniTask<OperationResult> ProcessTaskAsync(GameOperation operation, CancellationToken cancellationToken) {
        try {
            if (!ValidateOperation(operation)) {
                return OperationResult.Failure($"Validation failed: {operation}");
            }

            logger.LogInfo($"Beginning operation: {operation}", LogCategory);

            TargetOperationRequest request = new(
                operation.GetTargets(),
                operation.IsMandatory,
                operation.Source);

            TargetOperationResult targets = await operationFiller.FillTargetsAsync(request, cancellationToken);

            if (targets == null) {
                return OperationResult.Failure("Was cancelled during target filling");
            }

            operation.SetTargets(targets.FilledTargets);

            if (operation.IsReady()) {
                logger.LogDebug($"Operation {operation} is ready, executing...", LogCategory);
                bool success = await operation.Execute();
                return success ? OperationResult.Success() : OperationResult.Failure("Failed execution");
            } else {
                return OperationResult.Failure("Not ready for execution");
            }
        } catch (OperationCanceledException) {
            logger.LogInfo($"Operation {operation} was cancelled", LogCategory);
            throw;
        }
    }

    private bool ValidateOperation(GameOperation operation) {
        List<TargetInfo> typedTargetBases = operation.GetTargets();

        if (!operationFiller.CanFillTargets(typedTargetBases, operation.Source.OwnerId)) {
            logger.LogWarning($"{operation} cannot be executed - targets cannot be filled", LogCategory);
            return false;
        }

        logger.LogDebug($"Operation {operation} validation passed", LogCategory);
        return true;
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


