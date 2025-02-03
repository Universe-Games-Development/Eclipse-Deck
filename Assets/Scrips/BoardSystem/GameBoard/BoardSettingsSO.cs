using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridBoardSettings", menuName = "GridSettings")]
public class BoardSettingsSO : ScriptableObject {
    private const int MIN_ROWS = 1;
    private const int MIN_COLUMNS = 1;
    private const int DEFAULT_ROWS = 2;
    private const int DEFAULT_COLUMNS = 4;

    public int northRows = 3;
    public int southRows = 3;
    public int columns = 4;

    public CellSize cellSize = new CellSize { width = 1, height = 1 };

    public GlobalGridData globalGridData;

    private void OnValidate() {
        ValidateSettings();
    }

    private void ValidateSettings() {
        northRows = Mathf.Max(MIN_ROWS, northRows);
        southRows = Mathf.Max(MIN_ROWS, southRows);
        columns = Mathf.Max(MIN_COLUMNS, columns);
        if (globalGridData == null) {
            globalGridData = new GlobalGridData(northRows, southRows, columns);
        }
        if (globalGridData.gridDatas == null) {
            globalGridData.InitializeGrids(northRows, southRows, columns);
        }
        globalGridData.ResizeGrids(northRows, southRows, columns);
    }

    public void AddNorthRow() { northRows++; ValidateSettings(); }
    public void RemoveNorthRow() { if (northRows > 1) northRows--; ValidateSettings(); }
    public void AddSouthRow() { southRows++; ValidateSettings(); }
    public void RemoveSouthRow() { if (southRows > 1) southRows--; ValidateSettings(); }
    public void AddColumn() { columns++; ValidateSettings(); }
    public void RemoveColumn() { if (columns > 1) columns--; ValidateSettings(); }

    public void RandomizeAllGrids() => globalGridData.RandomizeAllGrids();
    public void SetAllGrids(int value) => globalGridData.SetAllGrids(value);
    public void ResetSettings() => globalGridData.ResetGrids(DEFAULT_ROWS, DEFAULT_COLUMNS);
    public GridData GetGridData(Direction dir) => globalGridData.GetGridData(dir);
    public List<List<int>> GetGridDataList(Direction dir) => globalGridData.GetGridDataList(dir);
    public void RandomizeGrid(Direction dir) => globalGridData.RandomizeGrid(dir);
}