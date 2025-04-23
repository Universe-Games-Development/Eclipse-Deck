using Cysharp.Threading.Tasks;
using ModestTree;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour {
    
    [SerializeField] private Transform spawnPoint;
    [Inject] private DiContainer _container;
    [Inject] private EnemyResourceProvider _enemyResourceProvider;

    private Enemy CreateEnemy(EnemyData enemyData) {
        Enemy enemy = _container.Instantiate<Enemy>(new object[] { enemyData});
        return enemy;
    }

    public async UniTask<bool> SpawnEnemy(EnemyType enemyType) {
        // Завантажуємо дані ворогів асинхронно
        List<EnemyData> enemiesData = await _enemyResourceProvider.GetEnemies(enemyType);

        // Перевірка на порожній список
        if (enemiesData == null || enemiesData.Count == 0) {
            Debug.LogWarning($"Enemy type {enemyType} not found to spawn");
            return false;
        }

        // Вибираємо випадкового ворога з отриманих даних
        var enemyData = enemiesData.GetRandomElement();

        // Створюємо ворога на основі даних
        Enemy enemy = CreateEnemy(enemyData);

        // Створюємо презентер ворога через DI контейнер
        OpponentPresenter enemyPresenter = _container.InstantiatePrefabForComponent<OpponentPresenter>(
            enemyData.presenterPrefab,
            spawnPoint.position,
            Quaternion.identity,
            spawnPoint
        );

        // Перевірка успішності створення презентера
        if (enemyPresenter == null) {
            Debug.LogError("Failed to instantiate enemy presenter prefab");
            return false;
        }


        // Ініціалізація презентера з ворогом і вью
        enemyPresenter.Initialize(enemy);

        // Запуск активності ворога асинхронно
        await enemy.StartEnemyActivity();

        return true;
    }

}

public enum EnemyType {
    Regular, Tutorial, Boss
}

