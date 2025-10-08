using System;

/// <summary>
/// Represents a cell with a specific number of areas
/// </summary>
public class Cell {
    public string Id { get; }
    public readonly int ColumnIndex;
    public readonly int RowIndex;
    public UnitModel AssignedUnit { get; private set; }

    public event Action<UnitModel> OnUnitChanged;
    public bool IsEmpty => AssignedUnit == null;

    public Cell(int rowIndex, int columnIndex) {
        RowIndex = rowIndex;
        ColumnIndex = columnIndex;
        Id = $"Cell_{rowIndex}_{columnIndex}";
    }

    public void AssignUnit(UnitModel unit) {
        if (AssignedUnit == unit) return;

        AssignedUnit = unit;
        OnUnitChanged?.Invoke(unit);
    }

    public void ReleaseUnit() {
        if (AssignedUnit == null) return;

        var unitToRelease = AssignedUnit;
        AssignedUnit = null;

        OnUnitChanged?.Invoke(null);
    }
}
