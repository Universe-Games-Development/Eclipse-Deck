using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;
using ModestTree;

public class CommandManager {
    private int _executionFlag = 0;

    private readonly PriorityQueue<int, Command> _commandQueue = new(Comparer<int>.Default);
    private readonly LinkedList<Command> _undoList = new();

    private readonly object _queueLock = new();
    private readonly object _undoLock = new();
    private const int MaxUndoAmount = 10;

    private bool _isPaused = false;
    private readonly ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);

    public void Pause() {
        if (!_isPaused) {
            _isPaused = true;
            _pauseEvent.Reset(); // Установить в несигнальное состояние
        }
    }

    public void Resume() {
        if (_isPaused) {
            _isPaused = false;
            _pauseEvent.Set(); // Установить в сигнальное состояние
        }
    }

    internal void EnqueueCommands(List<Command> commands) {
        foreach (var command in commands) {
            EnqueueCommand(command);
        }
    }

    public void EnqueueCommand(Command command) {
        if (!ValidateCommand(command)) {
            Debug.LogError("Command didn't pass validation");
            return;
        }

        lock (_queueLock) {
            _commandQueue.Enqueue(command.Priority, command);
        }

        TryExecuteCommands().Forget();
    }

    public async UniTask TryExecuteCommands() {
        if (Interlocked.CompareExchange(ref _executionFlag, 1, 0) != 0)
            return;

        try {
            while (_commandQueue.Count > 0) {
                // Проверяем состояние паузы
                if (_isPaused) {
                    await UniTask.WaitUntil(() => !_isPaused);
                }

                Command cmd;
                lock (_queueLock) {
                    cmd = _commandQueue.Dequeue();
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
            

            foreach (var child in command.GetChildCommands()) {
                await ExecuteCommandRecursively(child);
            }

            StoreUndoCommand(command);
        } catch (Exception ex) {
            Debug.LogError($"Command failed: {command} - {ex}");
        }
    }


    public async UniTask UndoLastCommand() {
        Command command = null;
        lock (_undoLock) {
            if (_undoList.Count == 0) return;
            command = _undoList.Last.Value;
            _undoList.RemoveLast();
        }
        try {
            await command.Undo();
        } catch (Exception ex) {
            Debug.LogError($"Undo failed: {command} - {ex}");
        }
    }

    public async UniTask UndoAllCommands() {
        while (!_undoList.IsEmpty()) {
            await UndoLastCommand();
        }
    }

    private void StoreUndoCommand(Command cmd) {
        lock (_undoLock) {
            _undoList.AddLast(cmd);
            CleanupUndoCommands();
        }
    }

    private void CleanupUndoCommands() {
        lock (_undoLock) {
            while (_undoList.Count > MaxUndoAmount) {
                _undoList.RemoveFirst();
            }
        }
    }

    private bool ValidateCommand(Command cmd) {
        return cmd != null && !cmd.IsDisposed && cmd.CanExecute();
    }

    public bool HasPendingCommands() {
        lock (_queueLock) {
            return !_commandQueue.IsEmpty();
        }
    }

}

public class PriorityQueue<TKey, TValue> {
    private readonly SortedDictionary<TKey, Queue<TValue>> _queue = new();
    private readonly IComparer<TKey> _keyComparer;

    public int Count {
        get {
            int count = 0;
            foreach (var queue in _queue.Values) {
                count += queue.Count;
            }
            return count;
        }
    }

    public PriorityQueue(IComparer<TKey> keyComparer = null) {
        _keyComparer = keyComparer ?? Comparer<TKey>.Default;
        _queue = new SortedDictionary<TKey, Queue<TValue>>(
            Comparer<TKey>.Create((x, y) => _keyComparer.Compare(y, x)) // Reverse for highest priority first
        );
    }

    public void Enqueue(TKey priority, TValue value) {
        if (!_queue.ContainsKey(priority)) {
            _queue[priority] = new Queue<TValue>();
        }
        _queue[priority].Enqueue(value);
    }

    public TValue Dequeue() {
        if (_queue.Count == 0) {
            throw new InvalidOperationException("The queue is empty.");
        }

        var maxPriority = _queue.Keys.First(); // Already sorted in descending order
        var dequeuedValue = _queue[maxPriority].Dequeue();

        if (_queue[maxPriority].Count == 0) {
            _queue.Remove(maxPriority);
        }

        return dequeuedValue;
    }

    public TValue Peek() {
        if (_queue.Count == 0) {
            throw new InvalidOperationException("The queue is empty.");
        }

        var maxPriority = _queue.Keys.First();
        return _queue[maxPriority].Peek();
    }

    public void Clear() {
        _queue.Clear();
    }

    public bool IsEmpty() {
        return _queue.Count == 0;
    }

    public bool TryDequeue(out TValue value) {
        if (IsEmpty()) {
            value = default(TValue);
            return false;
        }

        value = Dequeue();
        return true;
    }

    public IEnumerable<TValue> GetAllItems() {
        foreach (var kvp in _queue) {
            foreach (var item in kvp.Value) {
                yield return item;
            }
        }
    }

    public List<TValue> RemoveItems(Func<TValue, bool> predicate) {
        var removedItems = new List<TValue>();
        var keysToRemove = new List<TKey>();

        foreach (var kvp in _queue.ToList()) {
            var priority = kvp.Key;
            var queue = kvp.Value;
            var tempItems = new List<TValue>();

            while (queue.Count > 0) {
                var item = queue.Dequeue();
                if (predicate(item)) {
                    removedItems.Add(item);
                } else {
                    tempItems.Add(item);
                }
            }

            // Re-enqueue items that weren't removed
            foreach (var item in tempItems) {
                queue.Enqueue(item);
            }

            // Mark empty queues for removal
            if (queue.Count == 0) {
                keysToRemove.Add(priority);
            }
        }

        // Remove empty priority queues
        foreach (var key in keysToRemove) {
            _queue.Remove(key);
        }

        return removedItems;
    }
}
