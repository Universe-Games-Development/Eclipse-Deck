using UnityEngine;

public class BoardInputManager : MonoBehaviour {
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _testObject;
    [SerializeField] private LayerMask _boardLayer;
    [SerializeField] private float _raycastDistance = 10f;
    [SerializeField] private bool _isDebug = false;

    private void Awake() {
        if (!_camera) _camera = Camera.main;
        if (_camera == null) {
            Debug.LogError("BoardInputManager: No camera assigned!");
            enabled = false;
        }
    }

    private void Update() {
        if (!_isDebug) return;

        if (TryGetBoardCursorPosition(out var position, out var hitObject)) {
            if (_testObject) _testObject.position = position;
            Debug.Log($"Курсор попал по объекту: {hitObject.name} на позицию: {position}");
        }
    }

    public bool TryGetBoardCursorPosition(out Vector3 position, out GameObject hitObject) {
        position = default;
        hitObject = null;

        if (_camera == null) return false;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, _raycastDistance, _boardLayer)) {
            position = hitInfo.point;
            hitObject = hitInfo.collider.gameObject;
            return true;
        }
        return false;
    }

    // Перегрузка для случаев, когда не нужен hitObject
    public bool TryGetBoardCursorPosition(out Vector3 position) {
        return TryGetBoardCursorPosition(out position, out _);
    }
}
