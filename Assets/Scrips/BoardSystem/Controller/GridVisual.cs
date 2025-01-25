using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GridVisual : MonoBehaviour {
    [SerializeField] private FieldPool pool;

    GridManager gridManager;
    [Inject] OpponentManager opponentManager;

    private CellSize visualCellSize;
    private float xOffset;
    private float yOffset;

    [Header("Board Adjuster")]
    [SerializeField] private Transform origin;
    [SerializeField] private Transform globalCenter;

    [Header("Grid Interaction Params")]
    [Range(0, 10)]
    public float yInteractionRange = 1f;

    private Dictionary<Field, FieldController> fieldControllers = new();

    [Inject]
    public void Construct(GridManager gridManager) {
        this.gridManager = gridManager;
        gridManager.OnGridInitialized += UpdateVisualGrid;
        gridManager.OnGridChanged += UpdateVisualGrid;
        pool.InitPool();
    }

    public void UpdateVisualGrid(GridUpdateData gridUpdateData) {
        UpdateGridDimensions(gridManager.MainGrid);
        UpdateGrid(gridUpdateData);
        AdjustCenter();
    }

    public void UpdateGrid(GridUpdateData gridUpdateData) {
        foreach(Field field in gridUpdateData.addedFields) {
            AddField(field);
        }

        foreach (Field field in gridUpdateData.removedFields) {
            RemoveField(field);
        }
    }

    private void UpdateGridDimensions(Grid grid) {
        GridSettings gridSettings = grid.GetConfig();

        visualCellSize.width = gridSettings.cellSize.width;
        visualCellSize.height= gridSettings.cellSize.height;

        xOffset = visualCellSize.width / 2;
        yOffset = visualCellSize.height / 2;
    }

    private void AdjustCenter() {
        Vector3 boardLocalCenter = gridManager.MainGrid.GetGridCenter();
        Vector3 visualLocalCenter = new Vector3(boardLocalCenter.x * visualCellSize.width, 0, boardLocalCenter.z * visualCellSize.height);
        Vector3 boardGlobalCenter = origin.TransformPoint(boardLocalCenter);
        Vector3 offset = globalCenter.position - boardGlobalCenter;
        origin.DOMove(origin.position + offset, 0.5f)
            .SetEase(Ease.InOutSine);
    }

    public void AddField(Field field) {
        Vector3 spawnPosition = origin.TransformPoint(new Vector3(
            field.row * visualCellSize.width + xOffset,
            0f,
            field.column * visualCellSize.height + yOffset
        ));

        FieldController fieldController = pool.GetField(field, spawnPosition);
        fieldControllers.Add(field, fieldController);
    }

    public void RemoveField(Field field) {
        if (fieldControllers.TryGetValue(field, out FieldController fieldController)) {
            field.RemoveField();
            fieldControllers.Remove(field);
            pool.ReleaseField(fieldController);
        }
    }

    public void RemoveAll() {
        foreach (var field in fieldControllers.Keys) {
            RemoveField(field);
        }
    }

    public Vector2Int? GetGridIndex(Vector3 worldPosition) {
        if (Mathf.Abs(worldPosition.y - origin.position.y) > yInteractionRange) {
            return null;
        }

        return gridManager.MainGrid.GetGridIndexByWorld(origin, worldPosition);
    }
}
