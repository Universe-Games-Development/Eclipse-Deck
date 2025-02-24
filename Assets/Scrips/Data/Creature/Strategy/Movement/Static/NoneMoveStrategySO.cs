using System.Collections.Generic;
using UnityEngine;

/* The logic of movements for creature:
 * 1. I'm do nothing
 */

[CreateAssetMenu(fileName = "NoneMoveStrategySO", menuName = "Behaviour/Strategies/Movement/None")]
public class NoneMoveStrategyData : MovementStrategyProvider {
    public override MovementStrategy GetInstance() {
        return new NoneMoveStrategy();
    }
}


public class NoneMoveStrategy : MovementStrategy {
    public override List<Path> CalculatePath() {
        return new();
    }
}