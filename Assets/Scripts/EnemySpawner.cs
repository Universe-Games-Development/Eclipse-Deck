using Cysharp.Threading.Tasks;
using ModestTree;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour {
    [Inject] private DiContainer _container;
    [Inject] private EnemyResourceProvider _enemyResourceProvider;
    [Inject] EnemyPresenter _enemyPresenter;

    private Enemy InitializeEnemy(OpponentData enemyData) {
        Enemy enemy = _container.Instantiate<Enemy>();
        enemy.SetData(enemyData);
        return enemy;
    }

    public async UniTask<bool> SpawnEnemy(EnemyType enemyType) {
        List<OpponentData> enemiesData = await _enemyResourceProvider.GetEnemies(enemyType);

        if (enemiesData == null || enemiesData.IsEmpty()) {
            return false;
        }
        var enemyData = enemiesData.GetRandomElement();
        Enemy enemy = InitializeEnemy(enemyData);
        _enemyPresenter.InitializeEnemy(enemy);
        await _enemyPresenter.StartEnemyActivity();
        return true;
    }
}

public enum EnemyType {
    Regular, Tutorial, Boss
}

