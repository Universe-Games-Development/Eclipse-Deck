using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

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
