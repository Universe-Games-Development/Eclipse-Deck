using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class GridManager {
    [Inject] CommandManager CommandManager;

    public Action<GridUpdateData> OnGridChanged;
    public Action<GridUpdateData> OnGridInitialized;

    public Grid MainGrid { get; private set; }

    public void UpdateGrid(GridSettings newConfig) {
        ICommand command;
        if (MainGrid == null) {
            command = new GridInitCommand(SetMainGrid, newConfig, OnGridInitialized);
        } else {
            command = new GridUpdateCommand(MainGrid, newConfig, OnGridChanged);
        }
        CommandManager.RegisterCommand(command);
        CommandManager.ExecuteCommands().Forget();
    }

    private void SetMainGrid(Grid mainGrid) {
        MainGrid = mainGrid;
    }
}

public class GridInitCommand : BaseGridCommand {
    private Action<Grid> setMainGrid;
    private GridSettings newConfig;

    public GridInitCommand(Action<Grid> setMainGrid, GridSettings newConfig, Action<GridUpdateData> onGridInitialized) {
        this.setMainGrid = setMainGrid;
        this.newConfig = newConfig;
        OnGridCInitialized = onGridInitialized;
    }

    public override async UniTask Execute() {
        if (!ValidateBoardSettings(newConfig)) {
            Debug.LogWarning("Invalid BoardSettings. Execution halted.");
            return;
        }
        await InitGrid(newConfig);
    }

    public override async UniTask Undo() {
        await ResetGrid();
    }

    protected async UniTask InitGrid(GridSettings config) {
        grid = new Grid();
        setMainGrid.Invoke(grid);

        GridUpdateData gridUpdateData = grid.UpdateGrid(config);
        UpdateFieldTypes(config);

        OnGridCInitialized?.Invoke(gridUpdateData);
        await UniTask.Yield();
    }


    protected async UniTask ResetGrid() {
        OnGridChanged?.Invoke(grid.RemoveAll());
        await UniTask.Yield();
    }
}

public class GridUpdateCommand : BaseGridCommand {
    private GridSettings oldSettings;
    private GridSettings newSettings;
    
    public GridUpdateCommand(Grid grid, GridSettings newSettings, Action<GridUpdateData> onGridChanged) {
        this.grid = grid;
        this.newSettings = newSettings;
        oldSettings = grid.GetConfig();
        OnGridChanged = onGridChanged;
    }

    public override async UniTask Execute() {
        await UpdateGrid(newSettings);
    }

    public override async UniTask Undo() {
        await UpdateGrid(oldSettings);
    }

    protected async UniTask UpdateGrid(GridSettings config) {
        if (!ValidateBoardSettings(oldSettings)) {
            Debug.LogWarning("Invalid BoardSettings. Execution halted.");
            return;
        }

        GridUpdateData updateData = grid.UpdateGrid(config);
        UpdateFieldTypes(config);

        OnGridChanged?.Invoke(updateData);

        await UniTask.Yield();
    }
}


public abstract class BaseGridCommand : ICommand {
    protected Grid grid;
    public Action<GridUpdateData> OnGridChanged;
    public Action<GridUpdateData> OnGridCInitialized;

    protected void UpdateFieldTypes(GridSettings config) {
        foreach (List<Field> row in grid.Fields) {
            foreach (Field field in row) {
                if (config.RowTypes[field.row] == FieldType.Attack) {
                    field.Type = FieldType.Attack;
                } else {
                    field.Type = FieldType.Support;
                }
            }
        }
    }

    protected bool ValidateBoardSettings(GridSettings settings) {
        if (settings == null) {
            Debug.LogWarning("Accepted config null!");
            return false;
        }

        if (settings.RowTypes.Count < 2 || settings.columns < 2) {
            Debug.LogWarning("BoardSettings must have at least 2 rows and 2 columns.");
            return false;
        }

        if (!HasTwoAdjacentAttackRows(settings.RowTypes)) {
            Debug.LogWarning("BoardSettings must have exactly 2 adjacent Attack rows.");
            return false;
        }

        return true;
    }

    private bool HasTwoAdjacentAttackRows(List<FieldType> rowTypes) {
        var attackIndices = rowTypes
            .Select((type, index) => type == FieldType.Attack ? index : -1)
            .Where(index => index != -1)
            .ToList();

        return attackIndices.Count == 2 && Mathf.Abs(attackIndices[1] - attackIndices[0]) == 1;
    }


    public abstract UniTask Execute();
    public abstract UniTask Undo();
}
