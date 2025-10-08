using System;
using Unity.Loading;
using UnityEngine;
using Zenject;

/// <summary>
/// Manages individual cell's logic and visual representation
/// </summary>
public class CellPresenter : IDisposable {
    public Cell Cell;
    public Cell3DView CellView;

    public Vector3 ContentSize { get; private set; }
    public Vector3 DesiredSize { get; private set; }
    public Vector3 ActualSize { get; private set; }

    public event Action<CellPresenter, Vector3> OnDesiredSizeChanged;

    private AreaPresenter areaPresenter;

    [Inject] IUnitRegistry unitRegistry;
    public Action<Vector3> OnSizeChanged;

    public CellPresenter(Cell model, Cell3DView view) {
        Cell = model;
        CellView = view;

        ContentSize = view.GetCurrentSize();
        ActualSize = CellView.transform.localScale;

        Cell.OnUnitChanged += OnUnitAssigned;
    }

    private void OnUnitAssigned(UnitModel newUnit) {
        // Відписуємося від старого
        if (areaPresenter != null) {
            areaPresenter.OnSizeChanged -= HandleContentSizeChanged;
        }

        if (newUnit == null) {
            areaPresenter = null;
            UpdateContentSize(Vector3.zero);
            return;
        }

        // Підписуємося на новий
        areaPresenter = unitRegistry.GetPresenter<AreaPresenter>(newUnit);
        if (areaPresenter == null) {
            Debug.LogWarning($"Failed to get presenter for: {newUnit}");
            return;
        }

        UnitView view = areaPresenter.View;
        CellView.PositionArea(view.transform);

        areaPresenter.OnSizeChanged += HandleContentSizeChanged;

        // Ініціалізуємо розмір
        Vector3 contentSize = areaPresenter.AreaView.GetCurrentSize();
        UpdateContentSize(contentSize);
    }

    private void UpdateContentSize(Vector3 newContentSize) {
        if (Vector3.Distance(ContentSize, newContentSize) < 0.01f) return;

        ContentSize = newContentSize;

        // Обчислюємо бажаний розмір на основі контенту + padding
        Vector3 newDesiredSize = CalculateDesiredSize(ContentSize);

        if (Vector3.Distance(DesiredSize, newDesiredSize) < 0.01f) return;

        DesiredSize = newDesiredSize;

        // Повідомляємо BoardPresenter про БАЖАНИЙ розмір
        OnDesiredSizeChanged?.Invoke(this, DesiredSize);
    }

    private Vector3 CalculateDesiredSize(Vector3 contentSize) {
        // Можна додати padding
        const float padding = 0.2f;

        return new Vector3(
            Mathf.Max(contentSize.x + padding, 1f), // мінімум 1
            contentSize.y,
            Mathf.Max(contentSize.z + padding, 1f)  // мінімум 1
        );
    }

    private void HandleContentSizeChanged(Vector3 newSize) {
        if (CellView.GetCurrentSize() == newSize) return;

        ContentSize = newSize;
        OnDesiredSizeChanged?.Invoke(this, newSize);
    }

    public void ApplyActualSize(Vector3 size) {
        if (Vector3.Distance(ActualSize, size) < 0.01f) return;

        ActualSize = size;
        CellView.SetSize(size);
    }

    public void ApplyPosition(Vector3 position, Quaternion rotation) {
        CellView.transform.localPosition = position;
        CellView.transform.localRotation = rotation;
    }

    public void Dispose() {
        if (areaPresenter != null) {
            areaPresenter.OnSizeChanged -= HandleContentSizeChanged;
        }
        Cell.OnUnitChanged -= OnUnitAssigned;
    }
}
