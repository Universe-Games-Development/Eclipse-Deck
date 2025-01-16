using Cysharp.Threading.Tasks;
using UnityEngine;

/* The logic of movements for creature:
 * 1. I'm do nothing
 */
[CreateAssetMenu(fileName = "SimpleMoveSO", menuName = "Strategies/Movement/Simple")]
public class SimpleMoveStrategySO : StaticMovementStrategySO {
    public int defaultMoveAmount = 1;
    public Direction defaultMoveDirection = Direction.South;

    protected override async UniTask<int> Move() {
        return await navigator.TryMove(defaultMoveAmount, defaultMoveDirection);
    }
}