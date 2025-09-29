using UnityEngine;

public class ArrowTargeting : MonoBehaviour, ITargetingVisualization {
    [Header("Arrow Components")]
    [SerializeField] private LineRenderer arrowLine;
    [SerializeField] private Transform arrowHead;

    [Header("Materials")]
    [SerializeField] private Material validTargetMaterial;
    [SerializeField] private Material invalidTargetMaterial;
    [SerializeField] private Material noTargetMaterial;

    [Header("Dependencies")]
    [SerializeField] private BoardInputManager boardInputManager;
    [SerializeField] private LayerMask boardMask;

    public Vector3 _startPosition;
    public Transform startObject;

    private TargetSelectionRequest currentRequest;
    private GameObject lastHoveredObject;

    public void Initialize(TargetSelectionRequest request) {
        currentRequest = request;
        _startPosition = startObject ? startObject.transform.position : _startPosition;
    }

    public void StartTargeting() {
        SetArrowActive(true);
        ResetArrowColor();
    }

    public void UpdateTargeting(Vector3 cursorPosition) {
        UpdateArrowPosition(_startPosition, cursorPosition);
        UpdateArrowColor(cursorPosition);
    }

    public void StopTargeting() {
        SetArrowActive(false);
        ResetArrowColor();
    }

    private void UpdateArrowPosition(Vector3 start, Vector3 end) {
        arrowLine.positionCount = 2;
        arrowLine.SetPosition(0, start);
        arrowLine.SetPosition(1, end);

        arrowHead.position = end;
        arrowHead.LookAt(start);
    }

    private void UpdateArrowColor(Vector3 targetPosition) {
        GameObject hoveredObject = GetObjectUnderPosition(targetPosition);

        if (hoveredObject == lastHoveredObject) return;
        lastHoveredObject = hoveredObject;

        Material materialToUse = DetermineArrowMaterial(hoveredObject);
        ApplyArrowMaterial(materialToUse);
    }

    private GameObject GetObjectUnderPosition(Vector3 position) {
        return boardInputManager.TryGetCursorObject(boardMask, out GameObject hitObject)
            ? hitObject : null;
    }

    private Material DetermineArrowMaterial(GameObject hoveredObject) {
            return noTargetMaterial;

    }


    private void ApplyArrowMaterial(Material material) {
        arrowLine.material = material;
        if (arrowHead.TryGetComponent<Renderer>(out var renderer))
            renderer.material = material;
    }

    private void SetArrowActive(bool active) {
        arrowLine.enabled = active;
        arrowHead.gameObject.SetActive(active);
    }

    private void ResetArrowColor() {
        ApplyArrowMaterial(noTargetMaterial);
        lastHoveredObject = null;
    }
}

