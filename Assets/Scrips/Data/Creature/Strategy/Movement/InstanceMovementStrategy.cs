using Cysharp.Threading.Tasks;

public abstract class InstanceMovementStrategy : IMoveStrategy {

    protected CreatureNavigator navigator;

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
