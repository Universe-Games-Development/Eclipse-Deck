using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class OperationManager : MonoBehaviour, IOperationManager {
    [SerializeField] private OperationTargetsFiller operationFiller;

    private PriorityQueue<Priority, GameOperation> _operationQueue;
    private GameOperation _currentOperation;
    private bool _isRunning;
    private CancellationTokenSource _globalCancellationSource;
    private readonly ReaderWriterLockSlim _queueLock = new();

    public event Action<GameOperation, OperationStatus> OnOperationStatus;
    public event Action OnQueueEmpty;

    public bool IsRunning => _isRunning;
    public GameOperation CurrentOperation => _currentOperation;
    public int QueueCount => _operationQueue?.Count ?? 0;

    private void Awake() {
        _operationQueue = new PriorityQueue<Priority, GameOperation>();
        _globalCancellationSource = new CancellationTokenSource();

        //GameLogger.LogInfo("OperationManager initialized", LogCategory.OperationManager);
    }

    private void OnDestroy() {
        _globalCancellationSource?.Cancel();
        _globalCancellationSource?.Dispose();
        _queueLock?.Dispose();

        GameLogger.LogInfo("OperationManager destroyed", LogCategory.OperationManager);
    }

    public void Push(GameOperation operation, Priority priority = Priority.Normal) {
        if (operation == null) {
            GameLogger.LogError("Cannot push a null operation", LogCategory.OperationManager);
            return;
        }

        using (new WriteLock(_queueLock)) {
            _operationQueue.Enqueue(priority, operation);
        }

        GameLogger.LogDebug($"Operation '{operation}' pushed with priority {priority}. Queue size: {QueueCount}",
            LogCategory.OperationManager);

        TryStartProcessing().Forget();
    }

    public void PushRange(IEnumerable<GameOperation> operations, Priority priority = Priority.Normal) {
        if (operations == null) {
            GameLogger.LogWarning("Cannot push null operations collection", LogCategory.OperationManager);
            return;
        }

        bool hasValidOperations = false;
        int validCount = 0;
        int nullCount = 0;

        using (new WriteLock(_queueLock)) {
            foreach (var operation in operations) {
                if (operation != null) {
                    _operationQueue.Enqueue(priority, operation);
                    hasValidOperations = true;
                    validCount++;
                } else {
                    nullCount++;
                }
            }
        }

        if (nullCount > 0) {
            GameLogger.LogWarning($"Skipped {nullCount} null operations in PushRange", LogCategory.OperationManager);
        }

        if (hasValidOperations) {
            GameLogger.LogDebug($"Pushed {validCount} operations with priority {priority}. Queue size: {QueueCount}",
                LogCategory.OperationManager);
            TryStartProcessing().Forget();
        } else {
            GameLogger.LogWarning("No valid operations to push", LogCategory.OperationManager);
        }
    }

    public async UniTask CancelAllAsync() {
        GameLogger.LogInfo("Cancelling all operations...", LogCategory.OperationManager);

        // Cancel current operation
        _globalCancellationSource?.Cancel();

        // Wait for current operation to complete with timeout
        var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(10), ignoreTimeScale: true);
        var completionTask = WaitForCurrentOperationCompletion();

        int completedIndex = await UniTask.WhenAny(completionTask, timeoutTask);

        if (completedIndex == 1) {
            GameLogger.LogWarning("Force stopping operation due to timeout", LogCategory.OperationManager);
            _isRunning = false;
            _currentOperation = null;
        }

        // Create new cancellation token
        _globalCancellationSource?.Dispose();
        _globalCancellationSource = new CancellationTokenSource();

        // Clear queue
        var clearedCount = ClearQueueSafe();

        GameLogger.LogInfo($"All operations cancelled. Cleared {clearedCount} queued operations", LogCategory.OperationManager);
    }

    private async UniTask WaitForCurrentOperationCompletion() {
        while (_currentOperation != null && _isRunning) {
            await UniTask.Yield();
        }
    }

    public void CancelCurrent() {
        if (_currentOperation != null && _globalCancellationSource != null) {
            GameLogger.LogInfo($"Cancelling current operation: {_currentOperation}", LogCategory.OperationManager);
            ReplaceCancellationTokenSource();
        } else {
            GameLogger.LogDebug("No current operation to cancel", LogCategory.OperationManager);
        }
    }

    private void ReplaceCancellationTokenSource() {
        var oldSource = _globalCancellationSource;
        _globalCancellationSource = new CancellationTokenSource();
        oldSource.Cancel();
        oldSource.Dispose();

        GameLogger.LogDebug("Cancellation token source replaced", LogCategory.OperationManager);
    }

    public void ClearQueue() {
        var clearedCount = ClearQueueSafe();
        GameLogger.LogInfo($"Cleared {clearedCount} operations from queue", LogCategory.OperationManager);
    }

    private int ClearQueueSafe() {
        using (new WriteLock(_queueLock)) {
            var count = _operationQueue.Count;
            _operationQueue.Clear();
            return count;
        }
    }

    private async UniTaskVoid TryStartProcessing() {
        if (_isRunning) {
            GameLogger.LogDebug("Processing already running, skipping start", LogCategory.OperationManager);
            return;
        }

        _isRunning = true;
        GameLogger.LogInfo("Starting operation processing...", LogCategory.OperationManager);

        int processedCount = 0;

        try {
            while (HasOperationsInQueue()) {
                if (!TryDequeueOperation(out _currentOperation))
                    break;

                GameLogger.LogInfo($"Processing operation [{processedCount + 1}]: {_currentOperation}",
                    LogCategory.OperationManager);

                try {
                    await ProcessOperationAsync(_currentOperation, _globalCancellationSource.Token);
                    processedCount++;
                } catch (OperationCanceledException) {
                    GameLogger.LogInfo($"Operation cancelled: {_currentOperation}", LogCategory.OperationManager);
                } catch (Exception ex) {
                    GameLogger.LogError($"Operation failed: {_currentOperation} - {ex.Message}", LogCategory.OperationManager);
                    GameLogger.LogDebug($"Operation exception details: {ex}", LogCategory.OperationManager);
                } finally {
                    _currentOperation = null;
                }
            }

            GameLogger.LogInfo($"Queue processing completed. Processed {processedCount} operations", LogCategory.OperationManager);
            OnQueueEmpty?.Invoke();
        } catch (Exception ex) {
            GameLogger.LogError($"Critical error in operation processing: {ex.Message}", LogCategory.OperationManager);
            GameLogger.LogException(ex, LogCategory.OperationManager);
        } finally {
            _isRunning = false;
            _currentOperation = null;
            GameLogger.LogDebug("Operation processing finished", LogCategory.OperationManager);
        }
    }

    private bool HasOperationsInQueue() {
        using (new ReadLock(_queueLock)) {
            return !_operationQueue.IsEmpty();
        }
    }

    private bool TryDequeueOperation(out GameOperation operation) {
        using (new WriteLock(_queueLock)) {
            var result = _operationQueue.TryDequeue(out operation);
            if (result) {
                GameLogger.LogDebug($"Dequeued operation: {operation}. Remaining: {_operationQueue.Count}",
                    LogCategory.OperationManager);
            }
            return result;
        }
    }

    private async UniTask ProcessOperationAsync(GameOperation operation, CancellationToken cancellationToken) {
        OperationStatus status = OperationStatus.Failed;
        var startTime = DateTime.UtcNow;

        try {
            if (!ValidateOperation(operation)) {
                status = OperationStatus.Failed;
                GameLogger.LogWarning($"Operation validation failed: {operation}", LogCategory.OperationManager);
                return;
            }

            GameLogger.LogInfo($"Beginning operation: {operation}", LogCategory.OperationManager);

            TargetOperationRequest request = new(
                operation.RequestTargets,
                operation.IsMandatory,
                operation.Source);

            var targets = await operationFiller.FillTargetsAsync(
                request,
                cancellationToken);

            if (targets == null) {
                status = OperationStatus.Cancelled;
                GameLogger.LogInfo($"Operation {operation} was cancelled during target filling", LogCategory.OperationManager);
                return;
            }

            operation.SetTargets(targets.FilledTargets);

            if (operation.IsReady()) {
                GameLogger.LogDebug($"Operation {operation} is ready, executing...", LogCategory.OperationManager);

                OnOperationStatus?.Invoke(operation, OperationStatus.Start);
                bool success = operation.Execute();
                status = success ? OperationStatus.Success : OperationStatus.Failed;

                GameLogger.LogInfo($"Operation {operation} execution result: {status}", LogCategory.OperationManager);
            } else {
                GameLogger.LogWarning($"Operation {operation} is not ready for execution", LogCategory.OperationManager);
                status = OperationStatus.Cancelled;
            }
        } catch (OperationCanceledException) {
            status = OperationStatus.Cancelled;
            GameLogger.LogInfo($"Operation {operation} was cancelled", LogCategory.OperationManager);
            throw;
        } catch (Exception ex) {
            status = OperationStatus.ThrownException;
            GameLogger.LogError($"Operation {operation} failed with exception: {ex.Message}", LogCategory.OperationManager);
            GameLogger.LogException(ex, LogCategory.OperationManager);
        } finally {
            var duration = DateTime.UtcNow - startTime;
            GameLogger.LogInfo($"{operation} finished with status: {status} (Duration: {duration.TotalMilliseconds:F1}ms)",
                LogCategory.OperationManager);
            OnOperationStatus?.Invoke(operation, status);
        }
    }

    private bool ValidateOperation(GameOperation operation) {
        if (operation.RequestTargets.Count > 0 && !operationFiller.CanFillTargets(operation.RequestTargets)) {
            GameLogger.LogWarning($"{operation} cannot be executed - targets cannot be filled", LogCategory.OperationManager);
            return false;
        }

        GameLogger.LogDebug($"Operation {operation} validation passed", LogCategory.OperationManager);
        return true;
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void LogQueueState() {
        using (new ReadLock(_queueLock)) {
            string currentOperationStr = _currentOperation != null ? _currentOperation.ToString() : "None";
            var message = $"Queue State - Running: {_isRunning}, Current: {currentOperationStr}, " +
                         $"Total Queue Size: {_operationQueue.Count}";

            GameLogger.LogInfo(message, LogCategory.OperationManager);

            // Log details of queued operations if any
            if (_operationQueue.Count > 0) {
                var operations = _operationQueue.GetAllItems().Take(5).Select(op => op);
                GameLogger.LogDebug($"Next operations: {string.Join(", ", operations)}{(_operationQueue.Count > 5 ? "..." : "")}",
                    LogCategory.OperationManager);
            }
        }
    }

    public List<string> GetQueuedOperationNames() {
        using (new ReadLock(_queueLock)) {
            var operations = _operationQueue.GetAllItems().Select(op => op.ToString()).ToList();
            GameLogger.LogDebug($"Retrieved {operations.Count} queued operation names", LogCategory.OperationManager);
            return operations;
        }
    }

    public List<GameOperation> CancelOperations(IEnumerable<GameOperation> operationsToRemove) {
        if (operationsToRemove == null) {
            GameLogger.LogWarning("Cannot cancel null operations collection", LogCategory.OperationManager);
            return new List<GameOperation>();
        }

        var operationsSet = new HashSet<GameOperation>(operationsToRemove);
        List<GameOperation> removedOperations;

        using (new WriteLock(_queueLock)) {
            removedOperations = _operationQueue.RemoveItems(op => operationsSet.Contains(op));
        }

        if (_currentOperation != null && operationsSet.Contains(_currentOperation)) {
            CancelCurrent();
            removedOperations.Add(_currentOperation);
        }

        if (removedOperations.Count > 0) {
            var operationNames = removedOperations.Select(op => op);
            GameLogger.LogInfo($"Cancelled {removedOperations.Count} operations: {string.Join(", ", operationNames)}",
                LogCategory.OperationManager);
        } else {
            GameLogger.LogDebug("No operations were cancelled (not found in queue)", LogCategory.OperationManager);
        }

        return removedOperations;
    }

    public List<GameOperation> RemoveOperations(Func<GameOperation, bool> predicate) {
        if (predicate == null) {
            GameLogger.LogWarning("Cannot remove operations with null predicate", LogCategory.OperationManager);
            return new List<GameOperation>();
        }

        using (new WriteLock(_queueLock)) {
            var removed = _operationQueue.RemoveItems(predicate);

            if (removed.Count > 0) {
                var operationNames = removed.Select(op => op);
                GameLogger.LogInfo($"Removed {removed.Count} operations by predicate: {string.Join(", ", operationNames)}",
                    LogCategory.OperationManager);
            } else {
                GameLogger.LogDebug("No operations matched the removal predicate", LogCategory.OperationManager);
            }

            return removed;
        }
    }

    public bool IsQueueEmpty() {
        return QueueCount == 0;
    }

    #region Helper Disposable Structs for Lock Management
    private readonly struct ReadLock : IDisposable {
        private readonly ReaderWriterLockSlim _lock;

        public ReadLock(ReaderWriterLockSlim @lock) {
            _lock = @lock;
            _lock.EnterReadLock();
        }

        public void Dispose() {
            _lock.ExitReadLock();
        }
    }

    private readonly struct WriteLock : IDisposable {
        private readonly ReaderWriterLockSlim _lock;

        public WriteLock(ReaderWriterLockSlim @lock) {
            _lock = @lock;
            _lock.EnterWriteLock();
        }

        public void Dispose() {
            _lock.ExitWriteLock();
        }
    }
    #endregion
}

