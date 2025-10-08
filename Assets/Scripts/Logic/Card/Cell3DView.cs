using System;
using UnityEngine;

// Cells will update its size depends on others AreaView inside it
// Logic in CellPresenter
public class Cell3DView : AreaView {
    [SerializeField] public Vector3 cellOffset;

    [SerializeField] public Vector3 areaOffset = Vector3.zero;

    public void PositionArea(Transform area) {
        area.transform.SetParent(transform);
        area.position = transform.position + areaOffset;
    }
}


public class AreaView : InteractableView {
    [SerializeField] protected Transform _bodyToScale;
    public event Action<Vector3> OnSizeChanged;

    public virtual void SetSize(Vector3 scale) {
        _bodyToScale.localScale = scale;
        OnSizeChanged?.Invoke(scale);
    }

    public virtual Vector3 GetCurrentSize() {
        return _bodyToScale.localScale;
    }

    public Transform GetBody() {
        return _bodyToScale ?? transform;
    }
}
