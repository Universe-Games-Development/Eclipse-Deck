using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

public enum ExecutionMode {
    Manual, // Manual call by ExecuteCommands
    Auto    // Automatic executing
}

public class CommandManager {
    public ExecutionMode Mode = ExecutionMode.Auto;
    private bool _isExecuting = false;

    private readonly SemaphoreSlim _executionSemaphore = new(1);
    private readonly ConcurrentQueue<ICommand> _commandQueue = new();

    private const int MaxUndoAmount = 10;
    private readonly Stack<ICommand> _undoStack = new();

    internal void EnqueueCommands(List<ICommand> commands) {
        foreach (var command in commands) {
            EnqueueCommand(command);
        }
    }

    public void EnqueueCommand(ICommand command) {
        if (!ValidateCommand(command)) {
            Debug.Log("Command didn't pass validation");
            return;
        }

        _commandQueue.Enqueue(command);

        // If it's Auto mode, try to execute commands.
        if (Mode == ExecutionMode.Auto) {
            TryExecuteCommands();
        }
    }

    private async void TryExecuteCommands() {
        if (_isExecuting || Mode == ExecutionMode.Manual) return;

        // Start executing commands
        _isExecuting = true;
        await ExecuteCommands();
        _isExecuting = false;
    }

    public async UniTask ExecuteCommands() {
        await _executionSemaphore.WaitAsync();
        try {
            // Continue processing commands as they arrive
            while (_commandQueue.Count > 0) {
                if (_commandQueue.TryDequeue(out var command)) {
                    await command.Execute();
                    _undoStack.Push(command);
                }
            }
            CleanupUndoCommands();
        } finally {
            _executionSemaphore.Release();
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

    private void CleanupUndoCommands() {
        while (_undoStack.Count > MaxUndoAmount) {
            _undoStack.Pop();
        }
    }

    protected virtual bool ValidateCommand(ICommand command) {
        return command != null;
    }
}
