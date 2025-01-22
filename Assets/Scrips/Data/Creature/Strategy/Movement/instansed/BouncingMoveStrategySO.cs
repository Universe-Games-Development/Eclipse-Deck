using System.Collections.Generic;
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
    private readonly int moveAmount;
    public BouncingMoveStrategy(int moveAmount, Direction initialDirection) {
        this.currentDirection = initialDirection;
        this.moveAmount = moveAmount;
    }

    protected override List<Path> Move() {
        List<Path> paths = new();
        Path path = navigator.GenerateSimplePath(moveAmount, currentDirection);
        if (path.isInterrupted) {
            isBounced = true;
        }
        if (isBounced) {
            currentDirection = CompassUtil.GetOppositeDirection(currentDirection);
            path = navigator.GenerateSimplePath(moveAmount, currentDirection);
        }
        paths.Add(path);
        return paths;
    }
}