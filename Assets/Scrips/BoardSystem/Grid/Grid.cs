
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid {
    private GridSettings _settings;
    private List<List<Field>> _fields;
    public List<List<Field>> Fields {
        get => _fields;
        protected set => _fields = value;
    }

    public virtual GridUpdateData UpdateGrid(GridSettings gridSettings) {
        _settings = gridSettings;
        return BuildGrid(_settings.RowTypes.Count, _settings.columns);
    }


    public virtual GridUpdateData BuildGrid(int targetRows, int targetColumns) {
        GridUpdateData gridUpdateData = new GridUpdateData(); // Ініціалізація gridUpdateData
        gridUpdateData.IsInitialized = Fields == null;
        if (Fields == null) {
            Fields = new List<List<Field>>();
        }
        
        // adjust rows
        AdjustSize(Fields.Count, targetRows, _ => AddRow(), row => RemoveRow(row), out List<Field> rowRemovedFields, out List<Field> rowAddedFields);
        gridUpdateData.removedFields.AddRange(rowRemovedFields);
        gridUpdateData.addedFields.AddRange(rowAddedFields);

        // adjust columns
        foreach (var row in Fields) {
            AdjustSize(row.Count, targetColumns, _ => AddColumn(), col => RemoveColumn(col), out List<Field> colRemovedFields, out List<Field> colAddedFields);
            gridUpdateData.removedFields.AddRange(colRemovedFields);
            gridUpdateData.addedFields.AddRange(colAddedFields);
        }

        return gridUpdateData;
    }


        private void AdjustSize(int currentSize, int targetSize, Func<int, List<Field>> addAction, Func<int, List<Field>> removeAction, out List<Field> removedFields, out List<Field> addedFields) {
        removedFields = new List<Field>();
        addedFields = new List<Field>();

        while (currentSize < targetSize) {
            addedFields.AddRange(addAction(currentSize++));
        }
        while (currentSize > targetSize) {
            removedFields.AddRange(removeAction(--currentSize));
        }
    }


    #region ADD / REMOVE
    public virtual List<Field> AddRow() {
        int newRowIndex = Fields.Count; // Індекс нового ряду
        int columns = Fields.Count > 0 ? Fields[0].Count : 1; // Кількість колонок у рядку
        var newRow = Enumerable.Range(0, columns)
            .Select(col => new Field(newRowIndex, col))
            .ToList();

        Fields.Add(newRow);

        return newRow;
    }


    public virtual List<Field> RemoveRow(int rowIndex) {
        if (rowIndex < 0 || rowIndex >= Fields.Count) {
            // Можна кинути виняток або записати в лог
            return null;
        }
        var row = Fields[rowIndex];
        Fields.RemoveAt(rowIndex);
        return row;
    }



    public virtual List<Field> AddColumn() {

        List<Field> fieldsToAdd = new List<Field>();

        // Якщо немає рядків, створити перший рядок
        if (Fields.Count == 0) {
            AddRow();
        }

        foreach (var row in Fields) {
            int columnIndex = row.Count;
            Field field = new Field(Fields.IndexOf(row), columnIndex);
            row.Add(field);
            fieldsToAdd.Add(field);
        }

        return fieldsToAdd;
    }

    public virtual List<Field> RemoveColumn(int columnIndex) {
        if (Fields.Count == 0) {
            throw new InvalidOperationException("Fields is not initialized or empty.");
        }

        List<Field> fieldsToRemove = new List<Field>();

        foreach (var row in Fields) {
            if (row != null && columnIndex >= 0 && columnIndex < row.Count) {
                var field = row[columnIndex];
                row.RemoveAt(columnIndex);
                fieldsToRemove.Add(field);
            }
        }

        return fieldsToRemove;
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
        List<Field> fields = new List<Field>();

        if (reversed) {
            direction = CompassUtil.GetOppositeDirection(direction);
        }

        var (rowOffset, colOffset) = CompassUtil.DirectionOffsets[direction];

        for (int i = 1; i <= pathAmount; i++) {
            int newRow = currentField.row + rowOffset * i;
            int newCol = currentField.column + colOffset * i;

            if (newRow >= 0 && newRow < this.Fields.Count && newCol >= 0 && newCol < this.Fields[0].Count) {
                fields.Add(this.Fields[newRow][newCol]);
            } else {
                break;
            }
        }

        return fields;
    }

    public List<Field> GetFlankFields(Field field, int flankSize, bool isReversed) {
        List<Field> flankFields = new List<Field>();

        // Ліва сторона
        List<Field> leftFlank = GetFieldsInDirection(field, flankSize, Direction.West, isReversed);
        flankFields.AddRange(leftFlank);

        // Права сторона
        List<Field> rightFlank = GetFieldsInDirection(field, flankSize, Direction.East, isReversed);
        flankFields.AddRange(rightFlank);

        return flankFields;
    }

    public bool FieldExists(Field field) {
        return GetFieldAt(field.row, field.column) != null;
    }

    public Field GetFieldAt(int row, int column) {
        if (Fields == null
            || row < 0 || column < 0
            || row >= Fields.Count
            || column >= Fields[0].Count) {
            return null;
        }
        return Fields[row][column];
    }

    #endregion

    public bool IsFieldInEnemyZone(Field field) {
        return field.Owner is Enemy;
    }

    public Vector3 GetGridCenter() {
        float width = Fields.Count > 0 ? Fields.Count : 0f;
        float height = Fields.Count > 0 && Fields[0].Count > 0 ? Fields[0].Count : 0f;
        return new Vector3(width / 2f, 0, height / 2f);
    }

    public Vector2Int? GetGridIndexByWorld(Transform origin, Vector3 worldPosition) {
        if (_settings == null) return null;
        Vector3 localPosition = origin.InverseTransformPoint(worldPosition);
        int x = Mathf.FloorToInt(localPosition.x / _settings.cellSize.width);
        int y = Mathf.FloorToInt(localPosition.z / _settings.cellSize.height);
        return (x >= 0 && x < Fields.Count && y >= 0 && y < Fields[0].Count) ? new Vector2Int(x, y) : (Vector2Int?)null;
    }

    public GridUpdateData RemoveAll() {
        return BuildGrid(0, 0);
    }

    public GridSettings GetConfig() {
        return _settings;
    }
}
