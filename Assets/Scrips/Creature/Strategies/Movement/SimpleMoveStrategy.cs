using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

/* The simple logic of movements for creature:
 * 1. I'm on SupportField? I move forward by X amount.
 * 2. I'm on Attack field? I don't move!
 * 3. I can't move because my ally is on my way? I stay this time.
 */
[CreateAssetMenu(fileName = "Default Move Strategy", menuName = "Strategies/CreatureMovement/SimpleMoveStrategy")]
public class SimpleMoveStrategy : MovementStrategy {
    protected override async UniTask<int> AttackFieldMovement(GameBoard gameBoard, BoardOverseer boardOverseer, Creature creatureToMove, Field currentField, bool reversedDirection) {
        await UniTask.Yield();
        return 0;
    }

    protected override async UniTask<int> SupportFieldMovement(GameBoard gameBoard, BoardOverseer boardOverseer, Creature creatureToMove, Field currentField, bool reversedDirection) {
        // Determine movement direction and path
        List<Field> path = boardOverseer.GetMainGridPath(currentField, supportMoveAmount, supportMoveDirection, reversedDirection);

        int moves = await TryMove(gameBoard, creatureToMove, path);

        return moves;
    }
}
