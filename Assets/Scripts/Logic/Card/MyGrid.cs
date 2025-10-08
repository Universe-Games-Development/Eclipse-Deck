using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Універсальна сітка для зберігання даних будь-якого типу
/// </summary>
public class Grid2D<T> where T : class {
    private T[,] _cells;

    public int RowCount { get; private set; }
    public int ColumnCount { get; private set; }

    public Grid2D(int rows, int columns) {
        Resize(rows, columns);
    }

    public T Get(int row, int col) {
        if (!IsValid(row, col)) return null;
        return _cells[row, col];
    }

    public void Set(int row, int col, T value) {
        if (IsValid(row, col)) {
            _cells[row, col] = value;
        }
    }

    public bool IsValid(int row, int col) {
        return row >= 0 && row < RowCount && col >= 0 && col < ColumnCount;
    }

    public void Resize(int newRows, int newCols) {
        var newCells = new T[newRows, newCols];

        if (_cells != null) {
            int copyRows = Mathf.Min(RowCount, newRows);
            int copyCols = Mathf.Min(ColumnCount, newCols);

            for (int r = 0; r < copyRows; r++) {
                for (int c = 0; c < copyCols; c++) {
                    newCells[r, c] = _cells[r, c];
                }
            }
        }

        _cells = newCells;
        RowCount = newRows;
        ColumnCount = newCols;
    }

    public void Clear() {
        Array.Clear(_cells, 0, _cells.Length);
    }

    public IEnumerable<(int row, int col, T item)> EnumerateAll() {
        for (int r = 0; r < RowCount; r++) {
            for (int c = 0; c < ColumnCount; c++) {
                var item = _cells[r, c];
                if (item != null) {
                    yield return (r, c, item);
                }
            }
        }
    }

    public (int row, int col)? FindPosition(T item) {
        for (int r = 0; r < RowCount; r++) {
            for (int c = 0; c < ColumnCount; c++) {
                if (_cells[r, c] == item) {
                    return (r, c);
                }
            }
        }
        return null;
    }

    public bool Contains(T item) => FindPosition(item).HasValue;

    public int CountOccupied() {
        int count = 0;
        for (int r = 0; r < RowCount; r++) {
            for (int c = 0; c < ColumnCount; c++) {
                if (_cells[r, c] != null) count++;
            }
        }
        return count;
    }
}
