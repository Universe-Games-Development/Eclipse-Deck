using Cysharp.Threading.Tasks;
using UnityEngine;

public class Enemy : Opponent {
    private TurnManager _turnManager;
    public Enemy(TurnManager turnManager, GameEventBus eventBus, CommandManager commandManager, CardProvider cardProvider) 
        : base (eventBus) {
        _turnManager = turnManager;
    }

    private async UniTask PerformTestTurn() {
        await UniTask.Delay(1500);
        _turnManager.EndTurnRequest();
    }
}
