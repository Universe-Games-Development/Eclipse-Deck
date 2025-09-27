using UnityEngine;

public abstract class PoolableMonoBehaviour : MonoBehaviour, IPoolable {
    protected IComponentPool<MonoBehaviour> myPool;

    public virtual void OnPoolCreate() {
        // ������ ���������
    }

    public virtual void OnPoolGet() {
        // ������� ���� ��'����
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);
    }

    public virtual void OnPoolRelease() {
        // ������� ���� ����� ����������� � ���
    }

    public virtual void OnPoolDestroy() {
        // �������� �������
    }

    // ������� ����� ��� ���������� � ���
    public virtual void ReturnToPool() {
        if (myPool != null) {
            myPool.Release(this);
        } else {
            // Fallback - ������� ������ ����� PoolManager
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