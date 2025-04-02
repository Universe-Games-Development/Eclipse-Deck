using System;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] private Transform spawnPoint;
    [Inject] private EnemyProvider _enemyProvider;
    [Inject] private EnemyPresenter _enemyPresenter;
    [Inject] private GameEventBus _eventBus;
    [Inject] private DiContainer _container;

    [Inject]
    public void Construct (EnemyProvider enemyProvider) {
        _enemyProvider = enemyProvider;
    }

    public Enemy PrepareBoss() {
        OpponentData opponentData = _enemyProvider.GetBossData();
        Enemy enemy = _container.Instantiate<Enemy>();
        enemy.SetData(opponentData);
        _enemyPresenter.InitializeEnemy(enemy, spawnPoint);
        return enemy;
    }

    public Enemy PrepareEnemy() {
        OpponentData opponentData = _enemyProvider.GetEnemyData();
        Enemy enemy = _container.Instantiate<Enemy>();
        enemy.SetData(opponentData);
        _enemyPresenter.InitializeEnemy(enemy, spawnPoint);
        return enemy;
    }

    public Enemy PrepareTutorialEnemy() {
        OpponentData opponentData = _enemyProvider.GetTutorialEnemy();
        if (opponentData == null) {
            Debug.LogWarning("opponentData is null for enemyspawn");
            return null;
        }
        Enemy enemy = _container.Instantiate<Enemy>();
        enemy.SetData(opponentData);
        _enemyPresenter.InitializeEnemy(enemy, spawnPoint);
        return enemy;
    }
}
