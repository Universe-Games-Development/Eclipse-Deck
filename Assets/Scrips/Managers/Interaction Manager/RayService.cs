using UnityEngine;
using UnityEngine.SceneManagement;

public class RayService : MonoBehaviour {
    public Camera raycastCamera;

    [Header("Table View")]
    [SerializeField] private float tableRayDistance = 20f;

    private void Awake() {
        raycastCamera = Camera.main;
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
        raycastCamera = Camera.main;
    }

    public Vector3? GetRayHitPosition(float distance = 20f) {
        return GetRaycastHit(distance)?.point;
    }

    public GameObject GetRayObject(float distance = 20f) {
        return GetRaycastHit(distance)?.collider.gameObject;
    }

    private RaycastHit? GetRaycastHit(float distance) {
        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * distance, Color.red, 1f); // Debug ray for visualization

        if (Physics.Raycast(ray, out RaycastHit hit, distance, -1, QueryTriggerInteraction.Ignore)) {
            Debug.DrawLine(ray.origin, hit.point, Color.green, 1f); // Debug line to hit point
            return hit;
        }
        return null;
    }

    public Vector3 GetRayMousePosition() {
        var hit = GetRaycastHit(20f);
        return hit?.point ?? Vector3.zero;
    }
}
