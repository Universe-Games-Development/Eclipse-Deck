using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class GridManager {

    [Inject] BoardVisual boardVisual;
    [Inject] OpponentManager opponentManager;

    public Grid MainGrid { get; private set; }
    

    public void UpdateGrid(BoardSettings config) {
        ValidateBoardSettings(config);

        int rows = config.RowTypes.Count;
        int columns = config.columns;
        int divider = config.RowTypes.FindIndex(row => row == FieldType.Attack);

        if (MainGrid != null) {
            MainGrid.UpdateGridSize(rows, columns);
        } else {
            MainGrid = new Grid(rows, columns, config.cellSize);
            opponentManager.SetGrid(MainGrid, config);
            boardVisual.SetGrid(MainGrid).Forget();
        }

        UpdateFieldTypes(config);
    }

    private void UpdateFieldTypes(BoardSettings config) {
        MainGrid.Fields
            .ForEach(row =>
            row.ForEach(field => { field.Type = config.RowTypes[field.row] == FieldType.Attack ? FieldType.Attack : FieldType.Support; }));
    }

    
    private void ValidateBoardSettings(BoardSettings settings) {
        if (settings == null) {
            throw new System.ArgumentException("Accepted config null!");
        }

        if (settings.RowTypes.Count < 2 || settings.columns < 2) {
            throw new System.ArgumentException("BoardSettings must have at least 2 rows and 2 columns.");
        }

        if (!HasTwoAdjacentAttackRows(settings.RowTypes)) {
            throw new System.ArgumentException("BoardSettings must have exactly 2 adjacent Attack rows.");
        }
    }

    private bool HasTwoAdjacentAttackRows(List<FieldType> rowTypes) {
        var attackIndices = rowTypes
            .Select((type, index) => type == FieldType.Attack ? index : -1)
            .Where(index => index != -1)
            .ToList();

        return attackIndices.Count == 2 && Mathf.Abs(attackIndices[1] - attackIndices[0]) == 1;
    }
}
