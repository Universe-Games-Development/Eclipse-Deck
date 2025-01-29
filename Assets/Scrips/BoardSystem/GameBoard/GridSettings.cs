using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu (fileName ="GridBoardSettings", menuName = "GridSettings")]
public class GridSettings : ScriptableObject {
    private const int MIN_ATTACK_ROW_COUNT = 1;

    public List<FieldType> northRows;
    public List<FieldType> southRows;
    public List<int> westColumns;
    public List<int> eastColumns;
    public CellSize cellSize = new CellSize() { width = 1, height = 1 };

    public GridSettings() {
        Debug.LogWarning("Config reset to default!");
        ResetSettings();
    }

    #region Validation
    public void OnValidate() {
        if (!IsValidRow(northRows)) {
            northRows = GenerateDefaultRows();
        }
        if (!IsValidRow(southRows)) {
            southRows = GenerateDefaultRows();
        }
        if (!IsValidColumns()) {
            westColumns = GenerateDefaultColumns();
            eastColumns = GenerateDefaultColumns();
        }
    }

    public void ResetSettings() {
        northRows = GenerateDefaultRows();
        southRows = GenerateDefaultRows();
        westColumns = GenerateDefaultColumns();
        eastColumns = GenerateDefaultColumns();
    }

    public bool IsValidConfiguration() {
        return IsValidRow(northRows) && IsValidRow(southRows) && IsValidColumns();
    }

    private bool IsValidRow(List<FieldType> row) {
        if (row == null || row.Count < MIN_ATTACK_ROW_COUNT) {
            Debug.LogWarning("Wrong amount of rows. Need : " + MIN_ATTACK_ROW_COUNT);
            return false;
        }

        if (row[0] != FieldType.Attack) {
            Debug.LogWarning("First row is not attack type in settings");
            return false;
        }

        int attackRows = row.Count(rowtype => rowtype == FieldType.Attack);

        if (attackRows > MIN_ATTACK_ROW_COUNT || attackRows < MIN_ATTACK_ROW_COUNT) {
            Debug.LogWarning($"Wrong amount of attack rows {attackRows} is settings must be {MIN_ATTACK_ROW_COUNT} for each");
            return false;
        }

        return true;
    }

    private bool IsValidColumns() {
        bool leftColumnsEmpty = westColumns == null || westColumns.Count == 0;
        bool rightColumnsEmpty = eastColumns == null || eastColumns.Count == 0;

        if (leftColumnsEmpty && rightColumnsEmpty) {
            return false;
        }

        // Check if all values in westColumns are 0
        bool allLeftColumnsZero = westColumns != null && westColumns.All(value => value == 0);
        // Check if all values in eastColumns are 0
        bool allRightColumnsZero = eastColumns != null && eastColumns.All(value => value == 0);

        // If both column lists contain only zeros, return false
        if (allLeftColumnsZero && allRightColumnsZero) {
            return false;
        }

        return true;
    }



    #endregion

    #region Add/Remove Rows
    public void AddNorthRow(FieldType rowType) {
        AddRow(northRows, rowType);
    }

    public void AddSouthRow(FieldType rowType) {
        AddRow(southRows, rowType);
    }

    public void RemoveNorthRowAt(int index) {
        RemoveRowAt(northRows, index);
    }

    public void RemoveSouthRowAt(int index) {
        RemoveRowAt(southRows, index);
    }

    private void AddRow(List<FieldType> rows, FieldType rowType) {
        if (rows == null)
            rows = new List<FieldType>();
        rows.Add(rowType);
    }

    private void RemoveRowAt(List<FieldType> rows, int index) {
        if (rows != null && index >= 0 && index < rows.Count) {
            rows.RemoveAt(index);
        }
    }
    #endregion

    #region Add/Remove Columns
    public void AddLeftColumn(int value) {
        AddColumn(westColumns, value);
    }

    public void AddRightColumn(int value) {
        AddColumn(eastColumns, value);
    }

    private void AddColumn(List<int> columnList, int value) {
        if (columnList == null)
            columnList = new List<int>();
        columnList.Add(value);
    }

    public void RemoveLeftColumnAt(int index) {
        RemoveColumn(westColumns, index);
    }

    public void RemoveRightColumnAt(int index) {
        RemoveColumn(eastColumns, index);
    }

    private void RemoveColumn(List<int> columnList, int index) {
        if (columnList != null && index >= 0 && index < columnList.Count) {
            columnList.RemoveAt(index);
        }
    }

    public void SetEastColumns(List<int> list) {
        if (list != null) {
            eastColumns = new List<int>(list);
        }
    }

    public void SetWestColumns(List<int> list) {
        if (list != null) {
            westColumns = new List<int>(list);
        }
    }
    #endregion

    #region Default Generation
    private List<FieldType> GenerateDefaultRows() {
        return new List<FieldType> { FieldType.Attack, FieldType.Support };
    }

    private List<int> GenerateDefaultColumns() {
        return new List<int> { 1, 1}; // Example default column configuration
    }
    #endregion

    public void ResetToDefault() {
        northRows = GenerateDefaultRows();
        southRows = GenerateDefaultRows();
        westColumns = GenerateDefaultColumns();
        eastColumns = GenerateDefaultColumns();
    }
}
