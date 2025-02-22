using System.Collections.Generic;

public class SimpleMoveStrategy : MovementStrategy {
    public int defaultMoveAmount;
    public Direction defaultMoveDirection;

    public SimpleMoveStrategy(int defaultMoveAmount, Direction defaultMoveDirection) {
        this.defaultMoveAmount = defaultMoveAmount;
        this.defaultMoveDirection = defaultMoveDirection;
    }

    public override List<Path> CalculatePath(Field currentField) {
        List<Path> paths = new() {
            navigator.GenerateSimplePath(currentField, defaultMoveAmount, defaultMoveDirection)
        };
        return paths;
    }
}