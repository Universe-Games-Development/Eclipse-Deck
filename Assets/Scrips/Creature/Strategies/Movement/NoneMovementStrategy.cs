using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

/* The slide logic of movements for creature:
 * 1. I`m on SupportField ? I move forward by X amount
 * 2. I`m on Attack field ? I move on west
 * 3. I can`t move because my ally is on my way? I stay this time 
 */
[CreateAssetMenu(fileName = "None Move Strategy", menuName = "Strategies/NoneMoveStrategy")]
public class NoneMovementStrategy : MovementStrategy {
    [SerializeField] protected int supportMoveAmount = 1;
    [SerializeField] protected int atteckMoveAmount = 1;
    [SerializeField] private Direction supportMoveDirection = Direction.North;

    [SerializeField] private Direction attackMoveDirection = Direction.West;

    protected override async UniTask<int> AttackFieldMovement(GameBoard gameBoard, BoardOverseer boardOverseer, Creature creatureToMove, Field currentField, bool reversedDirection) {
        await UniTask.Yield();
        return 0;
    }

    protected override async UniTask<int> SupportFieldMovement(GameBoard gameBoard, BoardOverseer boardOverseer, Creature creatureToMove, Field currentField, bool reversedDirection) {
        await UniTask.Yield();
        return 0;
    }
}
