using UnityEngine;
using UnityEngine.Pool;

// MONOBEHAVIOUR IS NOT OBJECT POOL ITS POOLED OBJECT!
public abstract class BasePool<T> where T : MonoBehaviour {
    private ObjectPool<T> objectPool;
    private T prefab;
    private Transform defaultParent;
    public BasePool(T prefab, Transform parent, int maxSize = 50, bool collectionCheck = false) {
        this.prefab = prefab;
        defaultParent = parent;
        InitPool(maxSize, collectionCheck);
    }

    private void InitPool(int maxSize, bool collectionCheck) {
        objectPool = new ObjectPool<T>(
            CreateObject,
            OnTakeFromPool,
            OnReturnToPool,
            OnDestroyObject,
            collectionCheck,
            maxSize
        );
    }

    private T CreateObject() {
        T obj = Object.Instantiate(prefab, defaultParent);
        obj.gameObject.SetActive(false); // Start inactive
        return obj;
    }

    protected virtual void OnTakeFromPool(T obj) {
        obj.gameObject.SetActive(true);
    }

    protected virtual void OnReturnToPool(T obj) {
        obj.gameObject.SetActive(false);
    }

    protected virtual void OnDestroyObject(T obj) {
        Object.Destroy(obj.gameObject);
    }

    public T Get() {
        return objectPool.Get();
    }

    public void Release(T obj) {
        objectPool.Release(obj);
    }
}