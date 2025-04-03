using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemyManager : MonoBehaviour {
    [SerializeField] private Transform spawnPoint;

    [Inject] private DiContainer _container;
    [Inject] private EnemyPresenter _enemyPresenter;
    [Inject] private EnemyResourceLoader _enemyResourceLoader;
    [Inject] private LocationTransitionManager _transitionManager;

    public bool TrySpawnBoss(out Enemy enemy) {
        return TrySpawnEnemy(_enemyResourceLoader.GetBossesForLocation, out enemy);
    }

    public bool TrySpawnRegularEnemy(out Enemy enemy) {
        return TrySpawnEnemy(_enemyResourceLoader.GetRegularEnemiesForLocation, out enemy);
    }

    public bool TrySpawnTutorialEnemy(out Enemy enemy) {
        return TrySpawnEnemy(_enemyResourceLoader.GetTutorialsForLocation, out enemy);
    }

    private bool TrySpawnEnemy(
        System.Func<LocationType, List<OpponentData>> getEnemiesForLocation,
        out Enemy enemy
    ) {
        var currentLocation = _transitionManager.CurrentLocationData.locationType;
        var enemies = getEnemiesForLocation(currentLocation);
        var enemyData = enemies.Count > 0 ? enemies.GetRandomElement() : null;

        if (enemyData == null) {
            enemy = null;
            return false;
        }

        enemy = InitializeEnemy(enemyData);
        return true;
    }

    private Enemy InitializeEnemy(OpponentData enemyData) {
        Enemy enemy = _container.Instantiate<Enemy>();
        enemy.SetData(enemyData);
        _enemyPresenter.InitializeEnemy(enemy, spawnPoint);
        return enemy;
    }
}
