using Cysharp.Threading.Tasks;
using Zenject;

public abstract class VisualTask : IExecutableTask {
    public abstract UniTask<bool> Execute();
}
