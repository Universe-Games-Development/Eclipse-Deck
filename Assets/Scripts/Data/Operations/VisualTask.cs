using Cysharp.Threading.Tasks;
using Zenject;

public abstract class VisualTask {
    [Inject] protected IUnitRegistry UnitRegistry;
    public abstract UniTask<bool> Execute();
}
