using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class RetreatMoveStrategy : SimpleMoveStrategy {
    private Direction retreatCheckDirection = Direction.North;
    [SerializeField] private int scaredTriggerRadius = 1;
    protected override async UniTask<int> AttackFieldMovement(GameBoard gameBoard, BoardOverseer boardOverseer, Creature creatureToMove, Field currentField, bool reversedDirection) {
        int moves = 0;

        // Отримати протилежне поле
        Field oppositeField = GetOppositeField(boardOverseer, currentField, reversedDirection);

        if (oppositeField == null) {
            Debug.LogWarning("Opposite field is null. Cannot determine if retreat is needed!");
        } else if (oppositeField.OccupiedCreature != null) {
            // Отримати шлях для руху назад
            List<Field> path = boardOverseer.GetMainGridPath(currentField, atteckMoveAmount, attackMoveDirection, reversedDirection);
            moves = await TryMove(gameBoard, creatureToMove, path);
        }

        return moves;
    }

    private Field GetOppositeField(BoardOverseer boardOverseer, Field currentField, bool reversedDirection) {
        List<Field> oppositePath = boardOverseer.GetMainGridPath(currentField, scaredTriggerRadius, retreatCheckDirection, reversedDirection);
        if (oppositePath.Count > 0) {
            return oppositePath[0];
        }
        return null;
    }
}
