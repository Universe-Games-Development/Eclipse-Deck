using Cysharp.Threading.Tasks;
using UnityEngine;

public class Enemy : Opponent {
    private TurnManager _turnManager;
    public Enemy(TurnManager turnManager, GameEventBus eventBus, CommandManager commandManager, CardProvider cardProvider) 
        : base (eventBus, commandManager, cardProvider) {
        _turnManager = turnManager;
    }

    protected override void TurnStartActions(ref OnTurnStart eventData) {
        base.TurnStartActions(ref eventData);

        if (eventData.StartingOpponent != this) {
            return;
        }
        PerformTestTurn().Forget();
    }

    private async UniTask PerformTestTurn() {
        await UniTask.Delay(1500);
        _turnManager.EndTurnRequest();
    }
}
