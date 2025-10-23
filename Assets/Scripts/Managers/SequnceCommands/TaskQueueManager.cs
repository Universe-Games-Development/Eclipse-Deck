using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Zenject;

public enum Priority {
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public enum ExecutionStatus {
    Success,
    Failure,
    Canceled
}

public struct ExecutionResult {
    public ExecutionStatus Status { get; }
    public bool IsSuccess => Status == ExecutionStatus.Success;
    public bool Cancelled => Status == ExecutionStatus.Canceled;
    public bool Failed => Status == ExecutionStatus.Failure;

    const string cancelledMessage = "Operation was cancelled";
    public string Message { get; }

    private ExecutionResult(ExecutionStatus status, string resultMessage = null) {
        Status = status;
        Message = resultMessage;
    }

    public static ExecutionResult Success() => new(ExecutionStatus.Success);
    public static ExecutionResult Failure(string errorMessage = null) => new(ExecutionStatus.Failure, errorMessage);
    public static ExecutionResult Canceled() => new(ExecutionStatus.Canceled, cancelledMessage);

    public static implicit operator bool(ExecutionResult result) => result.Status == ExecutionStatus.Success;
}

public interface IExecutableTask {
    UniTask<ExecutionResult> ExecuteAsync();
}

public abstract class TaskQueueManager<TTask> : IDisposable, ITaskQueueManager<TTask>
    where TTask : class, IExecutableTask {
    private const int DEFAULT_CANCELLATION_TIMEOUT_MS = 10000;
    private const int MAX_LOGGED_TASKS = 5;

    [Inject]
    protected ILogger Logger { get; set; }

    protected CancellationTokenSource GlobalCancellationSource { get; private set; } = new();
    protected ConcurrentPriorityQueue<Priority, TTask> Queue { get; } = new();

    private readonly ConcurrentDictionary<TTask, UniTaskCompletionSource<ExecutionResult>> _pendingExecutions = new();
    private readonly object _processingLock = new();

    private bool _isRunning;
    private TTask _currentTask;

    public event Action OnQueueEmpty;

    public int QueueCount => Queue?.Count ?? 0;
    public bool IsRunning => _isRunning;
    public TTask CurrentTask => _currentTask;

    protected abstract LogCategory LogCategory { get; }
    protected abstract string TaskTypeName { get; }

    /// <summary>
    /// Executes a task asynchronously and waits for the result
    /// </summary>
    public async UniTask<ExecutionResult> ExecuteAsync(TTask task, Priority priority = Priority.Normal) {
        if (task == null) {
            Logger.LogError($"Cannot execute null {TaskTypeName}", LogCategory);
            return ExecutionResult.Failure("Task is null");
        }

        var completionSource = new UniTaskCompletionSource<ExecutionResult>();
        var actualSource = _pendingExecutions.GetOrAdd(task, completionSource);

        // If task is already pending, await existing execution
        if (actualSource != completionSource) {
            Logger.LogDebug($"{TaskTypeName} '{task}' already pending, awaiting existing execution", LogCategory);
            return await actualSource.Task;
        }

        // We're the first to add this task
        Push(task, priority);
        Logger.LogDebug($"Awaiting execution of {TaskTypeName}: {task}", LogCategory);

        try {
            var result = await actualSource.Task;
            Logger.LogDebug($"{TaskTypeName} '{task}' completed with status: {result.Status}", LogCategory);
            return result;
        } finally {
            _pendingExecutions.TryRemove(task, out _);
        }
    }

    public void Push(TTask task, Priority priority = Priority.Normal) {
        if (task == null) {
            Logger.LogError($"Cannot push a null {TaskTypeName}", LogCategory);
            return;
        }

        Queue.Enqueue(priority, task);
        Logger.LogDebug($"{TaskTypeName} '{task}' pushed with priority {priority}. Queue size: {QueueCount}", LogCategory);

        if (!_isRunning) {
            TryStartProcessing().Forget();
        }
    }

    public void PushRange(IEnumerable<TTask> tasks, Priority priority = Priority.Normal) {
        if (tasks == null) {
            Logger.LogWarning($"Cannot push null {TaskTypeName}s collection", LogCategory);
            return;
        }

        int validCount = 0;
        int nullCount = 0;

        foreach (var task in tasks) {
            if (task != null) {
                Queue.Enqueue(priority, task);
                validCount++;
            } else {
                nullCount++;
            }
        }

        if (nullCount > 0) {
            Logger.LogWarning($"Skipped {nullCount} null {TaskTypeName}s in PushRange", LogCategory);
        }

        if (validCount > 0) {
            Logger.LogDebug($"Pushed {validCount} {TaskTypeName}s with priority {priority}. Queue size: {QueueCount}", LogCategory);
            TryStartProcessing().Forget();
        } else {
            Logger.LogWarning($"No valid {TaskTypeName}s to push", LogCategory);
        }
    }

    public int ClearQueue() {
        var clearedCount = Queue.Count;
        Queue.Clear();

        // Cancel all pending operations for removed tasks
        foreach (var (task, completionSource) in _pendingExecutions) {
            if (task != _currentTask) {
                completionSource.TrySetResult(ExecutionResult.Canceled());
            }
        }

        _pendingExecutions.Clear();

        Logger.LogInfo($"Cleared {clearedCount} {TaskTypeName}s from queue", LogCategory);
        return clearedCount;
    }

    public void CancelCurrent() {
        if (_currentTask != null && GlobalCancellationSource != null) {
            Logger.LogInfo($"Cancelling current {TaskTypeName}: {_currentTask}", LogCategory);
            ReplaceCancellationTokenSource();
        } else {
            Logger.LogDebug($"No current {TaskTypeName} to cancel", LogCategory);
        }
    }

    public async UniTask CancelAllAsync() {
        Logger.LogInfo($"Cancelling all {TaskTypeName}s...", LogCategory);

        // Cancel all pending operations
        foreach (var completionSource in _pendingExecutions.Values) {
            completionSource.TrySetResult(ExecutionResult.Canceled());
        }
        _pendingExecutions.Clear();

        GlobalCancellationSource?.Cancel();

        var timeoutTask = UniTask.Delay(TimeSpan.FromMilliseconds(DEFAULT_CANCELLATION_TIMEOUT_MS), ignoreTimeScale: true);
        var completionTask = WaitForCurrentTaskCompletion();

        int completedIndex = await UniTask.WhenAny(completionTask, timeoutTask);

        if (completedIndex == 1) {
            Logger.LogWarning($"Force stopping {TaskTypeName} due to timeout", LogCategory);
            _isRunning = false;
            _currentTask = null;
        }

        GlobalCancellationSource?.Dispose();
        GlobalCancellationSource = new CancellationTokenSource();

        var clearedCount = ClearQueue();
        Logger.LogInfo($"All {TaskTypeName}s cancelled. Cleared {clearedCount} queued {TaskTypeName}s", LogCategory);
    }

    private async UniTask WaitForCurrentTaskCompletion() {
        await UniTask.WaitUntil(() => !_isRunning || _currentTask == null);
    }

    protected void ReplaceCancellationTokenSource() {
        var oldSource = GlobalCancellationSource;
        GlobalCancellationSource = new CancellationTokenSource();

        oldSource.Cancel();
        oldSource.Dispose();

        Logger.LogDebug("Cancellation token source replaced", LogCategory);
    }

    public List<TTask> CancelTasks(IEnumerable<TTask> tasksToRemove) {
        if (tasksToRemove == null) {
            Logger.LogWarning($"Cannot cancel null {TaskTypeName}s collection", LogCategory);
            return new List<TTask>();
        }

        var tasksSet = new HashSet<TTask>(tasksToRemove);
        var removedTasks = Queue.RemoveItems(task => tasksSet.Contains(task));

        // Handle pending operations for cancelled tasks
        foreach (var task in tasksSet) {
            if (_pendingExecutions.TryRemove(task, out var completionSource)) {
                completionSource.TrySetResult(ExecutionResult.Canceled());
            }
        }

        if (_currentTask != null && tasksSet.Contains(_currentTask)) {
            CancelCurrent();
            removedTasks.Add(_currentTask);
        }

        if (removedTasks.Count > 0) {
            Logger.LogInfo($"Cancelled {removedTasks.Count} {TaskTypeName}s", LogCategory);
        } else {
            Logger.LogDebug($"No {TaskTypeName}s were cancelled (not found in queue)", LogCategory);
        }

        return removedTasks;
    }

    public List<TTask> RemoveTasks(Func<TTask, bool> predicate) {
        if (predicate == null) {
            Logger.LogWarning($"Cannot remove {TaskTypeName}s with null predicate", LogCategory);
            return new List<TTask>();
        }

        var removedTasks = Queue.RemoveItems(predicate);

        // Cancel pending operations for removed tasks
        foreach (var task in removedTasks) {
            if (_pendingExecutions.TryRemove(task, out var completionSource)) {
                completionSource.TrySetResult(ExecutionResult.Canceled());
            }
        }

        if (removedTasks.Count > 0) {
            Logger.LogInfo($"Removed {removedTasks.Count} {TaskTypeName}s by predicate", LogCategory);
        } else {
            Logger.LogDebug($"No {TaskTypeName}s matched the removal predicate", LogCategory);
        }

        return removedTasks;
    }

    public bool IsQueueEmpty() => QueueCount == 0;

    public List<string> GetQueuedTaskNames() {
        var tasks = Queue.GetAllItems()
            .Select(task => task.ToString())
            .ToList();

        Logger.LogDebug($"Retrieved {tasks.Count} queued {TaskTypeName} names", LogCategory);
        return tasks;
    }

    private async UniTaskVoid TryStartProcessing() {
        lock (_processingLock) {
            if (_isRunning) {
                return;
            }
            _isRunning = true;
        }

        Logger.LogInfo($"Starting {TaskTypeName} processing...", LogCategory);
        int processedCount = 0;

        try {
            while (HasTasksInQueue()) {
                if (!TryDequeueTask(out _currentTask)) {
                    break;
                }

                Logger.LogInfo($"Processing {TaskTypeName} [{processedCount + 1}]: {_currentTask}", LogCategory);

                ExecutionResult result = ExecutionResult.Failure("Not started");

                try {
                    var startTime = DateTime.UtcNow;
                    result = await ProcessTaskAsync(_currentTask, GlobalCancellationSource.Token);
                    var duration = DateTime.UtcNow - startTime;

                    if (!result.IsSuccess) {
                        Logger.LogWarning($"{_currentTask} Fail: {result.Message}, (Duration: {duration.TotalMilliseconds:F1}ms)", LogCategory);
                    } else {
                        Logger.LogInfo($"{_currentTask} Success, (Duration: {duration.TotalMilliseconds:F1}ms)", LogCategory);
                    }

                    OnTaskCompleted(_currentTask, result);
                    processedCount++;
                } catch (OperationCanceledException) {
                    Logger.LogInfo($"{TaskTypeName} cancelled: {_currentTask}", LogCategory);
                    result = ExecutionResult.Canceled();
                } finally {
                    // Notify awaiter about completion
                    if (_pendingExecutions.TryRemove(_currentTask, out var completionSource)) {
                        completionSource.TrySetResult(result);
                    }
                    _currentTask = null;
                }
            }

            Logger.LogInfo($"Queue processing completed. Processed {processedCount} {TaskTypeName}s", LogCategory);
            OnQueueEmpty?.Invoke();
        } finally {
            _isRunning = false;
            _currentTask = null;
            Logger.LogDebug($"{TaskTypeName} processing finished", LogCategory);
        }
    }

    private bool HasTasksInQueue() => !Queue.IsEmpty;

    private bool TryDequeueTask(out TTask task) {
        var result = Queue.TryDequeue(out task);
        if (result) {
            Logger.LogDebug($"Dequeued {TaskTypeName}: {task}. Remaining: {Queue.Count}", LogCategory);
        }
        return result;
    }

    protected virtual async UniTask<ExecutionResult> ProcessTaskAsync(TTask task, CancellationToken cancellationToken) {
        try {
            Logger.LogInfo($"Beginning operation: {task}", LogCategory);
            ExecutionResult result = await task.ExecuteAsync();
            return result;
        } catch (OperationCanceledException) {
            Logger.LogInfo($"Task {task} was cancelled", LogCategory);
            throw;
        }
    }

    protected virtual void OnTaskCompleted(TTask task, ExecutionResult result) {
        // Can be overridden in descendants for additional logic
    }

    public void LogQueueState() {
        string currentTaskStr = _currentTask?.ToString() ?? "None";
        int pendingCount = _pendingExecutions.Count;

        var message = $"Queue State - Running: {_isRunning}, Current: {currentTaskStr}, " +
                      $"Queue Size: {Queue.Count}, Pending Awaiters: {pendingCount}";

        Logger.LogInfo(message, LogCategory);

        if (Queue.Count > 0) {
            var tasks = Queue.GetAllItems()
                .Take(MAX_LOGGED_TASKS)
                .Select(t => t.ToString());

            Logger.LogDebug($"Next {TaskTypeName}s: {string.Join(", ", tasks)}{(Queue.Count > MAX_LOGGED_TASKS ? "..." : "")}", LogCategory);
        }
    }

    public virtual void Dispose() {
        // Cancel all pending operations
        foreach (var completionSource in _pendingExecutions.Values) {
            completionSource.TrySetResult(ExecutionResult.Canceled());
        }
        _pendingExecutions.Clear();

        GlobalCancellationSource?.Cancel();
        GlobalCancellationSource?.Dispose();

        Logger.LogInfo($"{GetType().Name} destroyed", LogCategory);
    }
}
