using Cysharp.Threading.Tasks;
using ModestTree;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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
