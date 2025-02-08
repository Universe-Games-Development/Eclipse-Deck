using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GlobalGridData {
    public GridData[,] gridDatas;

    public GlobalGridData(int northRows, int southRows, int columns) {
        InitializeGrids(northRows, southRows, columns);
    }

    public void InitializeGrids(int northRows, int southRows, int columns) {
        if (gridDatas != null) {
            return;
        }
        gridDatas = new GridData[2, 2];

        gridDatas[0, 0] = new GridData(northRows, columns, Direction.NorthWest);
        gridDatas[0, 1] = new GridData(northRows, columns, Direction.NorthEast);
        gridDatas[1, 0] = new GridData(southRows, columns, Direction.SouthWest);
        gridDatas[1, 1] = new GridData(southRows, columns, Direction.SouthEast);
    }


    public void ResizeGrids(int northRows, int southRows, int columns) {
        foreach (var grid in gridDatas) {
            grid.ResizeGrid(CompassUtil.BelongsToGlobalDirection(grid.GridDirection, Direction.North) ? northRows : southRows, columns);
        }
    }

    public GridData GetGridData(Direction dir) {
        foreach (var data in gridDatas) {
            if (data.GridDirection == dir) return data;
        }
        return null;
    }

    public List<List<int>> GetGridDataList(Direction dir) {
        CorrectGridData();
        return GetGridData(dir)?.grid;
    }

    public void RandomizeAllGrids() {
        foreach (var data in gridDatas) {
            data.RandomizeGrid();
        }

        CorrectGridData();
    }

    public void RandomizeGrid(Direction dir) {
        GetGridData(dir)?.RandomizeGrid();
        CorrectGridData();
    }

    internal void SetAllGrids(int value) {
        foreach (var gridData in gridDatas) {
            gridData.SetAllValues(value);
        }

        CorrectGridData();
    }

    internal void ResetGrids(int DEFAULT_ROWS, int DEFAULT_COLUMNS) {
        foreach (var gridData in gridDatas) {
            gridData.ResizeGrid(DEFAULT_ROWS, DEFAULT_COLUMNS);
        }

        CorrectGridData();
    }

    public List<GridData> GetGridsByGlobalDirection(Direction globalDirection) {
        List<GridData> globalGridDatas = new();

        foreach (var gridData in gridDatas) {
            if (CompassUtil.BelongsToGlobalDirection(gridData.GridDirection, globalDirection)) {
                globalGridDatas.Add(gridData);
            }
        }

        return globalGridDatas;
    }

    public void CorrectGridData() {
        RestoreNecessaryFields();
        GenerateNeccessaryAttackFields();
    }

    private void RestoreNecessaryFields() {
        foreach (var gridData in gridDatas) {
            List<List<int>> values = gridData.grid;
            // Перебираємо кожен стовпець
            if (values == null || values.Count == 0) {
                //Debug.Log("Current board empty can`t restore fields");
                return;
            }
            for (int col = values[0].Count - 1; col >= 0; col--) {
                // Починаємо з останнього ряду і рухаємося до першого
                int lastNonEmptyField = -1; // Змінна для збереження останнього не порожнього поля в колонці

                for (int rowHeight = values.Count - 1; rowHeight >= 0; rowHeight--) {

                    // Якщо поле не порожнє, зберігаємо його
                    if (values[rowHeight][col] != 0) {
                        lastNonEmptyField = rowHeight;
                    }

                    // Якщо поле порожнє, перевіряємо наявність не порожнього сусіда
                    if (values[rowHeight][col] == 0 && lastNonEmptyField != -1) {
                        values[rowHeight][col] = 1;
                    }
                }
            }
        }
    }

    private void GenerateNeccessaryAttackFields() {
        int columns = gridDatas[0, 0].grid[0].Count;
        foreach (var grid in gridDatas) {
            if (grid.grid[0].Count != columns) {
                Debug.LogError("Grids have inconsistent column counts!");
                return;
            }
        }

        for (int col = 0; col < columns; col++) {
            // Перевіряємо перший рядок для кожної пари (північ-південь)
            for (int pairIndex = 0; pairIndex < 2; pairIndex++) // 0 для NW/SW, 1 для NE/SE
            {
                GridData topGrid = gridDatas[0, pairIndex];
                GridData bottomGrid = gridDatas[1, pairIndex];

                if (topGrid.grid[0][col] == 1 && bottomGrid.grid[0][col] == 0) {
                    bottomGrid.grid[0][col] = 1;
                } else if (topGrid.grid[0][col] == 0) {
                    bool foundOne = false;
                    for (int row = 0; row < bottomGrid.grid.Count; row++) {
                        if (bottomGrid.grid[row][col] == 1) {
                            foundOne = true;
                            break;
                        }
                    }
                    if (foundOne) {
                        topGrid.grid[0][col] = 1;
                    }
                }

                if (bottomGrid.grid[0][col] == 1 && topGrid.grid[0][col] == 0) {
                    topGrid.grid[0][col] = 1;
                } else if (bottomGrid.grid[0][col] == 0) {
                    bool foundOne = false;
                    for (int row = 0; row < topGrid.grid.Count; row++) {
                        if (topGrid.grid[row][col] == 1) {
                            foundOne = true;
                            break;
                        }
                    }
                    if (foundOne) {
                        bottomGrid.grid[0][col] = 1;
                    }
                }
            }
        }
    }
}
