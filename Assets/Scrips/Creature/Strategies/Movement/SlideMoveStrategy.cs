using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

/* The slide logic of movements for creature:
 * 1. I`m on SupportField ? I move forward by X amount
 * 2. I`m on Attack field ? I move on west
 * 3. I can`t move because my ally is on my way? I stay this time 
 */
[CreateAssetMenu(fileName = "Slide Move Strategy", menuName = "Strategies/CreatureMovement/SlidingMoveStrategy")]
public class SlidingMoveStrategy : SimpleMoveStrategy {

    protected override async UniTask<int> AttackFieldMovement(GameBoard gameBoard, BoardOverseer boardOverseer, Creature creatureToMove, Field currentField, bool reversedDirection) {
        List<Field> path = boardOverseer.GetMainGridPath(currentField, supportMoveAmount, attackMoveDirection, reversedDirection);
        return await TryMove(gameBoard, creatureToMove, path);
    }
}