using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardOverseer {
    private BoardSettings config;
    public BoardSettings Config {
        get => config;
        set {
            ValidateBoardSettings(value);
            config = value;
            GridManager.UpdateGrid(config);
        }
    }
    public GridManager GridManager { get; private set; }

    public GridNavigator mainNavigator { get; private set; }
    public GridNavigator playerNavigator { get; private set; }
    public GridNavigator enemyNavigator { get; private set; }

    public BoardOverseer(BoardSettings initialConfig) {
        ValidateBoardSettings(initialConfig);
        config = initialConfig;
        GridManager = new GridManager(Config);
        
        mainNavigator = new GridNavigator(GridManager.MainGrid);
        playerNavigator = new GridNavigator(GridManager.PlayerGrid);
        enemyNavigator = new GridNavigator(GridManager.EnemyGrid);
        Config = initialConfig;
    }

    

    public void UpdateBoard(BoardSettings newConfig) {
        Config = newConfig;
    }

    private void ValidateBoardSettings(BoardSettings settings) {
        if (settings.rowTypes.Count < 2 || settings.columns < 2) {
            throw new System.ArgumentException("BoardSettings must have at least 2 rows and 2 columns.");
        }

        if (!HasTwoAdjacentAttackRows(settings.rowTypes)) {
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

    public bool IsInitialized() {
        if (GridManager == null) {
            Debug.LogError("Managers are null");
            return false;
        }

        if (GridManager.MainGrid.Fields == null || GridManager.MainGrid.Fields.Count == 0) {
            Debug.LogError("MainGrid fields are not initialized.");
            return false;
        }
        return true;
    }
}
