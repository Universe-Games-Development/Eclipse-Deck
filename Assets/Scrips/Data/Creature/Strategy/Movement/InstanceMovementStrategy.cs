using System.Collections.Generic;

public abstract class InstanceMovementStrategy : IMoveStrategy {

    protected CreatureNavigator navigator;

    protected abstract List<Path> Move();

    public List<Path> CalculatePath(GameContext gameContext) {
        if (navigator != null) {
            navigator.UpdateParams(gameContext);
        } else {
            navigator = new CreatureNavigator(gameContext);
        }
        return Move();
    }
}
