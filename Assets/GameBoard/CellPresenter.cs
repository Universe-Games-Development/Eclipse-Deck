using System;
using UnityEngine;

public class CellPresenter : IDisposable {
    public Cell Cell { get; }
    public Cell3DView CellView { get; }

    public event Action<CellPresenter, Vector3> OnSizeChanged;

    private IArea assignedArea;
    
    private IUnitRegistry _unitRegistry;

    public CellPresenter(Cell model, Cell3DView view, IUnitRegistry _unitRegistry) {
        Cell = model;
        CellView = view;
        this._unitRegistry = _unitRegistry;

        Cell.OnUnitChanged += HandleUnitChanged;
        HandleUnitChanged(Cell.AssignedUnit);
    }

    private void HandleUnitChanged(UnitModel newUnit) {
        if (newUnit == null) return; // Якщо вміст видалено, тут логіка завершується

        if (assignedArea != null) {
            assignedArea.OnSizeChanged -= HandleAreaSizeChanged;
            assignedArea = null;
            CellView.RemoveAreaView(); // Важливо: видалити старий View
                                       // Додатково: скинути розмір комірки до мінімального
                                       // UpdateCellSize(Vector3.zero); 
        }

        // Крок 2: Додавання нового вмісту
        UnitPresenter assignedPresenter = _unitRegistry.GetPresenter<UnitPresenter>(newUnit);
        if (!(assignedPresenter.View is IArea area)) return;

        assignedArea = area;
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
