using UnityEngine;

[RequireComponent(typeof(RayService))]
public class InteractionManager : MonoBehaviour {
    [SerializeField] private GameObject hoveredInteractable;

    public GameObject HoveredInteractable => hoveredInteractable;

    private RayService rayService;
    private void Awake() {
        rayService = GetComponent<RayService>();
    }

    private void Update() {
        GameObject gameObject = rayService.GetRayObject();
        if (gameObject && gameObject != hoveredInteractable) {
            hoveredInteractable = gameObject;
        } else {
            hoveredInteractable = null;
        }
    }
}
