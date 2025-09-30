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

    [SerializeField] Vector3 _arrowEndOffset = new();
    public Vector3 _startPosition;
    public Transform startObject;

    public void Initialize() {
        _startPosition = startObject ? startObject.transform.position : _startPosition;
    }

    public void StartTargeting() {
        SetArrowActive(true);
        ResetArrowColor();
        UpdateHoverStatus(TargetValidationState.None);
    }

    public void UpdateTargeting(Vector3 cursorPosition) {
        Vector3 endResultPosition = cursorPosition + _arrowEndOffset;
        UpdateArrowPosition(_startPosition, endResultPosition);
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


    private GameObject GetObjectUnderPosition(Vector3 position) {
        return boardInputManager.TryGetCursorObject(boardMask, out GameObject hitObject)
            ? hitObject : null;
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
    }

    public void UpdateHoverStatus(TargetValidationState state) {
        Material material = noTargetMaterial;
        switch (state) {
            case TargetValidationState.Valid:
                material = validTargetMaterial;
                break;
            case TargetValidationState.WrongTarget:
                material = invalidTargetMaterial;
                break;
                
        }
        ApplyArrowMaterial(material);
    }
}

