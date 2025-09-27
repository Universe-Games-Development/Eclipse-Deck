using UnityEngine;

public abstract class PoolableMonoBehaviour : MonoBehaviour, IPoolable {
    protected IComponentPool<MonoBehaviour> myPool;

    public virtual void OnPoolCreate() {
        // Базова реалізація
    }

    public virtual void OnPoolGet() {
        // Скидаємо стан об'єкта
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);
    }

    public virtual void OnPoolRelease() {
        // Очищаємо стан перед поверненням в пул
    }

    public virtual void OnPoolDestroy() {
        // Очищення ресурсів
    }

    // Зручний метод для повернення в пул
    public virtual void ReturnToPool() {
        if (myPool != null) {
            myPool.Release(this);
        } else {
            // Fallback - пробуємо знайти через PoolManager
            var poolManager = FindFirstObjectByType<PoolManager>();
            if (poolManager != null) {
                poolManager.Release(this);
            } else {
                Debug.LogWarning($"No pool found for {GetType().Name}, destroying object");
                Destroy(gameObject);
            }
        }
    }
}