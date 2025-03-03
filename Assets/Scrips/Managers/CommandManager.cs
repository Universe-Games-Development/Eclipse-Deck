using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

public enum ExecutionMode {
    Manual, // Manual call by ExecuteCommands
    Auto    // Automatic executing
}

public class CommandManager {
    public ExecutionMode Mode = ExecutionMode.Auto;
    private int _executionFlag = 0;

    // Заміна ConcurrentQueue на PriorityQueue для підтримки пріоритетів
    private readonly PriorityQueue<int, Command> _commandQueue = new(Comparer<int>.Default);
    private readonly object _queueLock = new();
    private const int MaxUndoAmount = 10;
    private readonly Stack<Command> _undoStack = new();

    internal void EnqueueCommands(List<Command> commands) {
        foreach (var command in commands) {
            EnqueueCommand(command);
        }
    }

    public void EnqueueCommand(Command command) {
        if (!ValidateCommand(command)) Debug.LogError("Command didn't pass validation");
        lock (_queueLock) {
            _commandQueue.Enqueue(command.Priority, command);
        }

        // If it's Auto mode, try to execute commands.
        if (Mode == ExecutionMode.Auto) {
            TryExecuteCommands().Forget();
        }
    }

    public async UniTask TryExecuteCommands() {
        if (Interlocked.CompareExchange(ref _executionFlag, 1, 0) != 0)
            return;

        try {
            while (_commandQueue.Count > 0) {
                Command cmd;
                lock (_queueLock) {
                    cmd = _commandQueue.Dequeue();
                    if (cmd == null) {
                        Debug.LogWarning("TryExecuteCommands : Failed dequeue command. Command is null ");
                        continue;
                    }
                }
                
                await ExecuteCommandRecursively(cmd);
                CleanupUndoCommands();
            }
        } finally {
            Interlocked.Exchange(ref _executionFlag, 0);
        }
    }

    private async UniTask ExecuteCommandRecursively(Command command) {
        try {
            if (!command.CanExecute()) {
                Debug.LogWarning($"Command {command} validation failed");
                return;
            }

            await command.Execute(); // Оновіть інтерфейс Command
            StoreUndoCommand(command);

            foreach (var child in command.GetChildCommands()) {
                await ExecuteCommandRecursively(child);
            }
        } catch (Exception ex) {
            Debug.LogError($"Command failed: {command} - {ex}");
        }
    }

    
    public async UniTask UndoLastCommand() {
        if (_undoStack.Count > 0) {
            var command = _undoStack.Pop();
            await command.Undo();
        }
    }

    public async UniTask UndoAllCommands() {
        while (_undoStack.Count > 0) {
            var command = _undoStack.Pop();
            await command.Undo();
        }
    }

    private void StoreUndoCommand(Command cmd) {
        _undoStack.Push(cmd);
    }

    private void CleanupUndoCommands() {
        while (_undoStack.Count > MaxUndoAmount) {
            _undoStack.Pop();
        }
    }

    private bool ValidateCommand(Command cmd) {
        return cmd != null && !cmd.IsDisposed && cmd.CanExecute();
    }
}

public class PriorityQueue<TKey, TValue> {
    private readonly SortedDictionary<TKey, Queue<TValue>> _queue = new();
    private readonly IComparer<TKey> _keyComparer;

    public int Count { get { return _queue.Count; } }

    public PriorityQueue(IComparer<TKey> keyComparer = null) {
        _keyComparer = keyComparer ?? Comparer<TKey>.Default;
        _queue = new SortedDictionary<TKey, Queue<TValue>>(
            Comparer<TKey>.Create((x, y) => _keyComparer.Compare(y, x))
        );
    }

    // Enqueue an element with priority
    public void Enqueue(TKey priority, TValue value) {
        if (!_queue.ContainsKey(priority)) {
            _queue[priority] = new Queue<TValue>();
        }
        _queue[priority].Enqueue(value);
    }

    // Dequeue element with highest priority
    public TValue Dequeue() {
        if (_queue.Count == 0) {
            throw new InvalidOperationException("The queue is empty.");
        }

        var maxPriority = GetMinKey();
        var dequeuedValue = _queue[maxPriority].Dequeue();

        // Remove the key if the queue is empty for that priority
        if (_queue[maxPriority].Count == 0) {
            _queue.Remove(maxPriority);
        }

        return dequeuedValue;
    }

    // View the element with highest priority without dequeuing
    public TValue Peek() {
        if (_queue.Count == 0) {
            throw new InvalidOperationException("The queue is empty.");
        }

        var maxPriority = GetMinKey();
        return _queue[maxPriority].Peek();
    }

    private TKey GetMinKey() {
        return _queue.Keys.First();
    }

}

public class IntComparer : IComparer<int> {
    public int Compare(int x, int y) => x < y ? -1 : (x > y ? 1 : 0);
}
