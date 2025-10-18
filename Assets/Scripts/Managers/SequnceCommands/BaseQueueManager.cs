using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Zenject;

public abstract class BaseQueueManager<TTask> : IDisposable where TTask : class, IExecutableTask {
    [Inject] protected ILogger logger;

    protected PriorityQueue<Priority, TTask> _queue = new();
    protected readonly ReaderWriterLockSlim _queueLock = new();
    protected CancellationTokenSource _globalCancellationSource = new();

    private bool _isRunning;
    private TTask _currentTask;

    public event Action OnQueueEmpty;

    public int QueueCount => _queue?.Count ?? 0;
    public bool IsRunning => _isRunning;
    public TTask CurrentTask => _currentTask;

    protected abstract LogCategory LogCategory { get; }
    protected abstract string TaskTypeName { get; }

    public virtual void Dispose() {
        _globalCancellationSource?.Cancel();
        _globalCancellationSource?.Dispose();
        _queueLock?.Dispose();
        logger.LogInfo($"{GetType().Name} destroyed", LogCategory);
    }

    public void Push(TTask task, Priority priority = Priority.Normal) {
        if (task == null) {
            logger.LogError($"Cannot push a null {TaskTypeName}", LogCategory);
            return;
        }

        using (new WriteLock(_queueLock)) {
            _queue.Enqueue(priority, task);
        }

        logger.LogDebug($"{TaskTypeName} '{task}' pushed with priority {priority}. Queue size: {QueueCount}", LogCategory);
        TryStartProcessing().Forget();
    }

    public void PushRange(IEnumerable<TTask> tasks, Priority priority = Priority.Normal) {
        if (tasks == null) {
            logger.LogWarning($"Cannot push null {TaskTypeName}s collection", LogCategory);
            return;
        }

        bool hasValidTasks = false;
        int validCount = 0;
        int nullCount = 0;

        using (new WriteLock(_queueLock)) {
            foreach (var task in tasks) {
                if (task != null) {
                    _queue.Enqueue(priority, task);
                    hasValidTasks = true;
                    validCount++;
                } else {
                    nullCount++;
                }
            }
        }

        if (nullCount > 0) {
            logger.LogWarning($"Skipped {nullCount} null {TaskTypeName}s in PushRange", LogCategory);
        }

        if (hasValidTasks) {
            logger.LogDebug($"Pushed {validCount} {TaskTypeName}s with priority {priority}. Queue size: {QueueCount}", LogCategory);
            TryStartProcessing().Forget();
        } else {
            logger.LogWarning($"No valid {TaskTypeName}s to push", LogCategory);
        }
    }

    public void ClearQueue() {
        var clearedCount = ClearQueueSafe();
        logger.LogInfo($"Cleared {clearedCount} {TaskTypeName}s from queue", LogCategory);
    }

    protected int ClearQueueSafe() {
        using (new WriteLock(_queueLock)) {
            var count = _queue.Count;
            _queue.Clear();
            return count;
        }
    }

    public void CancelCurrent() {
        if (_currentTask != null && _globalCancellationSource != null) {
            logger.LogInfo($"Cancelling current {TaskTypeName}: {_currentTask}", LogCategory);
            ReplaceCancellationTokenSource();
        } else {
            logger.LogDebug($"No current {TaskTypeName} to cancel", LogCategory);
        }
    }

    public async UniTask CancelAllAsync() {
        logger.LogInfo($"Cancelling all {TaskTypeName}s...", LogCategory);
        _globalCancellationSource?.Cancel();

        var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(10), ignoreTimeScale: true);
        var completionTask = WaitForCurrentTaskCompletion();
        int completedIndex = await UniTask.WhenAny(completionTask, timeoutTask);

        if (completedIndex == 1) {
            logger.LogWarning($"Force stopping {TaskTypeName} due to timeout", LogCategory);
            _isRunning = false;
            _currentTask = null;
        }

        _globalCancellationSource?.Dispose();
        _globalCancellationSource = new CancellationTokenSource();

