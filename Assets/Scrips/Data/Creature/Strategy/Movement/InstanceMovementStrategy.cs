using System.Collections.Generic;
using Zenject;

public abstract class InstanceMovementStrategy : IMoveStrategy {
    [Inject] protected CreatureNavigator navigator;

    public abstract List<Path> CalculatePath(Field currentField);
}