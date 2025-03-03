using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

public interface ICommand {
    public UniTask Execute();
    public UniTask Undo();
}

public abstract class Command : ICommand, IDisposable {
    private const int DEFAULT_PRIORITY = 10; // Higher value => higher priority
    public bool IsDisposed { get; private set; }
    private readonly List<Command> _childrens = new();
    public int Priority { get; protected set; }
    protected Command() {
        Priority = DEFAULT_PRIORITY;
    }

    public IReadOnlyList<Command> Children => _childrens;

    public abstract UniTask Execute();
    public abstract UniTask Undo();
    internal bool CanExecute() {
        return true;
    }

    public Command SetPriority(int priority) {
        Priority = priority;
        return this;
    }

    internal IEnumerable<Command> GetChildCommands() {
        return _childrens;
    }

    public void AddChild(Command child) => _childrens.Add(child);
    public void RemoveChild(Command child) => _childrens.Remove(child);
    public void ClearChildren() => _childrens.Clear();

    protected bool HasChilds() {
        return _childrens.Any();
    }

    public virtual void Dispose() {
        IsDisposed = true;
    }
}