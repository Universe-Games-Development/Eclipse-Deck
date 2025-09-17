using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;
public interface IComponentPool<T> where T : MonoBehaviour {
    T Get();
    void Release(T item);
    void Clear();
    void PrewarmPool(int count);
    int ActiveCount { get; }
    int InactiveCount { get; }
}

public class ComponentPool<T> : MonoBehaviour, IComponentPool<T> where T : MonoBehaviour {
    [Header("Pool Settings")]
    [SerializeField] protected int defaultCapacity = 10;
    [SerializeField] protected int maxSize = 50;
    [SerializeField] protected bool collectionCheck = true;
    [SerializeField] protected T prefab;

    protected Transform poolParent;
    protected ObjectPool<T> pool;

    [Inject] protected DiContainer container;

    public int ActiveCount => poolParent != null ? poolParent.childCount : 0;
    public int InactiveCount => pool?.CountInactive ?? 0;

    protected virtual void Awake() {
        InitializePool();
    }

    protected virtual void InitializePool() {
        if (prefab == null) {
            Debug.LogError($"Prefab not assigned for {typeof(T).Name} pool!");
            return;
        }

        // Створюємо батьківський об'єкт для пулу
        poolParent = new GameObject($"Pool_{typeof(T).Name}").transform;
        poolParent.SetParent(transform);
        poolParent.localPosition = Vector3.zero;

        pool = new ObjectPool<T>(
            createFunc: CreatePooledItem,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: collectionCheck,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        // Попереднє заповнення
        PrewarmPool(defaultCapacity);
    }

    protected virtual T CreatePooledItem() {
        var item = container.InstantiatePrefabForComponent<T>(prefab, poolParent);

        // Викликаємо OnPoolCreate якщо об'єкт це підтримує
        if (item is IPoolable poolable) {
            poolable.OnPoolCreate();
        }

        return item;
    }

    protected virtual void OnTakeFromPool(T item) {
        if (item == null) return;

        item.gameObject.SetActive(true);

        if (item is IPoolable poolable) {
            poolable.OnPoolGet();
        }
    }

    protected virtual void OnReturnToPool(T item) {
        if (item == null) return;

        if (item is IPoolable poolable) {
            poolable.OnPoolRelease();
        }

        item.gameObject.SetActive(false);
        item.transform.SetParent(poolParent);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;
    }

    protected virtual void OnDestroyPoolObject(T item) {
        if (item != null) {
            if (item is IPoolable poolable) {
                poolable.OnPoolDestroy();
            }

            Destroy(item.gameObject);
        }
    }

    public virtual T Get() {
        if (pool == null) {
            Debug.LogError($"Pool for {typeof(T).Name} not initialized!");
            return null;
        }

        return pool.Get();
    }

    public virtual void Release(T item) {
        if (pool == null || item == null) return;
        pool.Release(item);
    }

    public virtual void Clear() {
        pool?.Clear();
    }

    public virtual void PrewarmPool(int count) {
        if (pool == null) return;

        var temp = new List<T>();

        for (int i = 0; i < count; i++) {
            temp.Add(Get());
        }

        foreach (var item in temp) {
            Release(item);
        }
    }

    [ContextMenu("Log Pool Info")]
    public void LogPoolInfo() {
        Debug.Log($"{typeof(T).Name} Pool Info:\n" +
                 $"- Active objects: {ActiveCount}\n" +
                 $"- Inactive objects: {InactiveCount}\n" +
                 $"- Capacity: {defaultCapacity}\n" +
                 $"- Max size: {maxSize}");
    }

    protected virtual void OnDestroy() {
        pool?.Clear();
    }
}
