using System.Collections.Generic;

public abstract class StaticMovementStrategySO : MovementStrategySO, IMoveStrategy {
    protected CreatureNavigator navigator;
    public override IMoveStrategy GetInstance() {
        return this; 
    }

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
