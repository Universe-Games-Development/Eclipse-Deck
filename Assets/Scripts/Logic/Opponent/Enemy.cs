using Cysharp.Threading.Tasks;
using System;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class Enemy : Opponent {
    [Inject] private TurnManager turnManager;
    private DialogueSystem _dialogueSystem;
    private Speaker speech;
    public Enemy(OpponentData opponentData, DialogueSystem dialogueSystem, GameEventBus eventBus, CommandManager commandManager, CardProvider cardProvider) 
        : base (opponentData, eventBus, commandManager, cardProvider) {
        Name = "Enemy";

        _dialogueSystem = dialogueSystem;

        if (opponentData != null && opponentData.speechData != null) {
            speech = new Speaker(opponentData.speechData, this, _dialogueSystem, _eventBus);
        }
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
            turnManager.EndTurnRequest();
        }
    }
