using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PoolManager : MonoBehaviour {
    private Dictionary<Type, object> pools = new();

    [Inject] DiContainer container;

    public void RegisterPool<T>(IComponentPool<T> pool) where T : MonoBehaviour {
        var type = typeof(T);
        if (pools.ContainsKey(type)) {
            Debug.LogWarning($"Pool for {type.Name} already registered!");
            return;
        }

        pools[type] = pool;
        Debug.Log($"Registered pool for {type.Name}");
    }

    public IComponentPool<T> GetPool<T>() where T : MonoBehaviour {
        var type = typeof(T);
        if (pools.TryGetValue(type, out var pool)) {
            return pool as IComponentPool<T>;
        }

        Debug.LogError($"No pool registered for {type.Name}!");
        return null;
    }

    public T Get<T>() where T : MonoBehaviour {
        return GetPool<T>()?.Get();
    }

    public void Release<T>(T item) where T : MonoBehaviour {
        GetPool<T>()?.Release(item);
    }
}
