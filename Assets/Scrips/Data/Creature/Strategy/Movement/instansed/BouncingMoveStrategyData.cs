using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BouncingMoveStrategyData", menuName = "Behaviour/Strategies/Movement/Bouncing")]
public class BouncingMoveStrategyData : MovementStrategyProvider {
    public Direction initialTurning = Direction.West;
    public int moveAmount = 1;
    public override MovementStrategy GetInstance() {
        return new BouncingMoveStrategy(moveAmount, initialTurning);
    }

    public void OnValidate() {
        if (initialTurning != Direction.West || initialTurning != Direction.East) {
            initialTurning = Direction.West;
        }
    }
}

public class BouncingMoveStrategy : MovementStrategy {
    private Direction bounceDirection;
    private readonly int moveAmount;
    public BouncingMoveStrategy(int moveAmount, Direction initialDirection) {
        this.bounceDirection = initialDirection;
        this.moveAmount = moveAmount;
    }

    public override List<Path> CalculatePath() {
        List<Path> paths = new();
        Field CurrentField = creature.CurrentField;
        Path path = navigator.GenerateSimplePath(CurrentField, moveAmount, bounceDirection);
        if (path.isInterrupted && path.interruptedAt == 0) {
            bounceDirection = CompassUtil.GetOppositeDirection(bounceDirection);
            path = navigator.GenerateSimplePath(CurrentField, moveAmount, bounceDirection);
        }
        paths.Add(path);
        return paths;
    }
}