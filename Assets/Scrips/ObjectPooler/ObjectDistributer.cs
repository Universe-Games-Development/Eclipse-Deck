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
            actionOnGet: @object => @object.SetActive(true),
            actionOnRelease: @object => {
                @object.transform.SetParent(poolParent);
                @object.SetActive(false);
            },
            actionOnDestroy: Destroy
        );
    }

    // Створення об'єкта в довільному місці
    public virtual GameObject CreateObject() {
        if (objectPool == null) {
            CreateObjectPool();  // Ініціалізуємо пул, якщо він не був створений
        }

        GameObject obj = objectPool.Get();
        return obj;
    }

    public virtual GameObject CreateObject(Vector3 position, Quaternion rotation) {
        if (objectPool == null) {
            CreateObjectPool(); 
        }

        GameObject obj = objectPool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    public virtual void ReleaseObject(GameObject obj) {
        if (objectPool == null) {
            CreateObjectPool();  // Ініціалізуємо пул, якщо він не був створений
        }

        // Повертаємо об'єкт в пул
        objectPool.Release(obj);
    }
}
