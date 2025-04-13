using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SimpleAttackStrategy", menuName = "Behaviour/Strategies/Attack/Simple")]
public class SimpleAttackStrategyData : AttackStrategyProvider {
    public override AttackStrategy GetInstance() {
        
        return new SimpleAttackStrategy();
    }
}

public class SimpleAttackStrategy : AttackStrategy {

    public override AttackData CalculateAttackData() {
        List<Field> fields = navigator.GetFieldsInDirection(creature.CurrentField, 1, Direction.North);
        AttackData attackData = new();
        attackData.AddFieldsDamage(fields, creature.Attack.CurrentValue);
        return attackData;
    }
}