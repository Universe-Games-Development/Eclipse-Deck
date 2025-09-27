using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GridBoardSettings", menuName = "PrefabSettings/BoardSettings")]
public class BoardSettingsData : ScriptableObject {
    private const int MIN_ROWS = 1;
    private const int MIN_COLUMNS = 1;

    private const int DEFAULT_ROWS = 2;
    private const int DEFAULT_COLUMNS = 4;

    public int northRows = 3;
    public int southRows = 3;
    public int eastColumns = 3;
    public int westColumns = 3;

    public CellSize cellSize = new CellSize { width = 1, height = 1 };

    [SerializeField] private List<GridData> directionGrids = new();

    private void OnEnable() {
        if (!IsInitialized()) {
            ResetGrids();
        }
    }

    public void ResetGrids() {
        westColumns = DEFAULT_COLUMNS;
        eastColumns = DEFAULT_COLUMNS;
        northRows = DEFAULT_ROWS;
        southRows = DEFAULT_ROWS;

        directionGrids.Clear();

        Direction[] allowedDirections = { Direction.SouthEast, Direction.SouthWest, Direction.NorthEast, Direction.NorthWest };

        foreach (Direction dir in allowedDirections) {
            int meridianRows = CompassUtil.BelongsToGlobalDirection(dir, Direction.North) ? northRows : southRows;
            meridianRows = Mathf.Max(meridianRows, MIN_ROWS);

            int zonalColumns = CompassUtil.BelongsToGlobalDirection(dir, Direction.North) ? eastColumns : westColumns;

            var grid = new List<GridRow>();
            for (int i = 0; i < meridianRows; i++) {
                grid.Add(new GridRow { columnValues = new List<int>(Enumerable.Repeat(0, zonalColumns)) });
            }

            directionGrids.Add(new GridData { direction = dir, grid = grid });
        }
    }

    private void OnValidate() {
        ValidateGrids();
    }

    private void ValidateGrids() {
        if (directionGrids == null) {
            ResetGrids();
            return;
        }

        foreach (var grid in directionGrids) {
            if (grid.grid == null) {
                ResetGrids();
                break;
            }
        }

        bool needResize = false;

        foreach (var gridData in directionGrids) {
            var grid = gridData.grid;

            int meridianRows = CompassUtil.BelongsToGlobalDirection(gridData.direction, Direction.North) ? northRows : southRows;
            meridianRows = Mathf.Max(meridianRows, MIN_ROWS);

            int zonalColumns = CompassUtil.BelongsToGlobalDirection(gridData.direction, Direction.North) ? eastColumns : westColumns;
            zonalColumns = Mathf.Max(zonalColumns, MIN_COLUMNS);

            if (grid.Count < meridianRows || grid[0].columnValues.Count < zonalColumns) {
                needResize = true;
            }

            if (grid.Count == 0 || grid[0].columnValues.Count < MIN_COLUMNS) {
                needResize = true;
            }

            if (needResize) {
                ResizeAllGrids();
                CorrectGridData();
                break;
            }
        }
    }

    private void ResizeAllGrids() {
        if (directionGrids != null)
            foreach (var gridData in directionGrids) {
                int targetRows = CompassUtil.BelongsToGlobalDirection(gridData.direction, Direction.North) ? northRows : southRows;
                int targetColumns = CompassUtil.BelongsToGlobalDirection(gridData.direction, Direction.East) ? eastColumns : westColumns;
                ResizeGrid(gridData, targetRows, targetColumns);
            }
    }

    private void ResizeGrid(GridData gridData, int targetRows, int targetColumns) {
        List<GridRow> grid = gridData.grid;

        while (grid.Count < targetRows) {
            List<int> row = new List<int>(Enumerable.Repeat(0, targetColumns));

            grid.Add(new GridRow { columnValues = row });
        }
        while (grid.Count > targetRows) {
            grid.RemoveAt(grid.Count - 1);
        }

        foreach (var row in grid) {
            while (row.columnValues.Count < targetColumns) {
                row.columnValues.Add(0);
            }
            while (row.columnValues.Count > targetColumns) {
                row.columnValues.RemoveAt(row.columnValues.Count - 1);
            }
        }
    }

    #region Editing
    public void ModifyRow(Direction direction, bool isAdding) {
        if (direction != Direction.North && direction != Direction.South) {
            Debug.Log("Wrong direction to modify rows");
            return;
        }

        ref int targetRows = ref (CompassUtil.BelongsToGlobalDirection(direction, Direction.North)
         ? ref northRows
         : ref southRows);

        if (isAdding) {
            targetRows++;
        } else {
            if (targetRows <= MIN_ROWS) {
                Debug.Log($"Wrong rows amount current{targetRows} Minimum: {MIN_ROWS}. Can`t remove more!");
                return;
            }
            targetRows--;
        }

        List<GridData> targetGrids = GetGridsByGlobalDirection(direction);

        foreach (var gridData in targetGrids) {
            int targetColumns = CompassUtil.BelongsToGlobalDirection(direction, Direction.North) ? eastColumns : westColumns;

            if (isAdding) {
                gridData.grid.Add(new GridRow { columnValues = new List<int>(Enumerable.Repeat(0, targetColumns)) });
            } else {
                gridData.grid.RemoveAt(gridData.grid.Count - 1);
            }
        }
        CorrectGridData();
    }

    public void AddRow(Direction direction) {
        ModifyRow(direction, true);
    }

    public void RemoveRow(Direction direction) {
        ModifyRow(direction, false);
    }

