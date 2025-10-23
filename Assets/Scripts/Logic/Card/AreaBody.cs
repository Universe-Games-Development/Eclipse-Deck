using System;
using UnityEngine;

public class AreaBody : MonoBehaviour , IArea {
    [SerializeField] private Transform _bodyToScale;

    public event Action<Vector3> OnSizeChanged;

    public Vector3 Size => _bodyToScale.localScale;

    protected void Awake() {
        if (_bodyToScale == null)
            _bodyToScale = transform;
    }

    public Transform GetBody() => _bodyToScale;

    public void SetBody(Transform body) {
        // Apply previous size
        if (_bodyToScale != null) {
            body.localScale = _bodyToScale.localScale;
        }

        _bodyToScale = body;
    }

    public void Resize(Vector3 newSize) {
        if (_bodyToScale == null) return;

        _bodyToScale.localScale = newSize;
        OnSizeChanged?.Invoke(newSize);
    }
}

