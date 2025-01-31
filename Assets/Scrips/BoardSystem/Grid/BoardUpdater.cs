
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class BoardUpdater {
    [Inject] private BoardAssigner _boardAssigner;
    private BoardSettingsSO initialBoardConfig;
    [Inject] CommandManager CommandManager;

    public Func<BoardUpdateData, UniTask> OnGridChanged;
    public Func<BoardUpdateData, UniTask> OnGridInitialized;

    public GridBoard GridBoard { get; private set; }
    string address = "DefaulBoardSetting";

    public void SetInitialConfig(BoardSettingsSO config) {
        initialBoardConfig = config;
    }

    public async UniTask SpawnBoard() {
        if (initialBoardConfig == null) {
            await LoadBoardSettings(address);
        }
        if (initialBoardConfig == null) {
            Debug.LogError("Loaded settings is null");
        }
        await UpdateGrid(initialBoardConfig);
    }

    public async UniTask UpdateGrid(BoardSettingsSO newConfig) {
        if (!ValidateBoardSettings(newConfig)) return; // Перевірка перед реєстрацією команди

        ICommand command = GridBoard == null
            ? new GridInitCommand(SetMainGrid, newConfig, OnGridInitialized, OnGridChanged)
            : new BoardUpdateCommand(GridBoard, newConfig, OnGridChanged);

        CommandManager.RegisterCommand(command);

        try {
            await CommandManager.ExecuteCommands(); // Очікуємо виконання команд
        } catch (Exception ex) {
            Debug.LogError($"Помилка під час виконання команди: {ex.Message}");
        }
    }

    protected bool ValidateBoardSettings(BoardSettingsSO settings) {
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

    private void SetMainGrid(GridBoard newGridBoard) {
        GridBoard = newGridBoard;
    }
    public async UniTask LoadBoardSettings(string address) {
        var handle = Addressables.LoadAssetAsync<BoardSettingsSO>(address);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded) {
            initialBoardConfig = handle.Result;
            Debug.Log("Board settings successfully loaded.");
        } else {
            Debug.LogError("Failed to load board settings.");
        }
    }
}


    public class GridInitCommand : BaseBoardCommand {
    private Action<GridBoard> setMainGrid;
    private BoardSettingsSO newConfig;

    public GridInitCommand(Action<GridBoard> setMainGrid, BoardSettingsSO newConfig, Func<BoardUpdateData, UniTask> onGridInitialized, Func<BoardUpdateData, UniTask> onGridChanged) {
        this.setMainGrid = setMainGrid;
        this.newConfig = newConfig;
        OnBoardInitialized = onGridInitialized;
        OnBoardChanged = onGridChanged;
    }

    public override async UniTask Execute() {
        await InitGrid(newConfig);
    }

    public override async UniTask Undo() {
        await ResetGrid();
    }

    protected async UniTask InitGrid(BoardSettingsSO config) {
        board = new GridBoard();
        setMainGrid(board);

        BoardUpdateData gridUpdateData = board.UpdateGlobalGrid(config);
        if (OnBoardInitialized != null) {
            await OnBoardInitialized.Invoke(gridUpdateData);
        }
        await UniTask.CompletedTask;
    }

    protected async UniTask ResetGrid() {
        if (board == null) return;

        BoardUpdateData updateData = board.RemoveAll();

        if (OnBoardChanged != null) {
            await OnBoardChanged.Invoke(updateData);
        }

        setMainGrid(null); // ⚠️ Видаляє посилання на `GridBoard`
    }

}

public class BoardUpdateCommand : BaseBoardCommand {
    private readonly BoardSettingsSO oldSettings;
    private readonly BoardSettingsSO newSettings;

    public BoardUpdateCommand(GridBoard board, BoardSettingsSO newSettings, Func<BoardUpdateData, UniTask> onGridChanged) {
        this.board = board;
        this.newSettings = newSettings;
        oldSettings = board.Config;
        OnBoardChanged = onGridChanged;
    }

    public override async UniTask Execute() {
        await UpdateBoard(newSettings);
    }

    public override async UniTask Undo() {
        await UpdateBoard(oldSettings);
    }

    protected async UniTask UpdateBoard(BoardSettingsSO config) {
        BoardUpdateData updateData = board.UpdateGlobalGrid(config);

        if (OnBoardChanged != null) {
            await OnBoardChanged.Invoke(updateData);
        }

        await UniTask.CompletedTask;
    }
}


public abstract class BaseBoardCommand : ICommand {
    protected GridBoard board;
    public Func<BoardUpdateData, UniTask> OnBoardInitialized;
    public Func<BoardUpdateData, UniTask> OnBoardChanged;

    public abstract UniTask Execute();
    public abstract UniTask Undo();
}
