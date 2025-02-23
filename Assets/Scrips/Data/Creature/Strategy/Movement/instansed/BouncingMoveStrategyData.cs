using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BouncingMoveStrategySO", menuName = "Strategies/Movement/Bouncing")]
public class BouncingMoveStrategyData : MovementStrategyData {
    public Direction initialTurning = Direction.East;
    public int moveAmount = 1;
    public override MovementStrategy GetInstance() {
        return new BouncingMoveStrategy(moveAmount, initialTurning);
    }
}

public class BouncingMoveStrategy : MovementStrategy {
    private bool isBounced = false;

    private Direction currentDirection;
    private readonly int moveAmount;
    public BouncingMoveStrategy(int moveAmount, Direction initialDirection) {
        this.currentDirection = initialDirection;
        this.moveAmount = moveAmount;
    }

    public override List<Path> CalculatePath(Field CurrentField) {
        List<Path> paths = new();
        Path path = navigator.GenerateSimplePath(CurrentField, moveAmount, currentDirection);
        if (path.isInterrupted) {
            isBounced = true;
        }
        if (isBounced) {
            currentDirection = CompassUtil.GetOppositeDirection(currentDirection);
            path = navigator.GenerateSimplePath(CurrentField, moveAmount, currentDirection);
        }
        paths.Add(path);
        return paths;
    }
}