    public void ModifyColumn(Direction direction, bool isAdding) {
        if (direction != Direction.East && direction != Direction.West) {
            Debug.Log($"Wrong direction to modify columns {direction}");
            return;
        }

        ref int targetColumns = ref (CompassUtil.BelongsToGlobalDirection(direction, Direction.East)
         ? ref eastColumns
         : ref westColumns);

        if (isAdding) {
            targetColumns++;
        } else {
            if (targetColumns <= MIN_COLUMNS) {
                Debug.Log($"Wrong columns amount current {targetColumns}, Minimum: {MIN_COLUMNS}. Can't remove more!");
                return;
            }
            targetColumns--;
        }

        var targetGridDatas = GetGridsByGlobalDirection(direction);
        foreach (var gridData in targetGridDatas) {
            ResizeGrid(gridData, gridData.grid.Count, targetColumns);
        }
        CorrectGridData();
    }

    public void AddColumn(Direction direction) {
        ModifyColumn(direction, true);
    }

    public void RemoveColumn(Direction direction) {
        ModifyColumn(direction, false);
    }

    public void SetAllGrids(int value) {
        foreach (var gridData in directionGrids) {
            SetGrid(gridData.grid, value, false);
        }
        CorrectGridData();
    }

    public void SetGrid(List<GridRow> grid, int value, bool doCorrectData = true) {
        foreach (var row in grid) {
            for (int j = 0; j < row.columnValues.Count; j++) {
                row.columnValues[j] = value;
            }
        }
        if (doCorrectData)
            CorrectGridData();
    }

    public void SetGridByDirection(Direction direction, int value) {
        var gridData = directionGrids.FirstOrDefault(g => g.direction == direction);
        if (gridData.grid != null) {
            SetGrid(gridData.grid, value);
        }
    }

    public void RandomizeAllGrids() {
        foreach (var gridData in directionGrids) {
            RandomizeGrid(gridData.grid, false);
        }
        CorrectGridData();
    }

    public void RandomizeGrid(Direction dir) {
        var gridData = directionGrids.FirstOrDefault(g => g.direction == dir);
        if (gridData.grid != null) {
            RandomizeGrid(gridData.grid);
        }
    }

    public void RandomizeGrid(List<GridRow> grid, bool doCorrectData = true) {
        foreach (var row in grid) {
            for (int j = 0; j < row.columnValues.Count; j++) {
                row.columnValues[j] = Random.Range(0, 2);
            }
        }
        if (doCorrectData)
            CorrectGridData();
    }
    #endregion

    private List<GridData> GetGridsByGlobalDirection(Direction globalDirection) {
        return directionGrids
            .Where(gridData => CompassUtil.BelongsToGlobalDirection(gridData.direction, globalDirection))
            .ToList();
    }

    public List<List<int>> GetGridValues(Direction dir) {
        GridData gridData = directionGrids.FirstOrDefault(g => g.direction == dir);
        List<List<int>> values = new List<List<int>>();
        foreach (var row in gridData.grid) {
            values.Add(row.columnValues);
        }
        return values;
    }

    public void CorrectGridData() {
        RestoreNecessaryFields();
        var eastNeighbors = GetGridsByGlobalDirection(Direction.East);
        var westNeighbors = GetGridsByGlobalDirection(Direction.West);

        GenerateNeccessaryAttackFields(eastNeighbors);
        GenerateNeccessaryAttackFields(westNeighbors);
    }

    private void GenerateNeccessaryAttackFields(List<GridData> meridianNeighbors) {
        var firstGridData = meridianNeighbors[0];
        var secondGridData = meridianNeighbors[1];

        var firstRowFirstGrid = firstGridData.grid[0].columnValues;
        var firstRowSecondGrid = secondGridData.grid[0].columnValues;

        for (int col = 0; col < firstRowFirstGrid.Count; col++) {
            if (firstRowFirstGrid[col] == 1 && firstRowSecondGrid[col] != 1) {
                firstRowSecondGrid[col] = 1;
            }

            if (firstRowSecondGrid[col] == 1 && firstRowFirstGrid[col] != 1) {
                firstRowFirstGrid[col] = 1;
            }
        }
    }

    private void RestoreNecessaryFields() {
        foreach (var gridData in directionGrids) {
            var grid = gridData.grid;

            if (grid == null || grid.Count == 0) {
                return;
            }

            for (int col = grid[0].columnValues.Count - 1; col >= 0; col--) {
                int lastNonEmptyField = -1;

                for (int rowHeight = grid.Count - 1; rowHeight >= 0; rowHeight--) {
                    if (grid[rowHeight].columnValues[col] != 0) {
                        lastNonEmptyField = rowHeight;
                    }

                    if (grid[rowHeight].columnValues[col] == 0 && lastNonEmptyField != -1) {
                        grid[rowHeight].columnValues[col] = 1;
                    }
                }
            }
        }
    }

    public List<GridData> GetGrids() {
        return directionGrids;
    }

    public bool IsInitialized() {
        bool isNotReady = directionGrids == null || directionGrids.Count == 0;
        return !isNotReady;
    }

    public void ResetSize() {
        westColumns = DEFAULT_COLUMNS;
        eastColumns = DEFAULT_COLUMNS;
        northRows = DEFAULT_ROWS;
        southRows = DEFAULT_ROWS;
        ResizeAllGrids();
    }
}


[System.Serializable]
public class GridRow {
    public List<int> columnValues;
}

[System.Serializable]
public struct GridData {
    public Direction direction;
    public List<GridRow> grid;
}