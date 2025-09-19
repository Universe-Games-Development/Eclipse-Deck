using Cysharp.Threading.Tasks;
using Zenject;

public abstract class VisualTask {
    [Inject] protected IUnitPresenterRegistry UnitRegistry;
    public abstract UniTask Execute();
}
