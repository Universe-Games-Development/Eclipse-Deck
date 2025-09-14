using System;
using UnityEngine;

public class BoardInputManager : MonoBehaviour {
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _testObject;
    [SerializeField] LayerMask defaultLayerMask;
    [SerializeField] private float _raycastDistance = 10f;

    [Header ("Effects Offset")]
    [SerializeField] float boardHeightOffset = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool _isDebug = false;
    

    public GameObject lastHitObject;
    public GameObject hoveredObject;

    private void Awake() {
        if (!_camera) _camera = Camera.main;
        if (_camera == null) {
            Debug.LogError("BoardInputManager: No camera assigned!");
            enabled = false;
        }
    }

    private void Update() {
        if (!_isDebug) return;

        if (TryGetCursorData(defaultLayerMask, out Vector3 position, out var hitObject)) {
            if (_testObject) _testObject.position = position;
            //Debug.Log($"Курсор попал по новому объекту: {hitObject.name} на позицию: {position}");
            
        }

        HandleObjectHover(hitObject);
    }

    private void HandleObjectHover(GameObject newObject) {
        if (newObject == lastHitObject) return;

        if (newObject == null) {
            ClearHoveredObject();
        } else {
            StoreHoveredObject(newObject);
        }
    }
    private void StoreHoveredObject(GameObject gameObject) {
        if (lastHitObject != gameObject) {
            lastHitObject = hoveredObject = gameObject;
        }
    }

    private void ClearHoveredObject() {
        hoveredObject = null;
    }

    public bool TryGetCursorData(LayerMask layerMask, out Vector3 position, out GameObject hitObject) {
        position = default;
        hitObject = null;

        if (_camera == null) return false;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, _raycastDistance, layerMask)) {
            position = hitInfo.point;
            hitObject = hitInfo.collider.gameObject;
            return true;
        }
        return false;
    }

    public bool TryGetCursorPosition(out Vector3 cursorPositiont) {
        return TryGetCursorData(defaultLayerMask, out cursorPositiont, out _);
    }

    public bool TryGetCursorPosition(LayerMask layerMask, out Vector3 cursorPositiont) {
         return TryGetCursorData(layerMask, out cursorPositiont, out _);
    }

    public bool TryGetCursorObject(LayerMask layerMask, out GameObject hitObject) {
        return TryGetCursorData(layerMask, out _, out hitObject);
    }

    public float GetBoardHeightOffset() {
        return boardHeightOffset;
    }
}
