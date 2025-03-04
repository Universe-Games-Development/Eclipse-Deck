using UnityEngine;
using UnityEngine.SceneManagement;

public class RaycastService : MonoBehaviour {
    public Camera raycastCamera;

    [Header("Table View")]
    [SerializeField] private float rayDistance = 20f;

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

    public Vector3? GetRayHitPosition() {
        return GetRaycastHit()?.point;
    }

    public GameObject GetRayObject() {
        return GetRaycastHit()?.collider.gameObject;
    }

    private RaycastHit? GetRaycastHit() {
        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 1f); // Debug ray for visualization

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, -1, QueryTriggerInteraction.Ignore)) {
            Debug.DrawLine(ray.origin, hit.point, Color.green, 1f); // Debug line to hit point
            return hit;
        }
        return null;
    }

    public Vector3? GetRayMousePosition() {
        var hit = GetRaycastHit();
        return hit?.point ?? null;
    }
}
