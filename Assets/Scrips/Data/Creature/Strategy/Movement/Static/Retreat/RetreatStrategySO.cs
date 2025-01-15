using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class RetreatStrategySO : SimpleMoveStrategySO {
    public int retreatAmount = 1;
    public Direction checkDirection = Direction.North;

    protected override async UniTask<int> Move() {
        int moves;
        if (ConditionToEscape()) {
            moves = await Escape();
        } else {
            moves = await base.Move();
        }

        return moves;
    }

    protected virtual bool ConditionToEscape() {
        return false;
    }

    protected async UniTask<int> Escape() {
        int moves = await navigator.TryMove(retreatAmount, checkDirection);

        if (moves == 0) {
            List<Field> freeFields = navigator.GetAdjacentFields()
                .Where(field => field.Owner == navigator.CurrentField.Owner && field.OccupiedCreature == null)
                .ToList();

            if (freeFields.Count == 0) {
                return moves;
            }

            Field fieldToEscape = RandomUtil.GetRandomFromList(freeFields);
            moves = await navigator.TryMoveToField(fieldToEscape);
        }

        return moves;
    }
}