public enum OperationStatus {
    Start,
    Success,
    PartialSuccess,
    Failed,
    Cancelled,
    ThrownException
}

public enum Priority {
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public abstract class GameOperation 
{
    public List<Target> RequestTargets = new();
    protected Dictionary<string, UnitModel> filledTargets;
    public bool IsMandatory { get; set; } = false;
    public UnitModel Source { get; set; }
    
    public abstract bool Execute();
    
    public void SetTargets(Dictionary<string, UnitModel> filledTargets) 
    {
        this.filledTargets = filledTargets;
    }
    
    protected bool TryGetTarget<T>(string key, out T result) where T : UnitModel {
        if (filledTargets.TryGetValue(key, out var obj) && obj is T cast) 
        {
            result = cast;
            return true;
        }
        result = null;
        return false;
    }
    
    public bool IsReady() 
    {
        return !HasUnfilledTargets();
    }
    
    public bool HasUnfilledTargets() 
    {
        if (filledTargets == null) return false;
        bool isMismatchTargets = filledTargets.Count != RequestTargets.Count;
        bool isAnyEmpty = filledTargets.Values.Any(unit => unit == null);
        return isAnyEmpty || isMismatchTargets;
    }
    
    public void SetSource(UnitModel source) {
        Source = source;
    }
}

public class Target {
    public string Key { get; }
    public ITargetRequirement Requirement { get; }
    public UnitModel Unit { get; set; }

    public Target(string key, ITargetRequirement requirement) {
        Key = key;
        Requirement = requirement;
    }

    public bool HasTarget => Unit != null;
    public bool IsValid => Unit != null && Requirement.IsValid(Unit).IsValid;

    public T As<T>() where T : UnitModel => Unit as T;
    public bool TryGet<T>(out T result) where T : UnitModel {
        result = Unit as T;
        return result != null;
    }
}
