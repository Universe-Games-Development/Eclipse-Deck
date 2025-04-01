using System;
using UnityEngine;
using Zenject;

public class EnemyPresenter : MonoBehaviour {
    [SerializeField] EnemyView view;
    [Inject] OpponentRegistrator opponentRegistrator;
    [Inject] DialogueSystem _dialogueSystem;
    [Inject] GameEventBus _eventBus;
    public Enemy Enemy { get; private set; }
    private Speaker speech;

    public void InitializeEnemy(Enemy enemy, Transform spawnPoint) {
        if (enemy.Data != null && enemy.Data.speechData != null) {
            speech = new Speaker(enemy.Data.speechData, enemy, _dialogueSystem, _eventBus);
        }

        enemy.OnDefeat += OnDefeatActions;
        opponentRegistrator.RegisterOpponent(enemy);
        view.Initialize(enemy.Data);
    }

    private void OnDefeatActions(Opponent opponent) {
        throw new NotImplementedException();
    }
}
