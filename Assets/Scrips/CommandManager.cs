using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

public class CommandManager {
    private readonly ConcurrentQueue<ICommand> _commandQueue = new();
    private readonly Stack<ICommand> _undoStack = new();
    private readonly SemaphoreSlim _executionSemaphore = new(1);

    private const int MaxExecutedCommands = 10;

    public void RegisterCommand(ICommand command) {
        if (ValidateCommand(command)) {
            _commandQueue.Enqueue(command);
        }
    }

    public async UniTask ExecuteCommands() {
        await _executionSemaphore.WaitAsync();
        try {
            ICommand command;
            while (_commandQueue.TryDequeue(out command)) {
                await command.Execute();
                _undoStack.Push(command);
            }
            CleanupExecutedCommands(); // Відновлення команди
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

    private void CleanupExecutedCommands() {
        if (_undoStack.Count > MaxExecutedCommands) {
            int excess = _undoStack.Count - MaxExecutedCommands;
            for (int i = 0; i < excess; i++) {
                _undoStack.Pop(); // Видаляємо зайві виконані команди
            }
        }
    }

    private bool ValidateCommand(ICommand command) {
        // Більш специфічна перевірка на допустимість команди
        return command != null;
    }
}
