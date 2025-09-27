using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enhanced BoardView with dynamic layout support
/// </summary>
public class BoardView : MonoBehaviour {
    [SerializeField] private LayoutSettings layoutSettings;
    [SerializeField] private float layoutUpdateDelay = 0.1f;
    private ILayout3DHandler layout;
    private List<Cell3DView> currentCellViews = new();
    private int currentRowCount;
    private bool layoutUpdatePending = false;

    private void Awake() {
        layout = new Linear3DLayout(layoutSettings);
    }

    public void BuildBoardVisual(List<Cell3DView> cellViews, int rowCount) {
        currentCellViews = new List<Cell3DView>(cellViews);
        currentRowCount = rowCount;

        ApplyLayout();
    }

    public void AddColumn(List<Cell3DView> newCellViews, int columnIndex) {
        // Insert new cell views at appropriate positions
        for (int i = 0; i < newCellViews.Count; i++) {
            int insertIndex = CalculateInsertIndex(i, columnIndex);
            if (insertIndex <= currentCellViews.Count) {
                currentCellViews.Insert(insertIndex, newCellViews[i]);
            }
        }

        RecalculateLayout();
    }

    public void RemoveColumn(List<Cell3DView> removedCellViews, int columnIndex) {
        foreach (var view in removedCellViews) {
            currentCellViews.Remove(view);
        }

        RecalculateLayout();
    }

    private int CalculateInsertIndex(int rowIndex, int columnIndex) {
        // Calculate where to insert new cell in the flat list
        int columnsCount = GetCurrentColumnsCount();
        return rowIndex * (columnsCount + 1) + columnIndex;
    }

    private int GetCurrentColumnsCount() {
        if (currentRowCount == 0 || currentCellViews.Count == 0) return 0;
        return currentCellViews.Count / currentRowCount;
    }

    public void RecalculateLayout() {
        if (layoutUpdatePending) return;

        layoutUpdatePending = true;
        Invoke(nameof(DelayedLayoutUpdate), layoutUpdateDelay);
    }

    private void DelayedLayoutUpdate() {
        layoutUpdatePending = false;
        ApplyLayout();
    }

    public void ApplyLayout() {
        if (currentCellViews.Count == 0) return;

        // Get current column count
        int columnsCount = GetCurrentColumnsCount();

        // Calculate layout points
        var rowSettings = new RowLayoutSettings(columnsCount, currentRowCount);
        var result = layout.CalculateLayout(currentCellViews.Count, rowSettings);
        List<LayoutPoint> layoutPoints = result.Points;

        // Apply positions and account for individual cell sizes
        for (int i = 0; i < currentCellViews.Count && i < layoutPoints.Count; i++) {
            var view = currentCellViews[i];
            var layoutPoint = layoutPoints[i];
            var transform = view.transform;

            if (transform != null) {
                // Set parent if not already set
                if (transform.parent != this.transform) {
                    transform.SetParent(this.transform);
                }

                // Apply position from layout
                transform.position = this.transform.position + layoutPoint.position;
                transform.rotation = layoutPoint.rotation;
            }
        }
    }

}
