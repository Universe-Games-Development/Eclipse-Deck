using System.Collections.Generic;
using System.Linq;

public class RetreatStrategySO : SimpleMoveStrategySO {
    public int retreatAmount = 1;
    public Direction checkDirection = Direction.North;

    protected override List<Path> Move() {
        List<Path> paths = new();
        if (ConditionToEscape()) {
            paths.Add(Escape());
        } else {
            paths = base.Move();
        }

        return paths;
    }

    protected virtual bool ConditionToEscape() {
        return false;
    }

    protected Path Escape() {
        Path escapePath = navigator.GenerateSimplePath(retreatAmount, checkDirection);

        if (escapePath.isInterrupted) {
            List<Field> freeFields = navigator.GetAdjacentFields()
                .Where(field => field.Owner == navigator.CurrentField.Owner && field.OccupiedCreature == null)
                .ToList();

            if (freeFields.Count == 0) {
                return escapePath;
            }

            Field fieldToEscape = RandomUtil.GetRandomFromList(freeFields);
            Direction directionToEscape = navigator.GetDirectionToField(fieldToEscape);
            escapePath = navigator.GenerateSimplePath(retreatAmount, directionToEscape);
        }

        return escapePath;
    }
}
