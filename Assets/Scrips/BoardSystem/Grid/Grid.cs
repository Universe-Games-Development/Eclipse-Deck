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
            var row = Fields[rowIndex];
            foreach (var field in row) {
                field.NotifyFieldRemoval(); // Сповіщення поля
            }
            Fields.RemoveAt(rowIndex);
        }
    }

    public virtual void RemoveColumn(int columnIndex) {
        foreach (var row in Fields) {
            if (columnIndex < row.Count) {
                var field = row[columnIndex];
                field.NotifyFieldRemoval(); // Сповіщення поля
                row.RemoveAt(columnIndex);
            }
        }
    }

    #endregion

    #region GETTERS
    public List<Field> GetAdjacentFields(Field currentField) {
        var adjacentFields = new List<Field>();
        var offsets = CompassUtil.GetOffsets();

        foreach (var (rowOffset, colOffset) in offsets) {
            int newRow = currentField.row + rowOffset;
            int newCol = currentField.column + colOffset;

            if (newRow >= 0 && newRow < Fields.Count && newCol >= 0 && newCol < Fields[0].Count) {
                adjacentFields.Add(Fields[newRow][newCol]);
            }
        }

        return adjacentFields;
    }

    public List<Field> GetFieldsToMove(Field currentField, int pathAmount, Direction direction, bool reversed = false) {
        List<Field> path = new();

        if (reversed) {
            direction = CompassUtil.GetOppositeDirection(direction);
        }

        var (rowOffset, colOffset) = CompassUtil.DirectionOffsets[direction];

        for (int i = 1; i <= pathAmount; i++) {
            int newRow = currentField.row + rowOffset * i;
            int newCol = currentField.column + colOffset * i;

            if (newRow >= 0 && newRow < Fields.Count && newCol >= 0 && newCol < Fields[0].Count) {
                path.Add(Fields[newRow][newCol]);
            } else {
                break;
            }
        }

        return path;
    }

    public List<Field> GetFlankFields(Field field, int flankSize, bool isReversed) {
        List<Field> flankFields = new List<Field>();

        // Ліва сторона
        List<Field> leftFlank = GetFieldsToMove(field, flankSize, Direction.West, isReversed);
        flankFields.AddRange(leftFlank);

        // Права сторона
        List<Field> rightFlank = GetFieldsToMove(field, flankSize, Direction.East, isReversed);
        flankFields.AddRange(rightFlank);

        return flankFields;
    }

    public bool FieldExists(Field field) {
        return Fields.Any(column => column?.Contains(field) == true);

    }

    public Field GetFieldAt(int row, int column) {
        return Fields[row][column];
    }
    #endregion

    public bool IsFieldInEnemyZone(Field field) {
        return field.Owner is Enemy;
    }
    public void PrintGrid() {
        foreach (var row in Fields) {
            Console.WriteLine(string.Join(", ", row.Select(t => t.column)));
            Console.WriteLine(string.Join(", ", row.Select(t => t.row)));
        }
        Console.WriteLine();
    }
}
