using UnityEngine;

public class ArrowVisualizationController : MonoBehaviour {
    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowUpdateRate = 30f; // Знижуємо FPS для оптимізації

    private GameObject currentArrow;
    private Vector3 startPosition;
    private System.Func<Vector3> getEndPosition;
    private bool isShowing = false;
    private float lastUpdateTime;

    public void ShowArrow(Vector3 start, System.Func<Vector3> endPositionGetter) {
        Hide();

        startPosition = start;
        getEndPosition = endPositionGetter;
        isShowing = true;

        CreateArrow();
    }

    public void Hide() {
        isShowing = false;
        if (currentArrow != null) {
            Destroy(currentArrow);
            currentArrow = null;
        }
        getEndPosition = null;
    }

    private void Update() {
        if (isShowing && currentArrow != null && getEndPosition != null) {
            if (Time.time - lastUpdateTime >= 1f / arrowUpdateRate) {
                UpdateArrowVisual();
                lastUpdateTime = Time.time;
            }
        }
    }

    private void CreateArrow() {
        if (arrowPrefab == null) return;

        currentArrow = Instantiate(arrowPrefab);
        UpdateArrowVisual();
    }

    private void UpdateArrowVisual() {
        if (currentArrow == null || getEndPosition == null) return;

        Vector3 endPos = getEndPosition();
        Vector3 direction = endPos - startPosition;

        if (direction.magnitude < 0.1f) return; // Skip very small movements

        currentArrow.transform.position = startPosition + direction * 0.5f;
        currentArrow.transform.rotation = Quaternion.LookRotation(direction);
        currentArrow.transform.localScale = new Vector3(1f, 1f, direction.magnitude);
    }
}
