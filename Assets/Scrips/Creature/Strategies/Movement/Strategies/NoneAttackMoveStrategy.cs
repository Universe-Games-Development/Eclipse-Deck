/* The logic of movements for creature:
 * 1. I'm do nothing
 */
using Cysharp.Threading.Tasks;

public class NoneMoveStrategy : MovementStrategy {
    public NoneMoveStrategy(CreatureMovementDataSO data) : base(data) {
    }

    protected override async UniTask<int> Movement(GameContext gameContext, bool reversedDirection) {
        await UniTask.Yield();
        return 0;
    }
}