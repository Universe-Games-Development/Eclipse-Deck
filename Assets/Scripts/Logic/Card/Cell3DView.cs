using System;
using UnityEngine;

// Cells will update its size depends on others AreaView inside it
// Logic in CellPresenter
public class Cell3DView : InteractableView, IArea {

    [SerializeField] private Vector3 areaOffset = Vector3.zero;
    [SerializeField] public Vector3 cellPadding = new Vector3(0.2f, 0f, 0.2f);

    [Header ("Serialized for Runtime Debug")]
    [SerializeField] private UnitView assignedBody;

    #region IArea 
    [SerializeField] private AreaBody ownBody;
    public event Action<Vector3> OnSizeChanged;

    public Vector3 Size => ownBody ? ownBody.Size : Vector3.one;
    public void Resize(Vector3 newSize) {
        ownBody.Resize(newSize);
        OnSizeChanged?.Invoke(newSize);
    }
    #endregion

    public void AddArea(UnitView areaView) {
        if (assignedBody == areaView) return;
        RemoveAreaView();

        assignedBody = areaView;

        Transform areaTransform = assignedBody.transform;
        areaTransform.SetParent(transform);
        areaTransform.localPosition = areaOffset;
    }

    public void RemoveAreaView() {
        if (assignedBody == null) return;
        Destroy(assignedBody.gameObject);
    }

    public void Clear() {
        RemoveAreaView();
        Resize(Vector3.one);
    }
}

public interface IArea {
    Vector3 Size { get; }
    void Resize(Vector3 newSize);
    event Action<Vector3> OnSizeChanged;
}
