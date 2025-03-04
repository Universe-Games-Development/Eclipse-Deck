using UnityEngine;

[CreateAssetMenu(fileName = "NoneAttackStrategy", menuName = "Behaviour/Strategies/Attack/None")]
public class NoneAttackStrategyData : AttackStrategyProvider {
    public override AttackStrategy GetInstance() {
        return new NoneAttackStrategy();
    }
}

public class NoneAttackStrategy : AttackStrategy {
    public override AttackData CalculateAttackData() {
        return new();
    }
}