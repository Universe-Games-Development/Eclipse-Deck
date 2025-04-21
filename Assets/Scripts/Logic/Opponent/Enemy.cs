using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using static UnityEngine.EventSystems.EventTrigger;

public class Enemy : Opponent {
    public Func<UniTask> OnSpawned;
    private Speaker speech;
    [Inject] private TurnManager _turnManager;
    [Inject] protected BattleRegistrator opponentRegistrator;

    public Enemy(OpponentData opponentData, DialogueSystem dialogueSystem, GameEventBus eventBus) 
        : base (opponentData) {
        SpeechData speechData = opponentData.speechData;
        if (speechData != null) {
            speech = new Speaker(speechData, this, dialogueSystem, eventBus);
        }
    }

    internal async UniTask StartEnemyActivity() {
        if (OnSpawned != null) {
            await OnSpawned.Invoke();
        }

        if (speech != null) {
            await speech.StartDialogue();
        }

        opponentRegistrator.RegisterEnemy(this);
    }

    private async UniTask PerformTestTurn() {
        await UniTask.Delay(1500);
        _turnManager.EndTurnRequest();
    }
}
