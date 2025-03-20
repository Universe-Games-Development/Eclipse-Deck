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

        var maxPriority = GetMaxKey();
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

        var maxPriority = GetMaxKey();
        return _queue[maxPriority].Peek();
    }

    private TKey GetMaxKey() => _queue.Keys.Max();


    public void Clear() {
        _queue.Clear();
    }

    public bool IsEmpty() {
        return _queue.IsEmpty();
    }
}

public static class CommandPriority {
    public const int High = 100;
    public const int Medium = 50;
    public const int Low = 10;
}
