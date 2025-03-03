using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class GameboardBuilder {
    private BoardSettingsSO initialBoardConfig;
    [Inject] CommandManager CommandManager;
    private GameEventBus eventBus;

    public Func<BoardUpdateData, UniTask> OnBoardChanged;
    public Func<BoardUpdateData, UniTask> OnGridInitialized;

    public GridBoard GridBoard { get; private set; }
    string address = "DefaulBoardSetting";

    [Inject]
    public void Construct(GameEventBus eventBus) {
        this.eventBus = eventBus;

        eventBus.SubscribeTo<BattleStartedEvent>(InitializeBoard);
    }

    private void InitializeBoard(ref BattleStartedEvent eventData) {
        CommandManager.EnqueueCommand(new GridInitCommand(this, InitBoard));
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

        Command command = GridBoard == null
            ? new GridInitCommand(this, InitBoard)
            : new BoardUpdateCommand(this, GridBoard, newConfig);

        CommandManager.EnqueueCommand(command);
    }

    protected bool ValidateBoardSettings(BoardSettingsSO settings) {
        if (settings == null) {
            Debug.LogWarning("Accepted config null!");
            return false;
        }
        return true;
    }
}


public class GridInitCommand : Command {
    private Func<UniTask<GridBoard>> initBoard;
    private GameboardBuilder boardManager;

    private GridBoard cachedBoard;

    public GridInitCommand(GameboardBuilder boardManager, Func<UniTask<GridBoard>> initBoard) {
        this.initBoard = initBoard;
        this.boardManager = boardManager;
    }

    public async override UniTask Execute() {
        GridBoard gridBoard = await initBoard.Invoke();
        await InitGrid(gridBoard);
    }

    public async override UniTask Undo() {
        await ResetGrid();
    }

    protected async UniTask InitGrid(GridBoard gridBoard) {
        BoardUpdateData gridUpdateData = gridBoard.UpdateGlobalGrid();
        if (boardManager.OnGridInitialized != null) {
            await boardManager.OnGridInitialized.Invoke(gridUpdateData);
        }
        await UniTask.CompletedTask;
    }

    protected async UniTask ResetGrid() {
        if (cachedBoard == null) return;

        BoardUpdateData updateData = cachedBoard.RemoveAll();

        if (boardManager.OnBoardChanged != null) {
            await boardManager.OnBoardChanged.Invoke(updateData);
        }
    }

}

public class BoardUpdateCommand : Command {
    private readonly BoardSettingsSO oldSettings;
    private readonly BoardSettingsSO newSettings;

    private GameboardBuilder boardManager;
    private GridBoard board;

    public BoardUpdateCommand(GameboardBuilder boardManager, GridBoard board, BoardSettingsSO newSettings) {
        this.board = board;
        this.newSettings = newSettings;
        this.boardManager = boardManager;
        oldSettings = board.Config;
    }

    public async override UniTask Execute() {
        await UpdateBoard(newSettings);
    }

    public async override UniTask Undo() {
        await UpdateBoard(oldSettings);
    }

    protected async UniTask UpdateBoard(BoardSettingsSO config) {
        BoardUpdateData updateData = board.UpdateGlobalGrid(config);

        if (boardManager.OnBoardChanged != null) {
            await boardManager.OnBoardChanged.Invoke(updateData);
        }

        await UniTask.CompletedTask;
    }
}
