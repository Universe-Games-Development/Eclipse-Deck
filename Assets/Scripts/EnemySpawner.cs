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

    public Enemy SpawnEnemy(RoomType roomType) {
        OpponentData opponentData = null;
        switch (roomType) {
            case RoomType.Boss:
                opponentData = _enemyProvider.GetBossData();
                break;
            case RoomType.Enemy:
                opponentData = _enemyProvider.GetEnemyData();
                break;
            default:
                throw new ArgumentException("Invalid room type");
        }

        Enemy enemy = _container.Instantiate<Enemy>();
        enemy.SetData(opponentData);
        _enemyPresenter.InitializeEnemy(enemy, spawnPoint);

        return enemy;
    }
}
