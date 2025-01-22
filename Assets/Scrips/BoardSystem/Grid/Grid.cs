using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid {
    public Func<Grid, UniTask> OnGridChangedAsync;
    public Action<Grid> OnGridChanged;

    public Action<Field> OnAddField;
    public Action<Field> OnRemoveField;


    public CellSize cellSize = new CellSize { width = 1f, height = 1f };
    private List<List<Field>> _fields;

    public List<List<Field>> Fields {
        get => _fields;
        protected set => _fields = value;
    }


    public Grid() {
        _fields = new List<List<Field>>();
    }

    public Grid(int rows, int columns, CellSize cellSize) {
        this.cellSize = cellSize;
        InitializeGrid(rows, columns);
    }

    public virtual void InitializeGrid(int rows, int columns) {
        Fields = new List<List<Field>>();
        UpdateGridSize(rows, columns);
        // spawn grid + find center
    }


    public virtual void UpdateGridSize(int targetRows, int targetColumns) {
        AdjustSize(Fields.Count, targetRows, _ => AddRow(), row => RemoveRow(row));
        foreach (var row in Fields) {
            AdjustSize(row.Count, targetColumns, _ => AddColumn(), row => RemoveColumn(row));
        }
        // update grid + find center
        OnGridChangedAsync?.Invoke(this);
        OnGridChanged?.Invoke(this);
    }


    private void AdjustSize(int currentSize, int targetSize, Action<int> addAction, Action<int> removeAction) {
        while (currentSize < targetSize) {
            addAction(currentSize++);
        }
        while (currentSize > targetSize) {
            removeAction(--currentSize);
        }
    }


    #region ADD / REMOVE
    public virtual void AddRow() {
        int newRowIndex = Fields.Count; // Індекс нового ряду
        int columns = Fields.Count > 0 ? Fields[0].Count : 1; // Кількість колонок у рядку
        var newRow = Enumerable.Range(0, columns)
            .Select(col => new Field(newRowIndex, col))
            .ToList();
        Fields.Add(newRow);

        foreach (var field in newRow) {
            OnAddField?.Invoke(field);
        }
    }

    public virtual void AddColumn() {
        foreach (var row in Fields) {
            int columnIndex = row.Count;
            Field field = new(Fields.IndexOf(row), columnIndex);
            row.Add(field);
            OnAddField?.Invoke(field);
        }
    }

    public virtual void RemoveRow(int rowIndex) {
        if (rowIndex >= 0 && rowIndex < Fields.Count) {
            var row = Fields[rowIndex];
            foreach (var field in row) {
                OnRemoveField?.Invoke(field);
            }
            Fields.RemoveAt(rowIndex);
        }
    }

    public virtual void RemoveColumn(int columnIndex) {
        foreach (var row in Fields) {
            if (columnIndex < row.Count) {
                var field = row[columnIndex];
                row.RemoveAt(columnIndex);
                OnRemoveField?.Invoke(field);
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

    public List<Field> GetFieldsInDirection(Field currentField, int pathAmount, Direction direction, bool reversed = false) {
        List<Field> fields = new();

        if (reversed) {
            direction = CompassUtil.GetOppositeDirection(direction);
        }

        var (rowOffset, colOffset) = CompassUtil.DirectionOffsets[direction];

        for (int i = 1; i <= pathAmount; i++) {
            int newRow = currentField.row + rowOffset * i;
            int newCol = currentField.column + colOffset * i;

            if (newRow >= 0 && newRow < Fields.Count && newCol >= 0 && newCol < Fields[0].Count) {
                fields.Add(Fields[newRow][newCol]);
            } else {
                break;
            }
        }

        return fields;
    }

    public List<Field> GetFlankFields(Field field, int flankSize, bool isReversed) {
        List<Field> flankFields = new();

        // Ліва сторона
        List<Field> leftFlank = GetFieldsInDirection(field, flankSize, Direction.West, isReversed);
        flankFields.AddRange(leftFlank);

        // Права сторона
        List<Field> rightFlank = GetFieldsInDirection(field, flankSize, Direction.East, isReversed);
        flankFields.AddRange(rightFlank);

        return flankFields;
    }

    public bool FieldExists(Field field) {
        return Fields.Any(column => column?.Contains(field) == true);

    }

    public Field GetFieldAt(int row, int column) {
        if (Fields == null) {
            return null;
        }
        if (row < 0 || column < 0 || row > Fields.Count - 1 || column > Fields[0].Count - 1) {
            return null;
        }
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

    public Vector2Int? GetGridIndexByWorld(Transform origin, Vector3 worldPosition) {
        Vector3 localPosition = origin.InverseTransformPoint(worldPosition);
        int x = Mathf.FloorToInt((localPosition.x) / cellSize.width);
        int y = Mathf.FloorToInt((localPosition.z) / cellSize.height);

        if (x < 0 || x >= Fields.Count || y < 0 || y >= Fields[0].Count) {
            return null;
        }

        return new Vector2Int(x, y);
    }
}
