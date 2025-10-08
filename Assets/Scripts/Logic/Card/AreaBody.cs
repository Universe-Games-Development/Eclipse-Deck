using System;
using UnityEngine;

public class AreaBody : MonoBehaviour , IArea {
    [SerializeField] private Transform _bodyToScale;

    public event Action<Vector3> OnSizeChanged;

    private Transform BodyToScale => _bodyToScale != null ? _bodyToScale : transform;

    public Vector3 Size => BodyToScale.localScale;

    protected void Awake() {
        if (_bodyToScale == null)
            _bodyToScale = transform;
    }

    public Transform GetBody() => BodyToScale;
    public void SetBody(Transform body) {
        // Apply previous size
        if (_bodyToScale != null) {
            body.localScale = _bodyToScale.localScale;
        }

        _bodyToScale = body;
    }

    public void Resize(Vector3 newSize) {
        if (BodyToScale == null) return;

        BodyToScale.localScale = newSize;
        OnSizeChanged?.Invoke(newSize);
    }
}

