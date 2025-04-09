using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridBoard {
    public Action<BoardUpdateData> OnGridUpdated;

    private CompasGrid[,] grids = new CompasGrid[2, 2];
    public BoardSettingsData Config {
        get => _config;
        set {
            _config = value;
        }
    }

    private BoardSettingsData _config;

    public GridBoard() {
        for (int meridian = 0; meridian < grids.GetLength(0); meridian++) {
            for (int zonal = 0; zonal < grids.GetLength(1); zonal++) {
                grids[meridian, zonal] = new CompasGrid(this, meridian, zonal);
            }
        }
    }

    #region Update methods
    public BoardUpdateData UpdateGlobalGrid(BoardSettingsData newConfig) {
        // Використовуємо передану конфігурацію або поточну, якщо newConfig == null
        Config = newConfig ?? Config;

        if (Config == null) {
            Debug.LogError("BoardSettingsSO is null!");
            return null;
        }

        BoardUpdateData boardUpdateData = new();

        // Оновлюємо кожен CompasGrid
        for (int meridian = 0; meridian < grids.GetLength(0); meridian++) {
            for (int zonal = 0; zonal < grids.GetLength(1); zonal++) {
                CompasGrid updateGrid = grids[meridian, zonal];

                List<List<int>> gridUpdateValues = Config.GetGridValues(updateGrid.GridDirection);

                GridUpdateData gridUpdateData = updateGrid.UpdateGrid(gridUpdateValues);
                boardUpdateData.gridsUpdateData.Add(gridUpdateData);
            }
        }
        OnGridUpdated?.Invoke(boardUpdateData);
        return boardUpdateData;
    }


    public Field GetFieldAt(int globalRow, int globalColumn) {
        if (globalRow == 0 || globalColumn == 0) {
            return null;
        }

        // Визначаємо, до якої сітки звертатися
        int meridian = globalRow > 0 ? 1 : 0; // Північ чи Південь
        int zonal = globalColumn > 0 ? 1 : 0; // Схід чи Захід

        // Отримуємо локальні координати
        int localRow = Mathf.Abs(globalRow) - 1;
        int localColumn = Mathf.Abs(globalColumn) - 1;

        return grids[meridian, zonal].GetField(localRow, localColumn);
    }

    public CompasGrid GetRowNeighbourGrid(CompasGrid directionalGrid) {
        int neighborMeridian = directionalGrid.gridRow == 1 ? 0 : 1;
        return (neighborMeridian >= 0 && neighborMeridian < grids.GetLength(0))
            ? grids[neighborMeridian, directionalGrid.gridColumn]
            : null;
    }

    public BoardUpdateData RemoveAll() {
        BoardUpdateData boardUpdateData = new();
        foreach (CompasGrid grid in grids) {
            boardUpdateData.gridsUpdateData.Add(grid.CleanAll());
        }
        return boardUpdateData;
    }

    public void ProcessGrids(Action<Field> fieldAction) {
        foreach (var grid in GetAllGrids()) {
            foreach (var row in grid.Fields) {
                foreach (var field in row) {
                    fieldAction(field);
                }
            }
        }
    }
    #endregion

    #region Geometry
    public bool GetFieldByLocal(Vector3 local, out Field field) {
        field = null;
        if (!GetGridIndexByLocal(local, out Vector2Int indexes)) {
            Debug.LogWarning($"Can't get field by local: {local}");
            return false;
        }
        field = GetFieldAt(indexes.x, indexes.y);
        if (field == null) {
            Debug.LogWarning($"Can't find field at local: {local}");
            return false;
        }

        return true;
    }

    public bool GetGridIndexByLocal(Vector3 localPosition, out Vector2Int indexes) {
        indexes = new();
        if (Config == null) return false;

        float xCellOffset = Config.cellSize.width / 2;
        float yCellOffset = Config.cellSize.height / 2;

        //int gapOffsetX = Mathf.FloorToInt((localPosition.x == 0 ? 0 : Mathf.Sign(localPosition.x))); 
        //int gapOffsetY = Mathf.FloorToInt((localPosition.z == 0 ? 0 : Mathf.Sign(localPosition.z)));

        int row = Mathf.FloorToInt((localPosition.z + yCellOffset) / Config.cellSize.height);
        int column = Mathf.FloorToInt((localPosition.x + xCellOffset) / Config.cellSize.width);

        bool validRow = Mathf.Abs(row) < Config.northRows + Config.southRows;
        bool validColumn = Mathf.Abs(column) < Config.eastColumns + Config.westColumns;

        if (validRow && validColumn) {
            indexes = new Vector2Int(row, column);
            return true;
        }
        return false;
    }
    public Vector3 GetGridBalanceOffset() {
        if (grids.Length == 0) return Vector3.zero;

        // Перевірка на існування _fields для північних і південних сіток
        int northHeight = (grids[1, 0].Fields != null) ? grids[1, 0].Fields.Count : 0;
        int southHeight = (grids[0, 0].Fields != null) ? grids[0, 0].Fields.Count : 0;
        int heightDifference = northHeight - southHeight;

        // Перевірка на існування _fields для східних і західних сіток
        int eastWidth = (grids[0, 1].Fields != null && grids[0, 1].Fields.Count > 0) ? grids[0, 1].Fields[0].Count : 0;
        int westWidth = (grids[0, 0].Fields != null && grids[0, 0].Fields.Count > 0) ? grids[0, 0].Fields[0].Count : 0;
        int widthDifference = eastWidth - westWidth;

        return new Vector3(widthDifference, 0, heightDifference);
    }

    public Vector3 GetGridCenter() {
        if (grids.Length == 0) return Vector3.zero;

        int totalWHeight = 0; // Найбільша кількість рядків у одній сітці
        int totalWidth = 0; // Сумарна ширина всіх сіток

        foreach (CompasGrid compasGrid in grids) {
            totalWHeight += compasGrid.Fields.Count;

            int compasGridWidth = 0;
            foreach (var row in compasGrid.Fields) {
                compasGridWidth = Mathf.Max(compasGridWidth, row.Count);
            }
            totalWidth += compasGridWidth;
        }

        return new Vector3(totalWidth / 2f, 0, totalWHeight / 2f);
    }

    private int GetTotatWidth() {
        if (grids.Length == 0) return 0;
        int totalWidth = 0; // Сумарна ширина всіх сіток

        foreach (CompasGrid compasGrid in grids) {
            int compasGridWidth = 0;
            foreach (var row in compasGrid.Fields) {
                compasGridWidth = Mathf.Max(compasGridWidth, row.Count);
            }
            totalWidth += compasGridWidth;
        }

        return totalWidth;
    }

    private int GetTotatlHeight() {
        if (grids.Length == 0) return 0;

        int totalWHeight = 0; // Найбільша кількість рядків у одній сітці

        foreach (CompasGrid compasGrid in grids) {
            totalWHeight += compasGrid.Fields.Count;
        }

        return totalWHeight;
    }
    #endregion

    #region Field Getters
    public List<Field> GetFieldsInDirection(Field currentField, int searchDistance, Direction searchDirection) {
        (int rowOffset, int colOffset) offset = CompassUtil.DirectionOffsets.GetValueOrDefault(searchDirection);
        List<Field> fields = new List<Field>();

        for (int i = 1; i <= searchDistance; i++) {
            int row = currentField.GetRow() + offset.rowOffset * i;
            int column = currentField.GetColumn() + offset.colOffset * i;

            // Пропускаємо координату (0,0)
            if (row == 0) row += Math.Sign(offset.rowOffset);
            if (column == 0) column += Math.Sign(offset.colOffset);

            Field foundField = GetFieldAt(row, column);
            if (foundField != null) {
                fields.Add(foundField);
            }
        }
        return fields;
    }


    public List<Field> GetAdjacentFields(Field currentField) {
        List<Field> adjacentFields = new();
        List<(int rowOffset, int colOffset)> offsets = CompassUtil.GetOffsets();

        foreach (var (rowOffset, colOffset) in offsets) {
            int newRow = currentField.GetRow() + rowOffset;
            int newCol = currentField.GetColumn() + colOffset;
            // Ïåðåâ³ðêà, ÷è çíàõîäèòüñÿ íîâå ïîëå â ìåæàõ îñíîâíî¿ ñ³òêè
            Field field = GetFieldAt(newRow, newCol);
            if (field != null) {
                adjacentFields.Add(field);
            }
        }
        Debug.Log($"Found {adjacentFields.Count} adjacent fields");
        return adjacentFields;
    }

    public List<Field> GetFlankFields(Field currentField, int flankSize) {
        List<Field> flankFields = new();
        flankFields.AddRange(GetFieldsInDirection(currentField, flankSize, Direction.East));
        flankFields.AddRange(GetFieldsInDirection(currentField, flankSize, Direction.West));
        return flankFields;
    }

    public List<Field> GetAttackFieldsByGlobalDirection(Direction globalDirection) {
        return GetGridsByGlobalDirection(globalDirection).SelectMany(grid => grid.GetAttackFields()).ToList();
    }
    #endregion

    #region Field Validators
    public bool FieldExists(Field field) {
        if (field == null) return false;
        Field fieldInGrids = GetFieldAt(field.GetRow(), field.GetColumn());
        return field != null && field == fieldInGrids;
    }

    public bool IsFieldBelogToDirection(Field currentField, Direction globalDirection) {
        Direction fieldDirection = GetFieldCompasDirection(currentField);
        return CompassUtil.BelongsToGlobalDirection(fieldDirection, globalDirection);
    }

    public Direction GetFieldCompasDirection(Field currentField) {
        int meridian = currentField.GetRow() > 0 ? 1 : -1;
        int zonal = currentField.GetColumn() > 0 ? 1 : -1;

        return CompassUtil.GetDirectionFromOffset(meridian, zonal);
    }
    #endregion

    #region Grid Getters
    public CompasGrid GetColumnNeighbourGrid(CompasGrid directionalGrid) {
        int neighborZonal = directionalGrid.gridColumn == 1 ? 0 : 1;
        return (neighborZonal >= 0 && neighborZonal < grids.GetLength(1))
            ? grids[directionalGrid.gridRow, neighborZonal]
            : null;
    }

    public List<CompasGrid> GetGridsByGlobalDirection(Direction globalDirection) {
        List<CompasGrid> compasGrids = new();

        foreach (var compasGrid in grids) {
            if (CompassUtil.BelongsToGlobalDirection(compasGrid.GridDirection, globalDirection)) {
                compasGrids.Add(compasGrid);
            }
        }

        return compasGrids;
    }

    public List<CompasGrid> GetAllGrids() {
        List<CompasGrid> compasGrids = new();

        foreach (var compasGrid in grids) {
            compasGrids.Add(compasGrid);
        }

        return compasGrids;
    }

    
    #endregion
}
