using UnityEngine;

[CreateAssetMenu(fileName = "AttackStrategyData", menuName = "CreatureStrategies/Attack")]
public class CreatureAttackData : ScriptableObject {
    [Header("UI")]
    public string Name;
    public Sprite strategyIcon;
    [Space]
    [Header("Attack Strategy")]
    public AttackStrategyProvider attackStrategy;
    [Space]
    [Header("Support Strategy")]
    public AttackStrategyProvider supportStrategy;
}

public abstract class AttackStrategyProvider : ScriptableObject {
    public abstract AttackStrategy GetInstance();
}

public interface IAttackStrategy {
    AttackData CalculateAttackData();
}

public abstract class AttackStrategy : IAttackStrategy {
    protected CreatureNavigator navigator;
    protected Creature creature;
    // Returns fields to deal damage
    public abstract AttackData CalculateAttackData();
    public void Initialize(Creature creature, CreatureNavigator navigator) {
        this.creature = creature;
        this.navigator = navigator;
    }
}