using System.Collections.Generic;
using UnityEngine;
/* The logic of movements for creature:
 * 1. I'm go on X tiles
 */

[CreateAssetMenu(fileName = "SimpleMoveSO", menuName = "Behaviour/Strategies/Movement/Simple/")]
public class SimpleMoveStrategyData : MovementStrategyProvider {
    public Direction moveDirection = Direction.North;
    public int moveAmount = 1;
    public override MovementStrategy GetInstance() {
        return new SimpleMoveStrategy(moveAmount, moveDirection);
    }
}
public class SimpleMoveStrategy : MovementStrategy {
    public int defaultMoveAmount;
    public Direction defaultMoveDirection;

    public SimpleMoveStrategy(int defaultMoveAmount, Direction defaultMoveDirection) {
        this.defaultMoveAmount = defaultMoveAmount;
        this.defaultMoveDirection = defaultMoveDirection;
    }

    public override List<Path> CalculatePath() {
        List<Path> paths = new() {
            navigator.GenerateSimplePath(creature.CurrentField, defaultMoveAmount, defaultMoveDirection)
        };
        return paths;
    }
}