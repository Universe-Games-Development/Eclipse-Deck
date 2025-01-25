using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

[Serializable]
public class GridSettings {
    private const int MIN_ROW_COUNT = 2;
    private const int MIN_COLUMN_COUNT = 2;

    public List<FieldType> _rowTypes;
    public List<FieldType> RowTypes {
        get => _rowTypes;
        private set {
            _rowTypes = value;
        }
    }

    public CellSize cellSize = new CellSize { width = 1f, height = 1f };

    [Min(2)]
    public int columns;

    public int minPlayers = 2;

    private List<FieldType> previousRowTypes;
    private int previousColumns;

    public GridSettings() {
        if (!IsValidColumns(columns)) {
            columns = MIN_COLUMN_COUNT;
        }

        if (!IsValidRowConfiguration(RowTypes)) {
            RowTypes = previousRowTypes ?? GenerateDefaultRows();
        }
    }

    private void OnValidate() {
        BackupCurrentValues();

        if (!IsValidColumns(columns)) {
            Debug.LogWarning($"Invalid column count: {columns}. Reverting to previous value or default (4).");
            columns = Mathf.Max(previousColumns, MIN_COLUMN_COUNT);
        }

        if (!IsValidRowConfiguration(RowTypes)) {
            Debug.LogWarning("Invalid row configuration. Reverting changes.");
            RowTypes = previousRowTypes ?? GenerateDefaultRows();
        }
    }

    #region Validation
    private void BackupCurrentValues() {
        if (RowTypes != null && IsValidRowConfiguration(RowTypes)) {
            previousRowTypes = new List<FieldType>(RowTypes);
        }
        if (IsValidColumns(columns)) {
            previousColumns = columns;
        }
    }

    private bool IsValidColumns(int columns) {
        return columns >= MIN_COLUMN_COUNT;
    }

    private bool IsValidRowConfiguration(List<FieldType> rows) {
        if (rows == null) {
            return false;
        }
        return HasTwoAdjacentAttackRows(rows) && rows.Count >= MIN_ROW_COUNT;
    }

    private bool HasTwoAdjacentAttackRows(List<FieldType> rowTypes) {
        var attackIndices = rowTypes
            .Select((type, index) => type == FieldType.Attack ? index : -1)
            .Where(index => index != -1)
            .ToList();

        return attackIndices.Count == MIN_ROW_COUNT && Mathf.Abs(attackIndices[1] - attackIndices[0]) == 1;
    }
    #endregion

    #region Add/Remove Columns
    public void AddColumn() {
        columns++;
    }

    public void RemoveColumn() {
        if (columns > MIN_COLUMN_COUNT) {
            columns--;
        } else {
            Debug.LogWarning("Cannot remove column. Minimum number of columns reached.");
        }
    }

    public void SetColumns(int columns) {
        if (columns != this.columns && IsValidColumns(columns)) {
            this.columns = columns;
        }
    }
    #endregion

    #region Add/Remove Rows
    public void AddRow(FieldType rowType) {
        AddRowAt(rowType, RowTypes.Count); // Вставка в кінець
    }

    public void AddRowAt(FieldType row, int index) {
        if (index < 0 || index > RowTypes.Count) {
            Debug.LogWarning($"Invalid row index: {index}. Allowed range: 0 to {RowTypes.Count}.");
            return;
        }

        var newRows = new List<FieldType>(RowTypes);
        newRows.Insert(index, row);

        if (HasTwoAdjacentAttackRows(newRows)) {
            RowTypes.Insert(index, row);
        } else {
            Debug.LogWarning("Cannot add row. Configuration does not meet the criteria of two adjacent attack rows.");
        }
    }

    public void RemoveRow() {
        RemoveRowAt(RowTypes.Count - 1); // Видалення останнього елемента
    }

    public void RemoveRowAt(int index) {
        if (index < 0 || index >= RowTypes.Count) {
            Debug.LogWarning($"Invalid row index: {index}. Allowed range: 0 to {RowTypes.Count - 1}.");
            return;
        }

        var newRows = new List<FieldType>(RowTypes);
        newRows.RemoveAt(index);

        if (HasTwoAdjacentAttackRows(newRows)) {
            RowTypes.RemoveAt(index);
        } else {
            Debug.LogWarning("Cannot remove row. Configuration does not meet the criteria of two adjacent attack rows after removal.");
        }
    }

    public void SetDefaultSettings() {
        columns = MIN_COLUMN_COUNT;
        RowTypes = GenerateDefaultRows();
    }
    #endregion

    private List<FieldType> GenerateDefaultRows() {
        return new List<FieldType> { FieldType.Attack, FieldType.Attack };
    }
}
