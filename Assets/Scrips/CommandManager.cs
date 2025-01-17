using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class CommandManager {
    private List<ICommand> _commandQueue = new();
    private List<ICommand> _executedCommands = new();

    private const int MaxExecutedCommands = 10;

    public void RegisterCommand(ICommand command) {
        if (ValidateCommand(command)) {
            _commandQueue.Add(command);
        }
    }

    public async UniTask ExecuteCommands() {
        foreach (var command in _commandQueue) {
            await command.Execute();
            _executedCommands.Add(command);
        }
        _commandQueue.Clear();
        CleanupExecutedCommands();
    }

    public async UniTask UndoLastCommand() {
        if (_executedCommands.Count > 0) {
            var lastCommand = _executedCommands[_executedCommands.Count - 1];
            await lastCommand.Undo();
            _executedCommands.RemoveAt(_executedCommands.Count - 1);
        }
    }

    public async UniTask UndoAllCommands() {
        while (_executedCommands.Count > 0) {
            var command = _executedCommands[_executedCommands.Count - 1];
            await command.Undo();
            _executedCommands.RemoveAt(_executedCommands.Count - 1);
        }
    }

    private void CleanupExecutedCommands() {
        if (_executedCommands.Count > MaxExecutedCommands) {
            int excess = _executedCommands.Count - MaxExecutedCommands;
            _executedCommands.RemoveRange(0, excess);
        }
    }

    private bool ValidateCommand(ICommand command) {
        return true;
    }
}
