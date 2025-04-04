using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component {
    private static T _instance;
    public static T Instance {
        get {
            if (_instance == null) {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null) {
                    GameObject newObj = new GameObject("Auto gen " + typeof(T));
                    _instance = newObj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake() {
        if (_instance == null) {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}
