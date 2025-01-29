using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class GridManager {
    [Inject] CommandManager CommandManager;

    public Action<BoardUpdateData> OnGridChanged;
    public Action<BoardUpdateData> OnGridInitialized;

    public GridBoard GridBoard { get; private set; }

    public void UpdateGrid(GridSettings newConfig) {

        ICommand command;
        if (GridBoard == null) {
            command = new GridInitCommand(SetMainGrid, newConfig, OnGridInitialized);
        } else {
            command = new BoardUpdateCommand(GridBoard, newConfig, OnGridChanged);
        }
        CommandManager.RegisterCommand(command);
        CommandManager.ExecuteCommands().Forget();
    }

    private void SetMainGrid(GridBoard newGridBoard) {
        GridBoard = newGridBoard;
    }
}

public class GridInitCommand : BaseBoardCommand {
    private Action<GridBoard> setMainGrid;
    private GridSettings newConfig;

    public GridInitCommand(Action<GridBoard> setMainGrid, GridSettings newConfig, Action<BoardUpdateData> onGridInitialized) {
        this.setMainGrid = setMainGrid;
        this.newConfig = newConfig;
        OnBoardCInitialized = onGridInitialized;
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
        board = new GridBoard(config);
        setMainGrid.Invoke(board);

        BoardUpdateData gridUpdateData = board.UpdateGlobalGrid(config);

        OnBoardCInitialized?.Invoke(gridUpdateData);
        await UniTask.Yield();
    }


    protected async UniTask ResetGrid() {
        OnBoardChanged?.Invoke(board.RemoveAll());
        await UniTask.Yield();
    }
}

public class BoardUpdateCommand : BaseBoardCommand {
    private GridSettings oldSettings;
    private GridSettings newSettings;
    
    public BoardUpdateCommand(GridBoard board, GridSettings newSettings, Action<BoardUpdateData> onGridChanged) {
        this.board = board;
        this.newSettings = newSettings;
        oldSettings = board.Config;
        OnBoardChanged = onGridChanged;
    }

    public override async UniTask Execute() {
        await UpdateGrid(newSettings);
    }

    public override async UniTask Undo() {
        await UpdateGrid(oldSettings);
    }

    protected async UniTask UpdateGrid(GridSettings config) {
        if (!ValidateBoardSettings(config)) {
            Debug.LogWarning("Invalid BoardSettings. Execution halted.");
            return;
        }

        BoardUpdateData updateData = board.UpdateGlobalGrid(config);

        OnBoardChanged?.Invoke(updateData);

        await UniTask.Yield();
    }
}


public abstract class BaseBoardCommand : ICommand {
    protected GridBoard board;
    public Action<BoardUpdateData> OnBoardCInitialized;
    public Action<BoardUpdateData> OnBoardChanged;
    
    protected bool ValidateBoardSettings(GridSettings settings) {
        if (settings == null) {
            Debug.LogWarning("Accepted config null!");
            return false;
        }

        if (!settings.IsValidConfiguration()) {
            Debug.LogWarning("BoardSettings wrong configuration");
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
