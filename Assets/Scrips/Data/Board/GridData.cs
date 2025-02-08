using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridData {
    public int rows;
    public int columns;
    public List<List<int>> grid;

    public Direction GridDirection { get; internal set; }

    public GridData(int rows, int columns, Direction gridDirection) {
        this.rows = Mathf.Max(1, rows);
        this.columns = Mathf.Max(1, columns);
        GridDirection = gridDirection;
        InitializeGrid();
    }

    private void InitializeGrid() {
        grid = new List<List<int>>();
        for (int i = 0; i < rows; i++) {
            grid.Add(new List<int>(new int[columns])); // Заповнюємо нулями
        }
    }

    public void ResizeGrid(int newRows, int newColumns) {
        newRows = Mathf.Max(1, newRows);
        newColumns = Mathf.Max(1, newColumns);

        // Перевіряємо, чи поточна кількість рядів та колонок вже відповідає необхідним
        if (rows == newRows && columns == newColumns) {
            return;
        }

        rows = newRows;
        columns = newColumns;

        // Додаємо або видаляємо рядки
        while (grid.Count < rows) {
            grid.Add(new List<int>(new int[columns]));
        }
        while (grid.Count > rows) {
            grid.RemoveAt(grid.Count - 1);
        }

        // Додаємо або видаляємо стовпці
        foreach (var row in grid) {
            while (row.Count < columns) {
                row.Add(0);
            }
            while (row.Count > columns) {
                row.RemoveAt(row.Count - 1);
            }
        }
    }


    public void RandomizeGrid() {
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                grid[i][j] = Random.Range(0, 2);
            }
        }
    }

    public void SetAllValues(int value) {
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                grid[i][j] = value;
            }
        }
    }

    public bool IsGridValid(int minRows, int minColumns) {
        if (grid == null) {
            return false;
        }

        if (rows < minRows) {
            return false;
        }

        foreach (var row in grid) {
            if (row.Count < minColumns) {
                return false;
            }
        }

        return true;
    }

}
