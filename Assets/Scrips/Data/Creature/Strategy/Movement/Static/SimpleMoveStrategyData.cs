using UnityEngine;
/* The logic of movements for creature:
 * 1. I'm go on X tiles
 */

[CreateAssetMenu(fileName = "SimpleMoveSO", menuName = "Strategies/Movement/Simple")]
public class SimpleMoveStrategyData : MovementStrategyData {
    public Direction moveDirection = Direction.East;
    public int moveAmount = 1;
    public override MovementStrategy GetInstance() {
        return new SimpleMoveStrategy(moveAmount, moveDirection);
    }
}
