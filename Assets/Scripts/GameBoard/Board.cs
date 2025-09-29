using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Основной класс игровой доски
/// </summary>
public class Board : UnitModel {
    private readonly List<Row> _rows = new();

    public IReadOnlyList<Row> Rows => _rows.AsReadOnly();
    public int RowCount => _rows.Count;

    public event EventHandler<ColumnRemovedEvent> ColumnRemoved;
    public event EventHandler<ColumnAddedEvent> ColumnAdded;

    public Board(BoardConfiguration configuration = null) {
        if (configuration == null) return;

        configuration.Validate();
        
        for (int i = 0; i < configuration.RowCount; i++) {
            _rows.Add(new Row(i, configuration.RowConfigurations[i]));
        }
    }

    public Row GetRow(int rowIndex) {
        if (rowIndex < 0 || rowIndex >= _rows.Count)
            return null;
        return _rows[rowIndex];
    }

    public Cell GetCell(int rowIndex, int cellIndex) {
        return GetRow(rowIndex)?.GetCell(cellIndex);
    }

    public void AssignAreaModelToCell(int rowIndex, int cellIndex, AreaModel area) {
        var cell = GetCell(rowIndex, cellIndex);
        AssignAreaModelToCell(cell, area);
    }

    public void AssignAreaModelToCell(Cell cell, UnitModel model) {
        cell?.AssignUnit(model);
    }

    #region Column API

    /// <summary>
    /// Adds a new column to all rows
    /// </summary>
    public bool AddColumn() { 
        List<Cell> newColumn = new();
        for (int i = 0; i < _rows.Count; i++) {
            var newCell = _rows[i].AddCell();
            if (newCell == null) {
                return false;
                //return OperationResult.Failed($"Failed to add cell to row {i}: {newCell}");
            }
            newColumn.Add(newCell);
        }

        int newColumnIndex = GetCurrentColumnsCount() - 1;
        ColumnAdded?.Invoke(this, new ColumnAddedEvent(newColumn, newColumnIndex));

        return true;
    }

    /// <summary>
    /// Removes a column from all rows
    /// </summary>
    public bool RemoveColumn(int columnIndex) {
        if (_rows.Count == 0) {
            return false;
            //return OperationResult.Failed("Board is empty");
        }

        if (GetCurrentColumnsCount() <= 1) {
            return false;
            //return OperationResult.Failed("Cannot delete the last column");
        }

        List<Cell> removedColumn = new();

        // Use the helper - much cleaner!
        foreach (var (cell, row, rowIndex) in EnumerateColumn(columnIndex)) {
            removedColumn.Add(cell);

            var removedCell = row.RemoveCell(columnIndex);
            if (removedCell == null) {
                return false;
               // return OperationResult.Failed($"Failed to remove cell from row {rowIndex}");
            }
        }

        ColumnRemoved?.Invoke(this, new ColumnRemovedEvent(removedColumn, columnIndex));
        return true;
    }

    /// <summary>
    /// Gets all cells from column by its index
    /// </summary>
    public List<Cell> GetColumn(int columnIndex) {
        return EnumerateColumn(columnIndex)
            .Select(tuple => tuple.cell)
            .ToList();
    }


    /// <summary>
    /// Safely enumerates all cells in a column
    /// </summary>
    private IEnumerable<(Cell cell, Row row, int rowIndex)> EnumerateColumn(int columnIndex) {
        if (columnIndex < 0 || columnIndex >= GetCurrentColumnsCount()) {
            yield break;
        }

        for (int i = 0; i < _rows.Count; i++) {
            var row = _rows[i];
            var cell = row.GetCell(columnIndex);
            if (cell != null) {
                yield return (cell, row, i);
            }
        }
    }
    #endregion

    public int GetCurrentColumnsCount() {
        return _rows.Count > 0 ? _rows[0].CellCount : 0;
    }
}


public struct ColumnRemovedEvent : IEvent {
    public List<Cell> RemovedColumn { get; }
    public int OldCellIndex { get; internal set; }

    public ColumnRemovedEvent(List<Cell> removedColumn, int cellIndex) {
        RemovedColumn = removedColumn;
        OldCellIndex = cellIndex;
    }
}

public struct ColumnAddedEvent : IEvent {
    public List<Cell> NewColumn { get; }
    public int NewColumnIndex { get; internal set; }

    public ColumnAddedEvent(List<Cell> newColumn, int columnIndex) {
        NewColumn = newColumn;
        NewColumnIndex = columnIndex;
    }
}
