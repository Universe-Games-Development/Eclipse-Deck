using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IEnemyFactory {
    Opponent Create(EnemyData enemyData);
    CharacterPresenter CreatePresenter(Opponent opponent);
}
public class EnemyFactory : MonoBehaviour {

    [SerializeField] private Transform spawnPoint;
    [Inject] private DiContainer _container;
    [Inject] private EnemyResourceProvider _enemyResourceProvider;

    public Enemy CreateEnemy(EnemyData enemyData) {
        Enemy enemy = _container.Instantiate<Enemy>(new object[] { enemyData });
        return enemy;
    }

    public Enemy CreateEnemy(EnemyType type) {
        if (!_enemyResourceProvider.TryGetCachedEnemies(type, out List<EnemyData> enemiesData)) {
            if (enemiesData == null || enemiesData.Count == 0) {
                Debug.LogWarning($"Enemy type {type} not found to spawn");
                return null;
            }
        }
        if (enemiesData.TryGetRandomElement(out var enemyData)) {
            return null;
        }
        return CreateEnemy(enemyData);
    }

    public CharacterPresenter SpawnEnemy(Enemy enemy) {

        // Створюємо презентер ворога через DI контейнер
        CharacterPresenter enemyPresenter = _container.InstantiatePrefabForComponent<CharacterPresenter>(
            enemy.Data.presenterPrefab,
            spawnPoint.position,
            Quaternion.identity,
            spawnPoint
        );

        // Перевірка успішності створення презентера
        if (enemyPresenter == null) {
            Debug.LogError("Failed to instantiate enemy presenter prefab");
            return null;
        }


        // Ініціалізація презентера з ворогом
        enemyPresenter.Initialize(enemy);

        return enemyPresenter;
    }

}

public enum EnemyType {
    Regular, Tutorial, Boss
}

