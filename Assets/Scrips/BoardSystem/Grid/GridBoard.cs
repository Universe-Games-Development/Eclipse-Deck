using System;
using System.Collections.Generic;
using UnityEngine;

public class GridBoard {
    private CompasGrid[,] grids = new CompasGrid[2, 2];
    public GridSettings Config {
        get => _config;
        set {
            _config = value;
        }
    }

    private GridSettings _config;

    public GridBoard(GridSettings _config) {
        for (int meridian = 0; meridian < grids.GetLength(0); meridian++) {
            for (int zonal = 0; zonal < grids.GetLength(1); zonal++) {
                grids[meridian, zonal] = new CompasGrid(meridian, zonal);
            }
        }
    }

    public BoardUpdateData UpdateGlobalGrid(GridSettings _config) {
        if (_config == null || _config.northRows == null || _config.southRows == null || _config.eastColumns == null || _config.westColumns == null) {
            return null;
        }
        Config = _config;
        BoardUpdateData boardUpdateData = new();

        List<FieldType> northRows = Config.northRows;
        List<FieldType> southRows = Config.southRows;

        List<int> eastColumns = Config.eastColumns;
        List<int> westColumns = Config.westColumns;

        // Оновлюємо кожен DirectionalGrid
        for (int meridian = 0; meridian < grids.GetLength(0); meridian++) {
            for (int zonal = 0; zonal < grids.GetLength(1); zonal++) {
                // Передаємо відповідні ряди та колонки на основі меридіану та зонального індексу
                GridUpdateData gridUpdateData = grids[meridian, zonal].UpdateGrid(
                    rowTypes: meridian == 0 ? southRows : northRows,
                    columns: zonal == 0 ? westColumns : eastColumns
                );
                boardUpdateData.gridsUpdateData.Add(gridUpdateData);
            }
        }

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

    public CompasGrid GetColumnNeighbourGrid(CompasGrid directionalGrid) {
        int neighborZonal = directionalGrid.gridColumn == 1 ? 0 : 1;
        return (neighborZonal >= 0 && neighborZonal < grids.GetLength(1))
            ? grids[directionalGrid.gridRow, neighborZonal]
            : null;
    }

    internal BoardUpdateData RemoveAll() {
        BoardUpdateData boardUpdateData = new();
        foreach (CompasGrid grid in grids) {
            boardUpdateData.gridsUpdateData.Add(grid.CleanAll());
        }
        return boardUpdateData;
    }

    internal bool FieldExists(Field field) {
        if (field == null) return false;
        Field fieldInGrids = GetFieldAt(field.GetRow(), field.GetColumn());
        return field != null && field == fieldInGrids;
    }

    /* This method deprecated and no longer works since we changed from 1 grid to 4 in each compas direction
     * To DO :
     *  We have grids thats starts rows and columns starting from 1, 1; -1, 1; -1, -1; 1, -1; in global coordinates
     *  We have origin that represents center for all 4 grids
     *  We have world position by wich we need to determine index of field that in bounds of it
     * 
     */
    internal Vector2Int? GetGridIndexByWorld(Transform origin, Vector3 worldPosition) {
        if (Config == null) return null;

        Vector3 localPosition = origin.InverseTransformPoint(worldPosition);
        int x = Mathf.FloorToInt(localPosition.x / Config.cellSize.width);
        int y = Mathf.FloorToInt(localPosition.z / Config.cellSize.height);

        bool validRow = x >= 0 && x < Config.northRows.Count + Config.southRows.Count;
        bool validColumn = y >= 0 && y < Config.westColumns.Count + Config.eastColumns.Count;

        return (validRow && validColumn) ? new Vector2Int(x, y) : (Vector2Int?)null;
    }

    public Vector3 GetGridCenter() {
        if (grids.Length == 0) return Vector3.zero;

        int totalWHeight = 0; // Найбільша кількість рядків у одній сітці
        int totalWidth = 0; // Сумарна ширина всіх сіток

        foreach (CompasGrid compasGrid in grids) {
            totalWHeight += compasGrid._fields.Count;

            int compasGridWidth = 0;
            foreach (var row in compasGrid._fields) {
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
            foreach (var row in compasGrid._fields) {
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
            totalWHeight += compasGrid._fields.Count;
        }

        return totalWHeight;
    }
    /* Each field have coordinates in format (+3, -5)
     * we need to define it`s direction by values and compare to incoming globalDirection
     * 
     * Example : 
     * Field at - 1; +4 
     * Direction: North
     * 
     * -1 means it on South
     * +4 means east
     * 
     * We make offset like -1; 1 and form direction and check it belongs to globalDirection
     */
    public bool IsFieldBelogToDirection(Field currentField, Direction globalDirection) {
        int meridian = currentField.GetRow() > 0 ? 1 : -1;
        int zonal = currentField.GetColumn() > 0 ? 1 : -1;

        Direction fieldDirection = CompassUtil.GetDirectionFromOffset(meridian, zonal);
        return CompassUtil.BelongsToGlobalDirection(fieldDirection, globalDirection);
    }

    internal List<Field> GetFieldsInDirection(Field currentField, int moveAmount, Direction moveDirection, bool isRelativeToEnemy) {
        throw new NotImplementedException();
    }

    internal List<Field> GetAdjacentFields(Field currentField) {
        throw new NotImplementedException();
    }

    internal List<Field> GetFlankFields(Field currentField, int flankSize, bool isRelativeToEnemy) {
        throw new NotImplementedException();
    }

    public List<CompasGrid> GetGridsByGlobalDirection(Direction globalDirection) {
        List<CompasGrid> compasGrids = new();

        foreach (var compasGrid in grids) {
            if (CompassUtil.BelongsToGlobalDirection(compasGrid.gridDirection, globalDirection)) {
                compasGrids.Add(compasGrid);
            }
        }

        return compasGrids;
    }

    public Vector3 GetGridBalanceOffset() {
        if (grids.Length == 0) return Vector3.zero;

        // Перевірка на існування _fields для північних і південних сіток
        int northHeight = (grids[1, 0]._fields != null) ? grids[1, 0]._fields.Count : 0;
        int southHeight = (grids[0, 0]._fields != null) ? grids[0, 0]._fields.Count : 0;
        int heightDifference = northHeight - southHeight;

        // Перевірка на існування _fields для східних і західних сіток
        int eastWidth = (grids[0, 1]._fields != null && grids[0, 1]._fields.Count > 0) ? grids[0, 1]._fields[0].Count : 0;
        int westWidth = (grids[0, 0]._fields != null && grids[0, 0]._fields.Count > 0) ? grids[0, 0]._fields[0].Count : 0;
        int widthDifference = eastWidth - westWidth;

        return new Vector3(widthDifference, 0, heightDifference);
    }

}
