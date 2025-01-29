using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CompasGrid {
    public List<List<Field>> _fields = new();
    public int gridRow;
    public int gridColumn;
    public Direction gridDirection;
    public CompasGrid(int meridian, int zonal) {
        this.gridRow = meridian;
        this.gridColumn = zonal;
        _fields = new();
        gridDirection = GetGridDirection2x2Array(gridRow , gridColumn);
    }

    public GridUpdateData UpdateGrid(List<FieldType> rowTypes, List<int> columns) {
        GridUpdateData gridUpdateData = new(gridDirection);

        if (rowTypes == null || columns == null) {
            Debug.LogError("received wrong settings to update grid");
            return gridUpdateData;
        }

        for (int row = 0; row < _fields.Count || row < rowTypes.Count; row++) {
            if (row >= _fields.Count) {
                AddRow(rowTypes[row], row, columns, gridUpdateData);
                continue;
            }

            if (row >= rowTypes.Count) {
                RemoveRow(row, gridUpdateData);
                continue;
            }

            // Update exist row
            UpdateRow(row, rowTypes[row], columns, gridUpdateData);
        }
        TrimGrid(gridUpdateData);

        return gridUpdateData;
    }

    private void TrimGrid(GridUpdateData gridUpdateData) {
        TrimEmptyColumns(gridUpdateData);
        TrimEmptyRows(gridUpdateData);

        // Remove Fields from markedEmpty that are also in removedFields
        gridUpdateData.markedEmpty.RemoveAll(field => gridUpdateData.removedFields.Contains(field));
    }

    private void UpdateRow(int row, FieldType rowType, List<int> columns, GridUpdateData gridUpdateData) {
        List<Field> fieldRow = _fields[row];
        int maxColumns = Mathf.Max(columns.Count, fieldRow.Count);

        for (int col = 0; col < maxColumns; col++) {
            if (col >= fieldRow.Count) {
                // Add new column
                Field newField = new(CalculateGlobalCoordinates(row, col)) {
                    Type = rowType
                };
                _fields[row].Add(newField);
                gridUpdateData.addedFields.Add(newField);
            }

            // We dont check exceeded columns because it`s handled in exceeded rows

            // Mark empty if value of column == 0
            if (col >= columns.Count || columns[col] == 0) {
                fieldRow[col].Type = FieldType.Empty;
                gridUpdateData.markedEmpty.Add(fieldRow[col]);
            } else if (fieldRow[col].Type != rowType) {
                fieldRow[col].Type = rowType;
            }
        }
    }

    public void TrimEmptyRows(GridUpdateData gridUpdateData) {
        for (int row = _fields.Count - 1; row >= 0; row--) {
            if (IsRowEmpty(_fields[row])) {
                RemoveRow(row, gridUpdateData);
            } else {
                break;  // stop if next is not Empty
            }
        }
    }

    public void TrimEmptyColumns(GridUpdateData gridUpdateData) {
        if (_fields.Count == 0) return;
        
        int columnCount = _fields[0].Count;

        for (int col = columnCount - 1; col >= 0; col--) {
            if (IsColumnEmpty(col)) {
                gridUpdateData.removedFields.AddRange(RemoveColumn(col));
            } else {
                break; // «упин€Їмос€, €кщо колонка не порожн€
            }
        }
    }

    private void AddRow(FieldType rowType, int rowIndex, List<int> columns, GridUpdateData gridUpdateData) {
        List<Field> newRow = new();

        for (int col = 0; col < columns.Count; col++) {
            Field newField = new(CalculateGlobalCoordinates(rowIndex, col));

            FieldType type;
            if (columns[col] == 0) {
                newField.Type = FieldType.Empty;
                gridUpdateData.markedEmpty.Add(newField);
                
            } else {
                newField.Type = rowType;
                gridUpdateData.addedFields.Add(newField);
            }

            newRow.Add(newField);
        }

        _fields.Add(newRow);
    }

    private void RemoveRow(int rowIndex, GridUpdateData gridUpdateData) {
        foreach (var field in _fields[rowIndex]) {
            gridUpdateData.removedFields.Add(field);
        }
        _fields.RemoveAt(rowIndex);
    }

    private List<Field> MarkEmptyRow(int rowIndex) {
        List<Field> markedEmptyRow = new();

        foreach (var field in _fields[rowIndex]) {
            field.Type = FieldType.Empty;
            markedEmptyRow.Add(field);
        }

        return markedEmptyRow;
    }

    private List<Field> AddColumn(int totalRows, int columnIndex, List<FieldType> rowTypes) {
        List<Field> addedColumn = new();

        for (int row = 0; row < totalRows; row++) {
            Field newField = new(CalculateGlobalCoordinates(row, columnIndex)) {
                Type = rowTypes[row]
            };
            addedColumn.Add(newField);
            _fields[row].Add(newField);
        }
        return addedColumn;
    }

    private List<Field> RemoveColumn(int col) {
        List<Field> removedColumn = new();
        foreach (var row in _fields) {
            removedColumn.Add(row[col]);
            row.RemoveAt(col);
        }
        return removedColumn;
    }

    private List<Field> MarkEmptyColumn(int totalRows, int columnIndex) {
        List<Field> markedEmptyColumn = new();

        for (int row = 0; row < totalRows; row++) {
            if (columnIndex >= _fields[row].Count) continue;

            Field field = _fields[row][columnIndex];
            field.Type = FieldType.Empty;
            markedEmptyColumn.Add(field);
        }
        return markedEmptyColumn;
    }

    public Field GetField(int localRow, int localColumn) {
        if (localRow >= 0 && localRow < _fields.Count &&
            localColumn >= 0 && localColumn < _fields[localRow].Count) {
            return _fields[localRow][localColumn];
        }
        return null; // якщо координати виход€ть за меж≥ с≥тки
    }

    private (int row, int column) CalculateGlobalCoordinates(int localRow, int localColumn) {
        int globalRow = localRow * (gridRow == 0 ? -1 : 1) + (gridRow == 1 ? 1 : -1);
        int globalCol = localColumn * (gridColumn == 0 ? -1 : 1) + (gridColumn == 1 ? 1 : -1);
        return (globalRow, globalCol);
    }


    private bool IsRowEmpty(List<Field> row) {
        return row.All(f => f.Type == FieldType.Empty);
    }

    private bool IsColumnEmpty(int columnIndex) {
        return _fields.All(row => columnIndex < row.Count && row[columnIndex].Type == FieldType.Empty);
    }

    public GridUpdateData CleanAll() {
        throw new System.NotImplementedException();
    }

    private Direction GetGridDirection2x2Array(int row, int column) {
        int xOffset = row == 0 ? -1 : row;
        int yOffset = column == 0 ? -1 : column;
        return CompassUtil.GetDirectionFromOffset(xOffset, yOffset);
    }
}
