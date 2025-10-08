using System.Collections.Generic;

/// <summary>
/// Представляет ряд доски с колонками
/// </summary>
public class Row {
    public int Index { get; }
    private readonly List<Cell> _cells;

    public IReadOnlyList<Cell> Cells => _cells.AsReadOnly();
    public int CellCount => _cells.Count;

    public Row(int columns, int rowIndex) {
        Index = rowIndex;
        _cells = new List<Cell>();

        for (int i = 0; i < columns; i++) {
            _cells.Add(new Cell(rowIndex, i));
        }
    }

    public Cell AddCell() {
        int newCellIndex = _cells.Count;
        var newCell = new Cell(Index, newCellIndex);
        _cells.Add(newCell);
        return newCell;
    }

    public Cell RemoveCell(int cellIndex) {
        var cell = GetCell(cellIndex);
        if (cell == null)
            return null;

        _cells.RemoveAt(cellIndex);

        return cell;
    }

    public Cell GetCell(int cellIndex) {
        if (cellIndex < 0 || cellIndex >= _cells.Count)
            return null;
        return _cells[cellIndex];
    }
}
