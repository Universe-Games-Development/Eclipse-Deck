using Cysharp.Threading.Tasks;
using ModestTree;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour {
    [Inject] private DiContainer _container;
    [Inject] private EnemyResourceProvider _enemyResourceProvider;
    [SerializeField] private Transform spawnPoint;

    private Enemy CreateEnemy(EnemyData enemyData) {
        Enemy enemy = _container.Instantiate<Enemy>(new object[] { enemyData});
        return enemy;
    }

    public async UniTask<bool> SpawnEnemy(EnemyType enemyType) {
        List<EnemyData> enemiesData = await _enemyResourceProvider.GetEnemies(enemyType);

        if (enemiesData == null || enemiesData.IsEmpty()) {
            Debug.LogWarning($"Enemy type {enemyType} not found to spawn");
            return false;
        }
        var enemyData = enemiesData.GetRandomElement();
        Enemy enemy = CreateEnemy(enemyData);
        OpponentView _enemyView = _container.InstantiatePrefabForComponent<OpponentView>(enemyData.viewPrefab, spawnPoint);
        EnemyPresenter enemyPresenter = new(enemy, _enemyView);
        await enemy.StartEnemyActivity();
        return true;
    }
}

public enum EnemyType {
    Regular, Tutorial, Boss
}

