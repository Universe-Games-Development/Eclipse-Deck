/* The logic of movements for creature:
 * 1. I'm do nothing
 */
using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "NoneMoveStrategySO", menuName = "Strategies/Movement/None")]
public class NoneMoveStrategySO : StaticMovementStrategySO {
    protected override async UniTask<int> Move() {
        await UniTask.Yield();
        return 0;
    }
}