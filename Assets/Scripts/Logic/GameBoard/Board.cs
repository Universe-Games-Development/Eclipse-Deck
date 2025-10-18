using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// ќсновной класс игровой доски
/// </summary>
public class Board : UnitModel {
    private readonly List<Row> _rows = new();

    public IReadOnlyList<Row> Rows => _rows.AsReadOnly();
    public int RowCount => _rows.Count;

    public int ColumnCount => _rows.First().CellCount;

    // ”н≥версальна под≥€ дл€ зм≥н структури дошки
    public event EventHandler<BoardStructureChangedEvent> StructureChanged;

    public Board(int rows, int columns) {
        for (int rowIndex = 0; rowIndex < rows; rowIndex++) {
            _rows.Add(new Row(columns, rowIndex));
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
            }
            newColumn.Add(newCell);
        }

        int newColumnIndex = GetCurrentColumnsCount() - 1;

        // ¬икликаЇмо Їдину под≥ю з≥ списком доданих €чейок
        StructureChanged?.Invoke(this, new BoardStructureChangedEvent(
            addedCells: newColumn,
            removedCells: new List<Cell>(),
            affectedIndex: newColumnIndex
        ));

        return true;
    }

    /// <summary>
    /// Removes a column from all rows
    /// </summary>
    public bool RemoveColumn(int columnIndex) {
        if (_rows.Count == 0) {
            return false;
        }

        if (GetCurrentColumnsCount() <= 1) {
            return false;
        }

        List<Cell> removedColumn = new();

        foreach (var (cell, row, rowIndex) in EnumerateColumn(columnIndex)) {
            removedColumn.Add(cell);

            var removedCell = row.RemoveCell(columnIndex);
            if (removedCell == null) {
                return false;
            }
        }

        // ¬икликаЇмо Їдину под≥ю з≥ списком видалених €чейок
        StructureChanged?.Invoke(this, new BoardStructureChangedEvent(
            addedCells: new List<Cell>(),
            removedCells: removedColumn,
            affectedIndex: columnIndex
        ));

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

    public List<Cell> GetAllCells() {
        List<Cell> cells = new List<Cell>();
        foreach (var row in _rows) {
            cells.AddRange(row.Cells);
        }
        return cells;
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



/// <summary>
/// ”н≥версальна под≥€ дл€ зм≥н структури дошки
/// </summary>
public struct BoardStructureChangedEvent : IEvent {
    /// <summary>
    /// ячейки, €к≥ були додан≥
    /// </summary>
    public List<Cell> AddedCells { get; }

    /// <summary>
    /// ячейки, €к≥ були видален≥
    /// </summary>
    public List<Cell> RemovedCells { get; }

    /// <summary>
    /// ≤ндекс затронутого р€дка/колонки (опц≥онально)
    /// </summary>
    public int AffectedIndex { get; }

    public BoardStructureChangedEvent(
        List<Cell> addedCells,
        List<Cell> removedCells,
        int affectedIndex = -1) {

        AddedCells = addedCells ?? new List<Cell>();
        RemovedCells = removedCells ?? new List<Cell>();
        AffectedIndex = affectedIndex;
    }
}