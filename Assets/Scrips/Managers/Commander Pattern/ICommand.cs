using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

public interface ICommand {
    public UniTask Execute();
    public UniTask Undo();
}

public abstract class Command : ICommand {
    private readonly List<Command> _children = new();
    public IReadOnlyList<Command> Children => _children;

    public void AddChild(Command child) => _children.Add(child);
    public void RemoveChild(Command child) => _children.Remove(child);
    public void ClearChildren() => _children.Clear();

    protected bool HasChild() {
        throw new NotImplementedException();
    }
    public abstract UniTask Execute();
    public abstract UniTask Undo();

    protected async UniTask ExecuteChildren() {
        foreach (var child in _children) {
            await child.Execute();
        }
    }

    protected async UniTask UndoChildren() {
        foreach (var child in Enumerable.Reverse(_children)) {
            await child.Undo();
        }
    }
}