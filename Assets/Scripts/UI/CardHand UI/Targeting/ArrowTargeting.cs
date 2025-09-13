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

    private Vector3 startPosition;
    private TargetSelectionRequest currentRequest;
    private GameObject lastHoveredObject;

    public void Initialize(Vector3 startPos, TargetSelectionRequest request) {
        startPosition = startPos;
        currentRequest = request;
    }

    public void StartTargeting() {
        SetArrowActive(true);
        ResetArrowColor();
    }

    public void UpdateTargeting(Vector3 cursorPosition) {
        UpdateArrowPosition(startPosition, cursorPosition);
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
        if (hoveredObject == null)
            return noTargetMaterial;

        UnitModel gameUnit = GetGameUnitFromObject(hoveredObject);
        if (gameUnit == null)
            return noTargetMaterial;

        ValidationResult validation = currentRequest.Requirement.IsValid(gameUnit, currentRequest.Initiator.GetPlayer());
        return validation.IsValid ? validTargetMaterial : invalidTargetMaterial;
    }

    private UnitModel GetGameUnitFromObject(GameObject obj) {
        return obj.TryGetComponent<UnitPresenter>(out var presenter)
            ? presenter.GetInfo() : null;
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

