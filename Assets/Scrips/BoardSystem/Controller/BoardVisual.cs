using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BoardVisual : MonoBehaviour {
    [SerializeField] private FieldPool pool;

    [Inject] GridManager gridManager;
    [Inject] OpponentManager opponentManager;

    private CellSize cellSize;
    private float xOffset;
    private float yOffset;
    [SerializeField] private Transform origin;
    [SerializeField] private Transform globalCenter;

    [Header("Board Adjuster")]
    private int currentColumns;
    private int currentRows;

    private float visualHeight;
    private float visualWidth;

    [Header("Grid Interaction Params")]
    [Range(0, 10)]
    public float yInteractionRange = 1f;

    private Grid grid;

    [Inject] CommandManager commandManager;

    private Dictionary<Field, FieldController> fieldControllers = new();
    private Queue<Field> fieldsToAdd = new();
    private Queue<Field> fieldsToRemove = new();

    public async UniTask SetGrid(Grid grid) {
        if (this.grid != null && this.grid != grid) {
            UnsubscribeGridEvents(this.grid);
        }
        this.grid = grid;
        SubscribeGridEvents(grid);

        UpdateGridDimensions(grid);


        commandManager.RegisterCommand(new AdjustCenterCommand(grid, cellSize, origin, globalCenter));
        await commandManager.ExecuteCommands();
        await SpawnBoard(grid);
    }

    private void UnsubscribeGridEvents(Grid grid) {
        grid.OnGridChangedAsync -= UpdateVisualGrid;
        opponentManager.OnFieldAssigned -= QueueAddField;
        opponentManager.OnFieldUnassigned -= QueueRemoveField;
    }

    private void SubscribeGridEvents(Grid grid) {
        grid.OnGridChangedAsync += UpdateVisualGrid;
        grid.OnAddField += QueueAddField;
        grid.OnRemoveField += QueueRemoveField;
    }

    private async UniTask UpdateVisualGrid(Grid grid) {
        QueueAdjustCenter(grid);
        await commandManager.ExecuteCommands();
    }

    private void QueueAddField(Field field) {
        commandManager.RegisterCommand(new AddFieldCommand(this, field));
    }

    private void QueueRemoveField(Field field) {
        commandManager.RegisterCommand(new RemoveFieldCommand(this, field));
    }

    private void QueueAdjustCenter(Grid grid) {
        commandManager.RegisterCommand(new AdjustCenterCommand(grid, cellSize, origin, globalCenter));
    }

    private void UpdateGridDimensions(Grid grid) {
        currentRows = grid.Fields.Count;
        currentColumns = grid.Fields[0].Count;

        cellSize = grid.cellSize;
        xOffset = cellSize.width / 2;
        yOffset = cellSize.height / 2;

        visualHeight = cellSize.height * currentColumns;
        visualWidth = cellSize.width * currentRows;
    }

    private async UniTask SpawnBoard(Grid grid) {
        foreach (var row in grid.Fields) {
            foreach (var field in row) {
                QueueAddField(field);
            }
        }
        await commandManager.ExecuteCommands();
    }

    public void AddField(Field field) {
        Vector3 spawnPosition = origin.TransformPoint(new Vector3(
            field.row * cellSize.width + xOffset,
            0f,
            field.column * cellSize.height + yOffset
        ));

        FieldController fieldController = pool.GetField(field, spawnPosition);
        fieldControllers.Add(field, fieldController);
    }

    public void RemoveField(Field field) {
        if (fieldControllers.TryGetValue(field, out FieldController fieldController)) {
            field.RemoveField();
            pool.ReleaseField(fieldController);
            fieldControllers.Remove(field);
        }
    }

    public Vector2Int? GetGridIndex(Vector3 worldPosition) {
        if (Mathf.Abs(worldPosition.y - origin.position.y) > yInteractionRange) {
            return null;
        }

        return grid.GetGridIndexByWorld(origin, worldPosition);
    }
}

public class AdjustCenterCommand : ICommand {
    private Grid grid;
    private CellSize cellSize;
    private Transform origin;
    private Transform globalCenter;
    private Vector3 initialPosition;

    public AdjustCenterCommand(Grid grid, CellSize cellSize, Transform origin, Transform globalCenter) {
        this.grid = grid;
        this.cellSize = cellSize;
        this.origin = origin;
        this.globalCenter = globalCenter;
        initialPosition = origin.position; 
    }

    public async UniTask Execute() {
        float visualHeight = cellSize.height * grid.Fields[0].Count;
        float visualWidth = cellSize.width * grid.Fields.Count; 

        Vector3 boardLocalCenter = new(visualWidth / 2, 0, visualHeight / 2);
        Vector3 boardGlobalCenter = origin.TransformPoint(boardLocalCenter);
        Vector3 offset = globalCenter.position - boardGlobalCenter;
        await origin.DOMove(origin.position + offset, 0.5f)
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
    }

    public async UniTask Undo() {
        // Move the origin back to its initial position
        await origin.DOMove(initialPosition, 0.5f)
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
    }
}


public class AddFieldCommand : ICommand {
    private Field field;
    private BoardVisual boardVisual;

    public AddFieldCommand(BoardVisual boardVisual, Field field) {
        this.boardVisual = boardVisual;
        this.field = field;
    }

    public async UniTask Execute() {
        Debug.Log("Add Field : " + field.row + " " + field.column);
         boardVisual.AddField(field);
        await UniTask.Yield();
    }

    public async UniTask Undo() {
        boardVisual.RemoveField(field);
        await UniTask.Yield();
    }
}

public class RemoveFieldCommand : ICommand {
    private Field field;
    private BoardVisual boardVisual;

    public RemoveFieldCommand(BoardVisual boardVisual, Field field) {
        this.boardVisual = boardVisual;
        this.field = field;
    }

    public async UniTask Execute() {
        Debug.Log("Remove Field : " + field.row + " " + field.column);
        boardVisual.RemoveField(field);
        await UniTask.Yield();
    }

    public async UniTask Undo() {
        boardVisual.AddField(field);
        await UniTask.Yield();
    }
}