using System.Collections.Generic;
using Zenject;

public abstract class MovementStrategy : IMoveStrategy {
    protected CreatureNavigator navigator;
    public abstract List<Path> CalculatePath(Field currentField);
    public void Initialize(CreatureNavigator navigator) {
        this.navigator = navigator;
    }
}