using UnityEngine;

public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour {
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static T Instance {
        get {
            if (_applicationIsQuitting) {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                return null;
            }

            lock (_lock) {
                if (_instance == null) {
                    _instance = (T)FindFirstObjectByType(typeof(T));

                    if (_instance == null) {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = $"{typeof(T).Name} (Singleton)";

                        Debug.Log($"[Singleton] Created new instance of '{typeof(T)}'");
                    } else {
                        Debug.Log($"[Singleton] Using existing instance: '{_instance.gameObject.name}'");
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake() {
        if (_instance == null) {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        } else if (_instance != this) {
            Debug.LogWarning($"[Singleton] Multiple instances of '{typeof(T)}' detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit() {
        _applicationIsQuitting = true;
    }

    protected virtual void OnDestroy() {
        if (_instance == this) {
            _applicationIsQuitting = true;
        }
    }
}
