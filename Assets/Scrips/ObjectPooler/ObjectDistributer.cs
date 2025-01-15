using UnityEngine;
using UnityEngine.Pool;
using Zenject;

public class ObjectDistributer : MonoBehaviour, IObjectDistributer {
    [SerializeField] protected GameObject objectPrefab;
    [SerializeField] protected Transform poolParent;

    protected ObjectPool<GameObject> objectPool;
    [Inject] DiContainer container;

    public virtual void Initialize() {
        if (objectPool == null) {
            CreateObjectPool();
        }
    }

    protected void CreateObjectPool() {
        if (objectPool != null) {
            Debug.LogWarning("Object pool is already created!");
            return;
        }

        objectPool = new ObjectPool<GameObject>(
            createFunc: () => container.InstantiatePrefab(objectPrefab, poolParent),
            actionOnGet: panel => panel.SetActive(true),
            actionOnRelease: panel => {
                panel.transform.SetParent(poolParent);
                panel.SetActive(false);
            },
            actionOnDestroy: Destroy
        );
    }

    public virtual GameObject CreateObject() {
        if (objectPool == null) {
            CreateObjectPool();  // Ініціалізуємо пул, якщо він не був створений
        }

        // Беремо об'єкт з пулу
        return objectPool.Get();
    }

    public virtual void ReleaseObject(GameObject obj) {
        if (objectPool == null) {
            CreateObjectPool();  // Ініціалізуємо пул, якщо він не був створений
        }

        // Повертаємо об'єкт в пул
        objectPool.Release(obj);
    }
}
