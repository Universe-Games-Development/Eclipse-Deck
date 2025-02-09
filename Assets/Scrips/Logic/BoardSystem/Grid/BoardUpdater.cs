using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class BoardUpdater :IEventListener {
    [Inject] private BoardAssigner _boardAssigner;
    private BoardSettingsSO initialBoardConfig;
    [Inject] CommandManager CommandManager;
    private IEventQueue eventQueue;

    public Func<BoardUpdateData, UniTask> OnBoardChanged;
    public Func<BoardUpdateData, UniTask> OnGridInitialized;

    public GridBoard GridBoard { get; private set; }
    string address = "DefaulBoardSetting";

    [Inject]
    public void Construct(IEventQueue eventQueue)
    {
        this.eventQueue = eventQueue;
        eventQueue.RegisterListener(this, EventType.BATTLE_START);

        LoadInitialConfig().Forget();
    }

    public async UniTask LoadInitialConfig() {
        if (initialBoardConfig != null) {
            await UniTask.CompletedTask;
        }

        var handle = Addressables.LoadAssetAsync<BoardSettingsSO>(address);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded) {
            initialBoardConfig = handle.Result;
            Debug.Log("Board settings successfully loaded.");
        } else {
            Debug.LogError("Failed to load board settings.");
        }
    }

    public async UniTask<GridBoard> InitBoard() {
        await LoadInitialConfig();
        if (initialBoardConfig == null) {
            Debug.LogError("Loaded settings is null");
        }

        GridBoard = new GridBoard(initialBoardConfig);
        return GridBoard;
    }

    public async UniTask UpdateGrid(BoardSettingsSO newConfig) {
        if (!ValidateBoardSettings(newConfig)) return; // Перевірка перед реєстрацією команди

        ICommand command = GridBoard == null
            ? new GridInitCommand(this, InitBoard)
            : new BoardUpdateCommand(this, GridBoard, newConfig);

        CommandManager.EnqueueCommand(command);

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
        return true;
    }

    public object OnEventReceived(object data) {
        return data switch {
            BattleStartEventData battleStartEventData => new GridInitCommand(this, InitBoard),
            _ => null
        };
    }
}


public class GridInitCommand : ICommand {
    private Func<UniTask<GridBoard>> initBoard;
    private BoardUpdater updater;

    private GridBoard cachedBoard;

    public GridInitCommand(BoardUpdater updater, Func<UniTask<GridBoard>> initBoard) {
        this.initBoard = initBoard;
        this.updater = updater;
    }

    public async UniTask Execute() {
        GridBoard gridBoard = await initBoard.Invoke();
        await InitGrid(gridBoard);
    }

    public async UniTask Undo() {
        await ResetGrid();
    }

    protected async UniTask InitGrid(GridBoard gridBoard) {
        BoardUpdateData gridUpdateData = gridBoard.UpdateGlobalGrid();
        if (updater.OnGridInitialized != null) {
            await updater.OnGridInitialized.Invoke(gridUpdateData);
        }
        await UniTask.CompletedTask;
    }

    protected async UniTask ResetGrid() {
        if (cachedBoard == null) return;

        BoardUpdateData updateData = cachedBoard.RemoveAll();

        if (updater.OnBoardChanged != null) {
            await updater.OnBoardChanged.Invoke(updateData);
        }
    }

}

public class BoardUpdateCommand : ICommand {
    private readonly BoardSettingsSO oldSettings;
    private readonly BoardSettingsSO newSettings;

    private BoardUpdater updater;
    private GridBoard board;

    public BoardUpdateCommand(BoardUpdater updater, GridBoard board, BoardSettingsSO newSettings) {
        this.board = board;
        this.newSettings = newSettings;
        this.updater = updater;
        oldSettings = board.Config;
    }

    public async UniTask Execute() {
        await UpdateBoard(newSettings);
    }

    public async UniTask Undo() {
        await UpdateBoard(oldSettings);
    }

    protected async UniTask UpdateBoard(BoardSettingsSO config) {
        BoardUpdateData updateData = board.UpdateGlobalGrid(config);

        if (updater.OnBoardChanged != null) {
            await updater.OnBoardChanged.Invoke(updateData);
        }

        await UniTask.CompletedTask;
    }
}
