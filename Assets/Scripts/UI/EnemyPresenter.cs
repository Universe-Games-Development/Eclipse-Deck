using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemyPresenter : MonoBehaviour {
    [SerializeField] EnemyView view;
    [Inject] OpponentRegistrator opponentRegistrator;

    public Enemy Enemy { get; private set; }

    
    public void InitializeEnemy(Enemy enemy, Transform spawnPoint) {

        enemy.OnDefeat += OnDefeatActions;
        opponentRegistrator.RegisterOpponent(enemy);
        view.Initialize(enemy.Data);
    }

    private void OnDefeatActions(Opponent opponent) {
        throw new NotImplementedException();
    }
}
