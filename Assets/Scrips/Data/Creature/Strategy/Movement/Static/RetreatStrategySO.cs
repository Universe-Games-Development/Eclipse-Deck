/* The logic of movements for creature:
 * 1. I'm do nothing
 */
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RetreatSO", menuName = "Strategies/Movement/Retreat")]
public class RetreatStrategySO : SimpleMoveStrategySO {
    public int scaredDistance = 1;
    public Direction scarredDirection = Direction.South;
    public int retreatAmount = 1;
    public Direction retreatDirection = Direction.North;

    protected override async UniTask<int> Move() {

        int moves;
        if (ConditionToEscape()) {
            moves = await Escape();
        } else {
            moves = await base.Move();
        }

        return moves;
    }

    protected bool ConditionToEscape() {
        return navigator.GetCreaturesInDirection(scaredDistance, scarredDirection).Count > 0;
    }

    protected async UniTask<int> Escape() {
        int moves = await navigator.TryMove(retreatAmount, retreatDirection);

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