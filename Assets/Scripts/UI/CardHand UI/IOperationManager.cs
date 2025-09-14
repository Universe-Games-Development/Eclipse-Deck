using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public interface IOperationManager {
    GameOperation CurrentOperation { get; }
    bool IsRunning { get; }
    int QueueCount { get; }

    event Action<GameOperation, OperationStatus> OnOperationStatus;
    event Action OnQueueEmpty;

    UniTask CancelAllAsync();
    void CancelCurrent();
    List<GameOperation> CancelOperations(IEnumerable<GameOperation> operationsToRemove);
    void ClearQueue();
    List<string> GetQueuedOperationNames();
    bool IsQueueEmpty();
    void Push(GameOperation operation, Priority priority = Priority.Normal);
    void PushRange(IEnumerable<GameOperation> operations, Priority priority = Priority.Normal);
    List<GameOperation> RemoveOperations(Func<GameOperation, bool> predicate);
}