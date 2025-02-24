using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureMovementData", menuName = "Behaviour/MovementBehaviour")]
public class CreatureMovementData : ScriptableObject {
    [Header("UI")]
    public string Name;
    public Sprite strategyIcon;
    [Header("Attack Strategy")]
    public MovementStrategyProvider attackStrategy;
    [Header("Support Strategy")]
    public MovementStrategyProvider supportStrategy;
}

public abstract class MovementStrategyProvider : ScriptableObject {
    public abstract MovementStrategy GetInstance();
}

public interface IMoveStrategy {
    public abstract List<Path> CalculatePath();
}
public abstract class MovementStrategy : IMoveStrategy {
    protected CreatureNavigator navigator;
    protected Creature creature;
    public abstract List<Path> CalculatePath();
    public void Initialize(Creature creature, CreatureNavigator navigator) {
        this.navigator = navigator;
        this.creature = creature;
    }
}
