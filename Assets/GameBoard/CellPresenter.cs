using System;
using Zenject;
using UnityEngine;

/// <summary>
/// Manages individual cell's logic and visual representation
/// </summary>
public class CellPresenter : UnitPresenter, IDisposable {
    public Cell Cell;
    public Cell3DView CellView;
    
    public event Action<CellPresenter, Vector3> OnCellSizeChanged;

    private AreaView _areaObjectView;

    [Inject] IUnitRegistry unitRegistry;

    public CellPresenter(Cell model, Cell3DView view) : base(model, view) {
        Cell = model;
        CellView = view;

        Cell.OnUnitAssigned += OnUnitAssigned;
    }

    private void OnUnitAssigned(Cell cell, UnitModel newUnit) {
        // Очищуємо старий презентер
        if (_areaObjectView != null) {
            _areaObjectView.OnSizeChanged -= OnAreaSizeChanged;
        }

        // Створюємо новий презентер
        UnitPresenter unitPresenter = unitRegistry.GetPresenterByModel(newUnit);
        _areaObjectView = unitRegistry.GetView<Cell3DView>(unitPresenter);

        _areaObjectView.transform.position = CellView.transform.position;

        if (_areaObjectView != null) {
            _areaObjectView.OnSizeChanged += OnAreaSizeChanged;
            OnAreaSizeChanged(_areaObjectView.GetCurrentSize());
        }
    }

    private void OnAreaSizeChanged(Vector3 newSize) {
        if (CellView.GetCurrentSize() == newSize) return;

        Vector3 offset = CellView.GetCellOffsets();
        CellView.SetScale(newSize + offset);
        OnCellSizeChanged?.Invoke(this, newSize);
    }

    public void Dispose() {
        Cell.OnUnitAssigned -= OnUnitAssigned;
    }
}
