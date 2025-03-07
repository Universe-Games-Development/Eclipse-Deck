using Cysharp.Threading.Tasks;
using System;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class Enemy : Opponent {
    [Inject] private TurnManager turnManager;

    public Enemy(GameEventBus eventBus, AssetLoader assetLoader, IActionFiller abilityInputter) : base(eventBus, assetLoader, abilityInputter) {
        Name = "Enemy";
    }

    protected override void TurnStartActions(ref OnTurnStart eventData) {
        if (eventData.startTurnOpponent != this) {
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
