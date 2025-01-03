using UnityEngine;
using UnityEngine.SceneManagement;

public class RayService : MonoBehaviour {
    public Camera raycastCamera;

    [Header("Table View")]
    [SerializeField] private float tableRayDistance = 20f;
    [SerializeField] private LayerMask itemsLayer;
    

    private void Awake() {
        raycastCamera = Camera.main;
        itemsLayer = LayerMask.GetMask("Items");
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoad; // Очищення підписки
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
        raycastCamera = Camera.main;
    }

    public GameObject GetRayObject(float distance = 20f, LayerMask? layerMask = null) {
        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);

        // Використовуємо шар, якщо задано
        var hit = GetRaycastHit(ray, distance);
        return hit?.collider.gameObject; // Перевірка null і повернення об'єкта
    }

    public GameObject GetRayObjectFromMouse() {
        return GetRayObject(tableRayDistance);
    }

    private RaycastHit? GetRaycastHit(Ray ray, float distance) {
        if (Physics.Raycast(ray, out RaycastHit hit, distance, itemsLayer, QueryTriggerInteraction.Ignore)) {
            return hit;
        }
        return null;
    }
}
