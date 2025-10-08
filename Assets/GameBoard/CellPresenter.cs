using System;
using UnityEngine;
using Zenject;

/// <summary>
/// Manages individual cell's logic and visual representation
/// </summary>
public class CellPresenter : IDisposable {
    public Cell Cell { get; }
    public Cell3DView CellView { get; }

    public event Action<CellPresenter, Vector3> OnSizeChanged;

    private IArea assignedArea;
    

    [Inject]
    private IUnitRegistry _unitRegistry;

    public CellPresenter(Cell model, Cell3DView view) {
        Cell = model;
        CellView = view;

        Cell.OnUnitChanged += HandleUnitChanged;
    }

    private void HandleUnitChanged(UnitModel newUnit) {
        if (newUnit == null) return;

        UnitPresenter assignedPresenter = _unitRegistry.GetPresenter<UnitPresenter>(newUnit);
        if (!(assignedPresenter.View is IArea area)) return;



        if (assignedArea != null) {
            assignedArea.OnSizeChanged -= HandleAreaSizeChanged;
            assignedArea = null;
        }

        area.OnSizeChanged += HandleAreaSizeChanged;
        CellView.AddArea(assignedPresenter.View);

        // Immediately update size based on new content
        UpdateCellSize(area.Size);
    }

    private void HandleAreaSizeChanged(Vector3 newAreaSize) {
        UpdateCellSize(newAreaSize);
    }

    private void UpdateCellSize(Vector3 areaSize) {
        Vector3 desiredSize = CalculateDesiredSize(areaSize);

        if (Vector3.Distance(CellView.Size, desiredSize) > 0.01f) {
            CellView.Resize(desiredSize);
            OnSizeChanged?.Invoke(this, desiredSize);
        }
    }

    private Vector3 CalculateDesiredSize(Vector3 contentSize) {
        // Apply padding and ensure minimum size
        return new Vector3(
            Mathf.Max(contentSize.x + CellView.cellPadding.x, 1f),
            contentSize.y, // Keep original height
            Mathf.Max(contentSize.z + CellView.cellPadding.z, 1f)
        );
    }

    public void Dispose() {
        if (assignedArea != null) {
            assignedArea.OnSizeChanged -= HandleAreaSizeChanged;
        }
        Cell.OnUnitChanged -= HandleUnitChanged;
    }
}
