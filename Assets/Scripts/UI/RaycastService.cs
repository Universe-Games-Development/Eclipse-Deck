using UnityEngine;

public class RaycastService : MonoBehaviour {
    [SerializeField] private float rayDistance = 20f;

    public RaycastHit? GetRaycastHit(Camera raycastCamera) {
        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 1f); // Debug ray for visualization

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, -1, QueryTriggerInteraction.Ignore)) {
            Debug.DrawLine(ray.origin, hit.point, Color.green, 1f); // Debug line to hit point
            return hit;
        }
        return null;
    }

    public Vector3? GetRayMousePosition() {
        var hit = GetRaycastHit(Camera.main);
        return hit?.point ?? null;
    }
}
