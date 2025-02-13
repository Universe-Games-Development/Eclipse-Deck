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
    private readonly ConcurrentQueue<Command> _commandQueue = new();

    private const int MaxUndoAmount = 10;
    private readonly Stack<Command> _undoStack = new();

    private async void TryExecuteCommands() {
        if (_isExecuting || Mode == ExecutionMode.Manual) return;

        // Start executing commands
        _isExecuting = true;
        await ExecuteCommands();
        _isExecuting = false;
    }

    public async UniTask ExecuteCommands() {
        while (_commandQueue.TryDequeue(out var command)) {
            await ExecuteCommandRecursively(command);
        }
        CleanupUndoCommands();
    }

    private async UniTask ExecuteCommandRecursively(Command rootCommand) {
        Stack<Command> stack = new();
        stack.Push(rootCommand);

        while (stack.Count > 0) {
            var command = stack.Pop();

            foreach (var child in command.Children) {
                stack.Push(child);
            }

            await command.Execute();
            _undoStack.Push(command);
        }
    }

    internal void EnqueueCommands(List<Command> commands) {
        foreach (var command in commands) {
            EnqueueCommand(command);
        }
    }

    public void EnqueueCommand(Command command) {
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
