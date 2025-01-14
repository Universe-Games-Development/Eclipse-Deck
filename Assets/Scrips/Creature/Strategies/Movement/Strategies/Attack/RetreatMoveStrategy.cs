/* The logic of movements for creature:
 * 1. I'm do nothing
 */
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;

public class RetreatStrategy : MovementStrategy {
    public RetreatStrategy(CreatureMovementDataSO data) : base(data) {
    }

    protected override async UniTask<int> Movement(GameContext gameContext, bool reversedDirection) {
        int moves = 0;

        if (data is RetreatMovementDataSO retreatData) {
            // Тепер можна працювати з retreatData, і він гарантовано буде типом RetreatMovementDataSO
            List<Field> oppositeField = GetOppositeFields(retreatData, reversedDirection);
            foreach (Field field in oppositeField) {
                if (field == null) {
                    throw new ArgumentException("Opposite field is null. Cannot determine if retreat is needed!");
                } else if (field.OccupiedCreature != null) {
                    // Отримати шлях для руху назад
                    List<Field> path = boardOverseer.GetMainGridPath(currentField, retreatData.attackMovesAmount, retreatData.attackMoveDirection, reversedDirection);
                    moves = await TryMove(gameBoard, creatureToMove, path);
                }
            }
        } else {
            throw new ArgumentException("Invalid move data type. Need : " + nameof(RetreatMovementDataSO));
        }

        return moves;
    }

    private List<Field> GetOppositeFields(RetreatMovementDataSO moveData, bool reversedDirection) {
        return boardOverseer.GetMainGridPath(currentField, moveData.scarredForwardDistance, moveData.escapeDirection, reversedDirection);
    }
}
