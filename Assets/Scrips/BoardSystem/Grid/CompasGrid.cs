using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CompasGrid {
    public List<List<Field>> Fields { get; private set; }
    public int gridRow;
    public int gridColumn;
    public Direction GridDirection { get; }
    private GridBoard _board;
    public CompasGrid(GridBoard gridBoard, int row, int col) {
        Fields = new();
        gridRow = row;
        gridColumn = col;
        _board = gridBoard;
        GridDirection = GetGridDirection2x2Array(gridRow , gridColumn);
    }

    // Add or remove missing rows
    public GridUpdateData UpdateGrid(List<List<int>> fieldValues) {
        GridUpdateData gridUpdateData = new GridUpdateData(GridDirection);

        int targetRows = fieldValues.Count; // Use fieldValues count as the target
        int currentRowCount = Fields.Count;

        // Adjust rows
        if (currentRowCount < targetRows) {
            for (int row = currentRowCount; row < targetRows; row++) {
                AddNewRow(row, GetTypeByRow(row), fieldValues[row], gridUpdateData);
            }
        } else if (currentRowCount > targetRows) {
            for (int row = currentRowCount - 1; row >= targetRows; row--) {
                RemoveRow(row, gridUpdateData);
            }
        }

        // Update existing rows (and columns within those rows)
        for (int row = 0; row < targetRows; row++) {
            UpdateRow(row, GetTypeByRow(row), fieldValues[row], gridUpdateData);
        }

        TrimGrid(gridUpdateData);
        RestoreNecessaryFields(gridUpdateData);

        return gridUpdateData;
    }

    private FieldType GetTypeByRow(int row) {
        if (row == 0) {
            return FieldType.Attack;
        } else {
            return FieldType.Support;
        }
    }

    private void AddNewRow(int rowIndex, FieldType rowType, List<int> columns, GridUpdateData gridUpdateData) {
        List<Field> newRow = new List<Field>();
        for (int col = 0; col < columns.Count; col++) {
            Field newField = CreateField(rowIndex, col, rowType, columns[col], gridUpdateData);
            newRow.Add(newField);
            if (newField.FieldType != FieldType.Empty) {
                gridUpdateData.addedFields.Add(newField);
            }
        }
        Fields.Add(newRow);
    }

    private void UpdateRow(int rowIndex, FieldType rowType, List<int> columns, GridUpdateData gridUpdateData) {
        List<Field> fieldRow = Fields[rowIndex];
        int targetColumns = columns.Count;
        int currentColumnCount = fieldRow.Count;

        // Adjust columns within the row
        if (currentColumnCount < targetColumns) {
            for (int col = currentColumnCount; col < targetColumns; col++) {
                Field newField = CreateField(rowIndex, col, rowType, columns[col], gridUpdateData);
                fieldRow.Add(newField);
            }
        } else if (currentColumnCount > targetColumns) {
            for (int col = currentColumnCount - 1; col >= targetColumns; col--) {
                RemoveField(rowIndex, col, gridUpdateData);
            }
        }

        // Update existing fields in the row
        for (int col = 0; col < targetColumns; col++) {
            Field field = fieldRow[col];
            if (columns[col] == 0) {
                SetFieldType(field, FieldType.Empty, gridUpdateData);
            } else if (columns[col] != 0) {
                SetFieldType(field, rowType, gridUpdateData);
            }
        }
    }

    private Field CreateField(int row, int col, FieldType rowType, int value, GridUpdateData gridUpdateData) {
        Field newField = new Field(CalculateGlobalCoordinates(row, col));
        if (value == 0) {
            SetFieldType(newField, FieldType.Empty, gridUpdateData);
        } else {
            SetFieldType(newField, rowType, gridUpdateData);
        }
        return newField;
    }

    private void SetFieldType(Field field, FieldType newType, GridUpdateData gridUpdateData) {
        FieldType oldFieldType = field.FieldType;

        if (oldFieldType != newType) {
            

            // if old was empty and new not we add this to update visual board
            if (oldFieldType == FieldType.Empty) {
                gridUpdateData.addedFields.Add(field);
            }

            // if new type is empty and old was not we need to add it for update
            if (newType == FieldType.Empty) {
                gridUpdateData.markedEmpty.Add(field);
            }

            field.FieldType = newType;
        }
    }


    private void RemoveRow(int rowIndex, GridUpdateData gridUpdateData) {
        foreach (var field in Fields[rowIndex]) {
            gridUpdateData.removedFields.Add(field);
        }
        Fields.RemoveAt(rowIndex);
    }

    private void RemoveField(int rowIndex, int colIndex, GridUpdateData gridUpdateData) {
        gridUpdateData.removedFields.Add(Fields[rowIndex][colIndex]);
        Fields[rowIndex].RemoveAt(colIndex);
    }


    /* How it works?
     * We need to start iteration through each column
     * If current FieldType is Empty and neigbout on +1 column index is presend
     * 
     * I don`t know how to do it in optimized way :D
     */
    private void RestoreNecessaryFields(GridUpdateData updateData) {
        // Перебираємо кожен стовпець
        if (Fields == null || Fields.Count == 0) {
            //Debug.Log("Current board empty can`t restore fields");
            return;
        }
        for (int col = Fields[0].Count - 1; col >= 0; col--) {
            // Починаємо з останнього ряду і рухаємося до першого
            Field lastNonEmptyField = null; // Змінна для збереження останнього не порожнього поля в колонці

            for (int row = Fields.Count - 1; row >= 0; row--) {
                Field field = Fields[row][col];

                // Якщо поле не порожнє, зберігаємо його
                if (field.FieldType != FieldType.Empty) {
                    lastNonEmptyField = field;
                }

                // Якщо поле порожнє, перевіряємо наявність не порожнього сусіда
                if (field.FieldType == FieldType.Empty && lastNonEmptyField != null) {
                    RestoreEmpty(updateData, row, field);
                }
            }
        }
    }

    private void RestoreEmpty(GridUpdateData updateData, int row, Field field) {
        // Відновлюємо поле
        if (row == 0) {
            field.FieldType = FieldType.Attack;
        } else {
            field.FieldType = FieldType.Support;
        }

        // Видаляємо поле з markedEmpty, якщо воно є
        bool removeResult = updateData.markedEmpty.Remove(field);
        if (!removeResult) {
            Debug.Log("Cannot find field to remove from update data");
        }

        // Додаємо поле до списку доданих полів
        updateData.addedFields.Add(field);
    }

    #region Trimming
    private void TrimGrid(GridUpdateData gridUpdateData) {
        TrimEmptyColumns(gridUpdateData);
        TrimEmptyRows(gridUpdateData);

        // Remove Fields from markedEmpty that are also in removedFields
        gridUpdateData.markedEmpty.RemoveAll(field => gridUpdateData.removedFields.Contains(field));
    }
    public void TrimEmptyRows(GridUpdateData gridUpdateData) {

        for (int row = Fields.Count - 1; row >= 0; row--) {
            if (IsRowEmpty(Fields[row])) {
                RemoveRow(row, gridUpdateData);
            } else {
                break;  // stop if next is not Empty
            }
        }
    }

    public void TrimEmptyColumns(GridUpdateData gridUpdateData) {
        if (Fields.Count == 0) return;
        CompasGrid neigbour = _board.GetColumnNeighbourGrid(this);

        int columnCount = Fields[0].Count;

        for (int col = columnCount - 1; col >= 0; col--) {
            bool currentColumnEmpty = IsColumnTypeEmpty(col);
            if (neigbour == null && currentColumnEmpty) {
                gridUpdateData.removedFields.AddRange(RemoveColumn(col));
            } else if (neigbour != null && currentColumnEmpty && neigbour.HasEmptyColumnAt(col)) {
                gridUpdateData.removedFields.AddRange(RemoveColumn(col));
            } else {
                break; // Зупиняємося, якщо колонка не порожня
            }
        }
    }

    private bool HasEmptyColumnAt(int col) {
        bool result = false;

        // We return false because it means start of the initializing we can`t define is it empty because it need time on initialization to define it

        // if fields not initialized
        if (Fields == null) {
            return result;
        }

        // if first row doesn`t exist
        if (Fields.Count == 0) {
            return result;
        }

        // if column is out of bounds it means removed already from grid == Empty
        if (col >= Fields[0].Count) {
            result = true;
            return result;
        }
        // if column have all fields with EmptyType
        return IsColumnTypeEmpty(col);
    }
    #endregion

    public Field GetField(int localRow, int localColumn) {
        if (localRow >= 0 && localRow < Fields.Count &&
            localColumn >= 0 && localColumn < Fields[localRow].Count) {
            return Fields[localRow][localColumn];
        }
        return null; // Якщо координати виходять за межі сітки
    }

    public List<Field> GetAttackFields() {
        return Fields[0];
    }

    public GridUpdateData CleanAll() {
        GridUpdateData gridUpdateData = new(GridDirection);
        foreach (var row in Fields) {
            gridUpdateData.removedFields.AddRange(row); // Use AddRange for efficiency
        }
        Fields.Clear();
        return gridUpdateData;
    }

    public int GetRowsCount() {
        return Fields.Count;
    }
   

    private List<Field> MarkEmptyRow(int rowIndex) {
        List<Field> markedEmptyRow = new();

        foreach (var field in Fields[rowIndex]) {
            field.FieldType = FieldType.Empty;
            markedEmptyRow.Add(field);
        }

        return markedEmptyRow;
    }

    private List<Field> AddColumn(int totalRows, int columnIndex, List<FieldType> rowTypes) {
        List<Field> addedColumn = new();

        for (int row = 0; row < totalRows; row++) {
            Field newField = new(CalculateGlobalCoordinates(row, columnIndex)) {
                FieldType = rowTypes[row]
            };
            addedColumn.Add(newField);
            Fields[row].Add(newField);
        }
        return addedColumn;
    }

    private List<Field> RemoveColumn(int col) {
        List<Field> removedColumn = new();
        foreach (var row in Fields) {
            removedColumn.Add(row[col]);
            row.RemoveAt(col);
        }
        return removedColumn;
    }

    private List<Field> MarkEmptyColumn(int totalRows, int columnIndex) {
        List<Field> markedEmptyColumn = new();

        for (int row = 0; row < totalRows; row++) {
            if (columnIndex >= Fields[row].Count) continue;

            Field field = Fields[row][columnIndex];
            field.FieldType = FieldType.Empty;
            markedEmptyColumn.Add(field);
        }
        return markedEmptyColumn;
    }

    //Transforms local field index to global field indexes
    private (int row, int column) CalculateGlobalCoordinates(int localRow, int localColumn) {
        int globalRow = localRow * (gridRow == 0 ? -1 : 1) + (gridRow == 1 ? 1 : -1);
        int globalCol = localColumn * (gridColumn == 0 ? -1 : 1) + (gridColumn == 1 ? 1 : -1);
        return (globalRow, globalCol);
    }

    private bool IsRowEmpty(List<Field> row) {
        return row.All(f => f.FieldType == FieldType.Empty);
    }

    private bool IsColumnTypeEmpty(int columnIndex) {
        return Fields.All(row => columnIndex < row.Count && row[columnIndex].FieldType == FieldType.Empty);
    }

    private Direction GetGridDirection2x2Array(int row, int column) {
        int xOffset = row == 0 ? -1 : row;
        int yOffset = column == 0 ? -1 : column;
        return CompassUtil.GetDirectionFromOffset(xOffset, yOffset);
    }
}
