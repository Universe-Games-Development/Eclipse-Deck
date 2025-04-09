using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BoardPresenter : MonoBehaviour {
    [Inject] private CommandManager _commandManager;
    [SerializeField] private BoardView _view;

    public GridBoard GridBoard;
    private FieldManager _fieldManager;
    private BoardLayoutCalculator _layoutCalculator;
    private BoardAssigner _boardAssigner;

    public void Initialize(BoardAssigner boardAssigner, BoardSettingsData boardSettingsData = null) {
        _boardAssigner = boardAssigner;

        GridBoard = new GridBoard();

        _view.Initialize();

        _fieldManager = new FieldManager(_view, GridBoard);
        _layoutCalculator = new BoardLayoutCalculator(_view, GridBoard);

        if (boardSettingsData)
        UpdateGrid(boardSettingsData);
    }

    // Публічний метод для командного оновлення
    public void UpdateGrid(BoardSettingsData newConfig) {
        Command command = new BoardUpdateCommand(GridBoard, newConfig, UpdateVisualAsync);
        _commandManager.EnqueueCommand(command);
    }

    public async UniTask UpdateVisualAsync(BoardUpdateData boardUpdateData) {
        _fieldManager.ProcessBoardUpdate(boardUpdateData);
        _layoutCalculator.AdjustBoardCenter();
        _boardAssigner.HandleGridUpdate(boardUpdateData);
        await UniTask.CompletedTask;
    }



    public FieldPresenter GetFieldPresenter(Field targetField) {
        return _fieldManager.GetFieldPresenter(targetField);
    }

    public bool TryGetField(Vector3 worldPosition, out Field field) {
        field = null;

        if (!_view.IsWithinYInteractionRange(worldPosition)) {
            return false;
        }

        Vector3 local = _view.GetBoardOrigin().InverseTransformPoint(worldPosition);
        if (!GridBoard.GetFieldByLocal(local, out field)) {
            return false;
        }
        return true;
    }

}

public class BoardUpdateCommand : Command {
    private readonly BoardSettingsData oldSettings;
    private readonly BoardSettingsData newSettings;
    Func<BoardUpdateData, UniTask> handleGridUpdate;
    private GridBoard board;

    public BoardUpdateCommand(GridBoard board, BoardSettingsData newSettings, Func<BoardUpdateData, UniTask> handleGridUpdate) {
        this.board = board;
        this.newSettings = newSettings;
        this.handleGridUpdate = handleGridUpdate;
        oldSettings = board.Config;
    }

    public async override UniTask Execute() {
        await UpdateBoard(newSettings);
    }

    public async override UniTask Undo() {
        await UpdateBoard(oldSettings);
    }

    protected async UniTask UpdateBoard(BoardSettingsData config) {
        BoardUpdateData updateData = board.UpdateGlobalGrid(config);
        await handleGridUpdate.Invoke(updateData);
    }
}
public class BoardLayoutCalculator {
    private BoardView boardView;
    private GridBoard gridBoard;

    public BoardLayoutCalculator(BoardView boardView, GridBoard gridBoard) {
        this.boardView = boardView;
        this.gridBoard = gridBoard;
    }

    public void AdjustBoardCenter() {
        Vector3 boardBalanceAmount = gridBoard.GetGridBalanceOffset() / 2;

        CellSize cellSize = gridBoard.Config.cellSize;
        Vector3 visualLocalOffsetBalance = new(
            boardBalanceAmount.x * cellSize.width,
            0,
            boardBalanceAmount.z * cellSize.height
        );

        Vector3 boardGlobalCenter = boardView.GetBoardParent().TransformPoint(visualLocalOffsetBalance);
        Vector3 offset = boardView.GetCenter().position - boardGlobalCenter;
        boardView.MoveTo(boardView.transform.position + offset);
    }
}
public class FieldManager {
    private Dictionary<Field, FieldPresenter> fieldPresenters = new();
    private BoardView boardView;
    private GridBoard gridBoard;

    public FieldManager(BoardView boardView, GridBoard gridBoard) {
        this.boardView = boardView;
        this.gridBoard = gridBoard;
    }

    public void ProcessBoardUpdate(BoardUpdateData updateData) {
        foreach (Field field in updateData.GetAllAddedFields()) {
            AddField(field);
        }

        foreach (Field field in updateData.GetAllRemovedFields()) {
            RemoveField(field);
        }

        foreach (Field field in updateData.GetAllEmptyFields()) {
            RemoveField(field);
        }
    }

    public void AddField(Field field) {
        if (fieldPresenters.ContainsKey(field)) {
            Debug.Log($"Trying to add exist field at {field.GetTextCoordinates()}");
            return;
        }

        Vector3 spawnPosition = CalculateFieldPosition(field);
        FieldPresenter fieldPresenter = boardView.SpawnFieldAt(spawnPosition);
        fieldPresenters.Add(field, fieldPresenter);

        fieldPresenter.gameObject.name = $"Field {field.GetTextCoordinates()} {field.FieldType}";
        fieldPresenter.Initialize(field);
        fieldPresenter.InitializeLevitator(spawnPosition);
    }

    public void RemoveField(Field field) {
        if (fieldPresenters.TryGetValue(field, out FieldPresenter fieldController)) {
            field.RemoveField();
            fieldPresenters.Remove(field);
            fieldController.RemoveController().Forget();
        }
    }

    public FieldPresenter GetFieldPresenter(Field field) {
        if (!fieldPresenters.TryGetValue(field, out FieldPresenter controller)) {
            Debug.LogWarning("No controller for field : " + field.GetCoordinates());
            return null;
        }
        return controller;
    }

    private Vector3 CalculateFieldPosition(Field field) {
        return boardView.GetBoardOrigin().TransformPoint(new Vector3(
            field.GetColumn() * gridBoard.Config.cellSize.width,
            0f,
            field.GetRow() * gridBoard.Config.cellSize.height
        ));
    }

    public void RemoveAll() {
        foreach (var field in new List<Field>(fieldPresenters.Keys)) {
            RemoveField(field);
        }
    }
}