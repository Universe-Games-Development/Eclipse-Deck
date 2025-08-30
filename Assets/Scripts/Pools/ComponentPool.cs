using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

public class ComponentPool<T> : MonoBehaviour where T : MonoBehaviour {
    [Header("Pool Settings")]
    [SerializeField] protected int defaultCapacity = 10;
    [SerializeField] protected int maxSize = 50;
    [SerializeField] protected bool collectionCheck = true;

    protected Transform poolParent;

    [Header("Prefab")]
    [SerializeField] private T prefab; // ����� ������� ���!

    private ObjectPool<T> pool;

    // �������� ������ �� ����
    public static ComponentPool<T> Instance { get; private set; }
    [Inject] DiContainer container;

    protected void Awake() {
        // Singleton pattern ��� ������� �������
        if (Instance == null) {
            Instance = this;

            poolParent = new GameObject($"Pool_{GetType().Name}").transform;
            poolParent.SetParent(transform);
            poolParent.localPosition = Vector3.zero;

            InitializePool();
        } else if (Instance != this) {
            Debug.LogWarning($"Multiple {typeof(T).Name} pools detected! Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    protected void InitializePool() {
        if (prefab == null) {
            Debug.LogError($"Prefab not assigned for {typeof(T).Name} pool!");
            return;
        }

        pool = new ObjectPool<T>(
            createFunc: CreatePooledItem,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: collectionCheck,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        // �������� ���������� ����
        PrewarmPool();
    }

    private T CreatePooledItem() {
        return container.InstantiatePrefabForComponent<T>(prefab, poolParent);
    }

    private void OnTakeFromPool(T item) {
        item.gameObject.SetActive(true);

        if (item is IPoolable poolable) {
            poolable.OnPoolGet();
        }
    }

    private void OnReturnToPool(T item) {
        if (item is IPoolable poolable) {
            poolable.OnPoolRelease();
        }

        item.gameObject.SetActive(false);
        item.transform.SetParent(poolParent);
    }

    private void OnDestroyPoolObject(T item) {
        if (item != null) {
            if (item is IPoolable poolable) {
                poolable.OnPoolDestroy();
            }

            Destroy(item.gameObject);
        }
    }

    private void PrewarmPool() {
        var temp = new List<T>();

        // ��������� ��'���� ��� �������
        for (int i = 0; i < defaultCapacity; i++) {
            temp.Add(Get());
        }

        // ��������� �� �����
        foreach (var item in temp) {
            Release(item);
        }
    }

    // ������ ������ ��� ������ � �����
    public T Get() {
        if (pool == null) {
            Debug.LogError($"Pool for {typeof(T).Name} not initialized!");
            return null;
        }

        return pool.Get();
    }

    public void Release(T item) {
        if (pool == null || item == null) return;

        pool.Release(item);
    }

    public void Clear() {
        pool?.Clear();
    }

    // ������� ������ ��� ��������
    public static T GetStatic() {
        return Instance?.Get();
    }

    public static void ReleaseStatic(T item) {
        Instance?.Release(item);
    }

    // ���������� ��� ���
    [ContextMenu("Log Pool Info")]
    public void LogPoolInfo() {
        if (pool == null) return;

        Debug.Log($"{typeof(T).Name} Pool Info:\n" +
                 $"- Active objects: {poolParent.childCount}\n" +
                 $"- Capacity: {defaultCapacity}\n" +
                 $"- Max size: {maxSize}");
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }

        pool?.Clear();
    }
}

public abstract class PoolableObject<T> : MonoBehaviour, IPoolable where T : MonoBehaviour {

    public virtual void OnPoolCreate() {
    }

    public virtual void OnPoolGet() {
        // ��� ����� ������� ���� ��'����
    }

    public virtual void OnPoolRelease() {
        // ��� ����� �������� ���� ��'����
    }

    public virtual void OnPoolDestroy() {
        // �������� �������
    }

    // ������� ����� ��� ���������� ���� � ���
    public virtual void ReturnToPool() {
        if (this is T item) {
            ComponentPool<T>.ReleaseStatic(item);
        }
    }
}