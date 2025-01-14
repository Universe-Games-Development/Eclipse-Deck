using Cysharp.Threading.Tasks;
using System.Collections.Generic;

/* The logic of movements for creature:
 * 1. I'm go on choosen direction at support field
 */
public class SimpleMoveStrategy : MovementStrategy {
    public SimpleMoveStrategy(CreatureMovementDataSO data) : base(data) {
    }
    protected override async UniTask<int> Movement(GameContext gameContext, bool reversedDirection) {
        // Determine movement direction and path
        List<Field> path = boardOverseer.GetMainGridPath(currentField, data.supportMoveAmount, data.supportMoveDirection, reversedDirection);

        return await TryMove(gameBoard, creatureToMove, path);
    }
}
