using System;
using UnityEngine;

// Cells will update its size depends on others AreaView inside it
// Logic in CellPresenter
public class Cell3DView : AreaView {
    [SerializeField] Vector3 cellOffset;
    public Vector3 GetCellOffsets() {
        return cellOffset;
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
}
