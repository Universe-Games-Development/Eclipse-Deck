using System;
using UnityEngine;

public class BoardInputManager : MonoBehaviour {
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _testObject;
    [SerializeField] LayerMask layerMask;
    [SerializeField] private float _raycastDistance = 10f;
    [SerializeField] private bool _isDebug = false;

    public GameObject lastHitObject;
    public GameObject hitObject;

    private void Awake() {
        if (!_camera) _camera = Camera.main;
        if (_camera == null) {
            Debug.LogError("BoardInputManager: No camera assigned!");
            enabled = false;
        }
    }

    private void Update() {
        if (!_isDebug) return;

        if (TryGetCursorData(layerMask, out Vector3 position, out var hitObject)) {
            if (_testObject) _testObject.position = position;

            if (lastHitObject != hitObject) {
                lastHitObject = hitObject;
                //Debug.Log($"Курсор попал по новому объекту: {hitObject.name} на позицию: {position}");
            }
        }
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

    public bool TryGetCursorPosition(LayerMask layerMask, out Vector3 cursorPositiont) {
         return TryGetCursorData(layerMask, out cursorPositiont, out GameObject hitObject);
    }
}
