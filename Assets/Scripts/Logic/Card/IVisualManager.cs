using System;

public interface IVisualManager {
    VisualTask CurrentTask { get; }
    bool IsRunning { get; }
    int QueueCount { get; }

    event Action OnQueueEmpty;

    void Push(VisualTask task, Priority priority = Priority.Normal);
}