        var clearedCount = ClearQueueSafe();
        logger.LogInfo($"All {TaskTypeName}s cancelled. Cleared {clearedCount} queued {TaskTypeName}s", LogCategory);
    }

    private async UniTask WaitForCurrentTaskCompletion() {
        while (_currentTask != null && _isRunning) {
            await UniTask.Yield();
        }
    }

    protected void ReplaceCancellationTokenSource() {
        var oldSource = _globalCancellationSource;
        _globalCancellationSource = new CancellationTokenSource();
        oldSource.Cancel();
        oldSource.Dispose();
        logger.LogDebug("Cancellation token source replaced", LogCategory);
    }

    public List<TTask> CancelTasks(IEnumerable<TTask> tasksToRemove) {
        if (tasksToRemove == null) {
            logger.LogWarning($"Cannot cancel null {TaskTypeName}s collection", LogCategory);
            return new List<TTask>();
        }

        var tasksSet = new HashSet<TTask>(tasksToRemove);
        List<TTask> removedTasks;

        using (new WriteLock(_queueLock)) {
            removedTasks = _queue.RemoveItems(task => tasksSet.Contains(task));
        }

        if (_currentTask != null && tasksSet.Contains(_currentTask)) {
            CancelCurrent();
            removedTasks.Add(_currentTask);
        }

        if (removedTasks.Count > 0) {
            logger.LogInfo($"Cancelled {removedTasks.Count} {TaskTypeName}s", LogCategory);
        } else {
            logger.LogDebug($"No {TaskTypeName}s were cancelled (not found in queue)", LogCategory);
        }

        return removedTasks;
    }

    public List<TTask> RemoveTasks(Func<TTask, bool> predicate) {
        if (predicate == null) {
            logger.LogWarning($"Cannot remove {TaskTypeName}s with null predicate", LogCategory);
            return new List<TTask>();
        }

        using (new WriteLock(_queueLock)) {
            var removed = _queue.RemoveItems(predicate);

            if (removed.Count > 0) {
                logger.LogInfo($"Removed {removed.Count} {TaskTypeName}s by predicate", LogCategory);
            } else {
                logger.LogDebug($"No {TaskTypeName}s matched the removal predicate", LogCategory);
            }

            return removed;
        }
    }

    public bool IsQueueEmpty() => QueueCount == 0;

    public List<string> GetQueuedTaskNames() {
        using (new ReadLock(_queueLock)) {
            var tasks = _queue.GetAllItems().Select(task => task.ToString()).ToList();
            logger.LogDebug($"Retrieved {tasks.Count} queued {TaskTypeName} names", LogCategory);
            return tasks;
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void LogQueueState() {
        using (new ReadLock(_queueLock)) {
            string currentTaskStr = _currentTask != null ? _currentTask.ToString() : "None";
            var message = $"Queue State - Running: {_isRunning}, Current: {currentTaskStr}, " +
                         $"Total Queue Size: {_queue.Count}";

            logger.LogInfo(message, LogCategory);

            if (_queue.Count > 0) {
                var tasks = _queue.GetAllItems().Take(5).Select(t => t.ToString());
                logger.LogDebug($"Next {TaskTypeName}s: {string.Join(", ", tasks)}{(_queue.Count > 5 ? "..." : "")}", LogCategory);
            }
        }
    }

    private async UniTaskVoid TryStartProcessing() {
        if (_isRunning) {
            return;
        }

        _isRunning = true;
        logger.LogInfo($"Starting {TaskTypeName} processing...", LogCategory);

        int processedCount = 0;

        try {
            while (HasTasksInQueue()) {
                if (!TryDequeueTask(out _currentTask))
                    break;

                logger.LogInfo($"Processing {TaskTypeName} [{processedCount + 1}]: {_currentTask}", LogCategory);

                try {
                    var startTime = DateTime.UtcNow;
                    var result = await ProcessTaskAsync(_currentTask, _globalCancellationSource.Token);
                    var duration = DateTime.UtcNow - startTime;

                    if (!result.IsSuccess) {
                        logger.LogWarning($"{_currentTask} Fail: {result.Message}, (Duration: {duration.TotalMilliseconds:F1}ms)", LogCategory);
                    } else {
                        logger.LogInfo($"{_currentTask} Success, (Duration: {duration.TotalMilliseconds:F1}ms)", LogCategory);
                    }

                    OnTaskCompleted(_currentTask, result);
                    processedCount++;
                } catch (OperationCanceledException) {
                    logger.LogInfo($"{TaskTypeName} cancelled: {_currentTask}", LogCategory);
                } finally {
                    _currentTask = null;
                }
            }

            logger.LogInfo($"Queue processing completed. Processed {processedCount} {TaskTypeName}s", LogCategory);
            OnQueueEmpty?.Invoke();
        } finally {
            _isRunning = false;
            _currentTask = null;
            logger.LogDebug($"{TaskTypeName} processing finished", LogCategory);
        }
    }

    private bool HasTasksInQueue() {
        using (new ReadLock(_queueLock)) {
            return !_queue.IsEmpty();
        }
    }

    private bool TryDequeueTask(out TTask task) {
        using (new WriteLock(_queueLock)) {
            var result = _queue.TryDequeue(out task);
            if (result) {
                logger.LogDebug($"Dequeued {TaskTypeName}: {task}. Remaining: {_queue.Count}", LogCategory);
            }
            return result;
        }
    }

    protected abstract UniTask<OperationResult> ProcessTaskAsync(TTask task, CancellationToken cancellationToken);

    protected virtual void OnTaskCompleted(TTask task, OperationResult result) {
        // Можна перевизначити в нащадках
    }
}