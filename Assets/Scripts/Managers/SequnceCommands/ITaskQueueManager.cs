using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public interface ITaskQueueManager<TTask> where TTask : class, IExecutableTask {
    TTask CurrentTask { get; }
    bool IsRunning { get; }
    int QueueCount { get; }

    event Action OnQueueEmpty;

    UniTask CancelAllAsync();
    void CancelCurrent();
    List<TTask> CancelTasks(IEnumerable<TTask> tasksToRemove);
    int ClearQueue();
    void Dispose();
    UniTask<ExecutionResult> ExecuteAsync(TTask task, Priority priority = Priority.Normal);
    List<string> GetQueuedTaskNames();
    bool IsQueueEmpty();
    void LogQueueState();
    void Push(TTask task, Priority priority = Priority.Normal);
    void PushRange(IEnumerable<TTask> tasks, Priority priority = Priority.Normal);
    List<TTask> RemoveTasks(Func<TTask, bool> predicate);
}