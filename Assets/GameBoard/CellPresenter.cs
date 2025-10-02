using System;
using Zenject;
using UnityEngine;

/// <summary>
/// Manages individual cell's logic and visual representation
/// </summary>
public class CellPresenter : IDisposable {
    public Cell Cell;
    public Cell3DView CellView;

    public Vector3 ContentSize { get; private set; }

    public event Action<CellPresenter, Vector3> OnContentSizeChanged;

    private AreaPresenter areaPresenter;

    [Inject] IUnitRegistry unitRegistry;
    public Action<Vector3> OnSizeChanged;

    public CellPresenter(Cell model, Cell3DView view) {
        Cell = model;
        CellView = view;

        ContentSize = view.GetCurrentSize();

        Cell.OnUnitChanged += OnUnitAssigned;
    }
    
    private void OnUnitAssigned(UnitModel newUnit) {
        // Очищуємо старий презентер
        if (areaPresenter != null) {
            areaPresenter.OnSizeChanged -= HandleContentSizeChanged;
        }

        // Створюємо новий презентер
        areaPresenter = unitRegistry.GetPresenter<AreaPresenter>(newUnit);
        if (areaPresenter == null) {
            Debug.LogWarning($"Failed to get presenter for : {newUnit}");
            return;
        }

        UnitView view = areaPresenter.View;
        view.transform.position = CellView.transform.position;
        view.transform.SetParent(CellView.transform);

        areaPresenter.OnSizeChanged += HandleContentSizeChanged;
        HandleContentSizeChanged(areaPresenter.CurrentSize);
    }

    private void HandleContentSizeChanged(Vector3 newSize) {
        if (CellView.GetCurrentSize() == newSize) return;

        OnContentSizeChanged?.Invoke(this, newSize);
    }

    public void ChangeSize(Vector3 size) {
        CellView.SetSize(size);
        OnSizeChanged?.Invoke(size);
    }

    public void Dispose() {
        if (areaPresenter != null) {
            areaPresenter.OnSizeChanged -= HandleContentSizeChanged;
        }
        Cell.OnUnitChanged -= OnUnitAssigned;
    }
}
