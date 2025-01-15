using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "BouncingMoveStrategySO", menuName = "Strategies/Movement/Bouncing")]
public class BouncingMoveStrategySO : MovementStrategySO {
    public Direction initialTurning = Direction.East;
    public int moveAmount = 1;
    public override IMoveStrategy GetInstance() {
        return new BouncingMoveStrategy(moveAmount, initialTurning);
    }
}

public class BouncingMoveStrategy : InstanceMovementStrategy {
    private bool isBounced = false;

    private Direction currentDirection;
    private int moveAmount;
    public BouncingMoveStrategy(int moveAmount, Direction initialDirection) {
        this.currentDirection = initialDirection;
        this.moveAmount = moveAmount;
    }

    protected override async UniTask<int> Move() {
        int moves = await navigator.TryMove(moveAmount, currentDirection);
        if (moves == 0) {
            isBounced = true;
        }
        if (moves == 0 && isBounced) {
            currentDirection = CompasUtil.GetOppositeDirection(currentDirection);
            await navigator.TryMove(moveAmount, currentDirection);
        }
        return moves;
    }
}