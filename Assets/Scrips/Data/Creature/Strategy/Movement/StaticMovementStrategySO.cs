using Cysharp.Threading.Tasks;
using Zenject;

public abstract class StaticMovementStrategySO : MovementStrategySO, IMoveStrategy {
    protected CreatureNavigator navigator;
    private DiContainer diContainer;
    public override IMoveStrategy GetInstance() {
        return this;
    }

    protected abstract UniTask<int> Move();

    public async UniTask<int> Movement(GameContext gameContext) {
        if (navigator != null) {
            navigator.UpdateParams(gameContext);
        } else {
            navigator = new CreatureNavigator(gameContext);
        }
        return await Move();
    }
}
