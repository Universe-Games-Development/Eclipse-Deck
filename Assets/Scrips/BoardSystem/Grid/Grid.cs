using System;
using System.Collections.Generic;
using System.Linq;

public class Grid {
    private List<List<Field>> _fields;

    public List<List<Field>> Fields {
        get => _fields;
        protected set => _fields = value;
    }

    public Grid() {
        _fields = new List<List<Field>>();
    }

    public Grid(int rows, int columns) {
        InitializeGrid(rows, columns);
    }

    public virtual void InitializeGrid(int rows, int columns) {
        Fields = new List<List<Field>>();
        for (int x = 0; x < rows; x++) {
            Fields.Add(Enumerable.Range(0, columns).Select(y => new Field(x, y)).ToList());
        }
    }

    public virtual void UpdateGridSize(int targetRows, int targetColumns) {
        while (Fields.Count < targetRows) {
            AddRow();
        }

        while (Fields.Count > targetRows) {
            RemoveRow(Fields.Count - 1);
        }

        foreach (var row in Fields) {
            while (row.Count < targetColumns) {
                row.Add(new Field(Fields.IndexOf(row), row.Count));
            }

            while (row.Count > targetColumns) {
                row.RemoveAt(row.Count - 1);
            }
        }
    }

    #region ADD / REMOVE
    public virtual void AddRow() {
        int newRowIndex = Fields.Count; // Індекс нового ряду
        int columns = Fields.Count > 0 ? Fields[0].Count : 1; // Кількість колонок у рядку
        var newRow = Enumerable.Range(0, columns).Select(col => new Field(newRowIndex, col)).ToList();
        Fields.Add(newRow);
    }

    public virtual void RemoveRow(int rowIndex) {
        if (rowIndex >= 0 && rowIndex < Fields.Count) {
            Fields.RemoveAt(rowIndex);
        }
    }

    public virtual void RemoveColumn(int columnIndex) {
        foreach (var row in Fields) {
            if (columnIndex < row.Count)
                row.RemoveAt(columnIndex);
        }
    }
    #endregion

    public void PrintGrid() {
        foreach (var row in Fields) {
            Console.WriteLine(string.Join(", ", row.Select(t => t.column)));
            Console.WriteLine(string.Join(", ", row.Select(t => t.row)));
        }
        Console.WriteLine();
    }
}
