using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager {
    public Grid MainGrid { get; private set; }
    public SubGrid PlayerGrid { get; private set; }
    public SubGrid EnemyGrid { get; private set; }

    public GridManager(BoardSettings config) {
        UpdateGrid(config);
    }

    public void UpdateGrid(BoardSettings config) {
        int rows = config.rowTypes.Count;
        int columns = config.columns;
        int divider = config.rowTypes.FindIndex(row => row == FieldType.Attack);

        if (MainGrid != null) {
            MainGrid.UpdateGridSize(rows, columns);
            PlayerGrid.BoundToMainGrid(MainGrid, 0, divider);
            EnemyGrid.BoundToMainGrid(MainGrid, divider + 1, rows - 1);
        } else {
            MainGrid = new Grid(rows, columns);
            PlayerGrid = new SubGrid(MainGrid, 0, divider);
            EnemyGrid = new SubGrid(MainGrid, divider + 1, rows - 1);
        }
    }

    private void UpdateFieldTypes(BoardSettings config) {
        MainGrid.Fields
            .ForEach(row =>
            row.ForEach(field => { field.Type = config.rowTypes[field.row] == FieldType.Attack ? FieldType.Attack : FieldType.Support; }));
    }

    private void ValidateBoardSettings(BoardSettings settings) {
        if (settings == null) {
            throw new System.ArgumentException("Accepted config null!");
        }

        if (settings.rowTypes.Count < 2 || settings.columns < 2) {
            throw new System.ArgumentException("BoardSettings must have at least 2 rows and 2 columns.");
        }

        if (!HasTwoAdjacentAttackRows(settings.rowTypes)) {
            throw new System.ArgumentException("BoardSettings must have exactly 2 adjacent Attack rows.");
        }
    }

    private BoardSettings GenerateDefaultBoardSettings() {
        BoardSettings _boardSettings = new BoardSettings();
        _boardSettings.rowTypes[0] = FieldType.Attack;
        _boardSettings.rowTypes[1] = FieldType.Attack;
        _boardSettings.columns = 4;
        return _boardSettings;
    }

    private bool HasTwoAdjacentAttackRows(List<FieldType> rowTypes) {
        var attackIndices = rowTypes
            .Select((type, index) => type == FieldType.Attack ? index : -1)
            .Where(index => index != -1)
            .ToList();

        return attackIndices.Count == 2 && Mathf.Abs(attackIndices[1] - attackIndices[0]) == 1;
    }
}
