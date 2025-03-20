using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class Enemy : Opponent {
    [Inject] private TurnManager turnManager;
    private Speaker speech;
    public Enemy(GameEventBus eventBus, CardManager cardManager, IActionFiller abilityInputter) : base(eventBus, cardManager, abilityInputter) {
        Name = "Enemy";
    }

    protected override void TurnStartActions(ref OnTurnStart eventData) {
        if (eventData.StartingOpponent != this) {
            return;
        }
        base.TurnStartActions(ref eventData);
        PerformTestTurn().Forget();
    }

    private async UniTask PerformTestTurn() {
        await UniTask.Delay(1500);
        turnManager.EndTurnRequest(this);
    }
}
