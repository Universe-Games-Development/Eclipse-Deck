using System.Collections.Generic;
using UnityEngine;

/* The logic of movements for creature:
 * 1. I'm do nothing
 */

[CreateAssetMenu(fileName = "NoneMoveStrategySO", menuName = "Strategies/Movement/None")]
public class NoneMoveStrategyData : MovementStrategyData {
    public override MovementStrategy GetInstance() {
        return new NoneMoveStrategy();
    }
}


public class NoneMoveStrategy : MovementStrategy {
    public override List<Path> CalculatePath(Field currentField) {
        List<Path> paths = new();
        return paths;
    }
}