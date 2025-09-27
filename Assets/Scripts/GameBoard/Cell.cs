using System;
using UnityEngine;

/// <summary>
/// Represents a cell with a specific number of areas
/// </summary>
public class Cell : UnitModel {
    public int RowIndex { get; }
    public int Index { get; private set; }
    public UnitModel AssignedUnit { get; private set; }

    public event Action<Cell, UnitModel> OnUnitAssigned;

    public Cell(int rowIndex, int cellIndex) {
        RowIndex = rowIndex;
        Index = cellIndex;
        Id = $"Cell_{rowIndex}_{cellIndex}";
    }

    public void AssignUnit(UnitModel unit) {
        if (AssignedUnit == unit) return;

        AssignedUnit = unit;
        OnUnitAssigned?.Invoke(this, unit);
    }

    public void UpdateCellIndex(int newCellIndex) {
        Index = newCellIndex;
    }
}