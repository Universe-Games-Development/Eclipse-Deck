using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class GridVisual : MonoBehaviour {
    private FieldPool pool;

    BoardUpdater baordUpdater;

    [SerializeField] private CellSize visualCellSize = new(1.0f, 1.0f);
    [Header("Field Data")]
    [SerializeField] private FieldController fieldPrefab;
    [Header("Board Adjuster")]
    [SerializeField] private Transform origin;
    [SerializeField] private Transform globalCenter;

    [Header("Grid Interaction Params")]
    [Range(0, 10)]
    public float yInteractionRange = 1f;

    private Dictionary<Field, FieldController> fieldControllers = new();

    [Inject]
    public void Construct(BoardUpdater gridManager) {
        this.baordUpdater = gridManager;
        gridManager.OnGridInitialized += UpdateVisualGrid;
        gridManager.OnGridChanged += UpdateVisualGrid;
        pool = new FieldPool(fieldPrefab, origin);
    }

    public async UniTask UpdateVisualGrid(BoardUpdateData boardUpdateData) {
        UpdateGrid(boardUpdateData);
        AdjustCenter();
        await UniTask.Yield();
    }

    public void UpdateGrid(BoardUpdateData boardUpdateData) {
        foreach(Field field in boardUpdateData.GetAllAddedFields()) {
            AddField(field);
        }

        foreach (Field field in boardUpdateData.GetAllRemovedFields()) {
            RemoveField(field);
        }

        foreach (Field field in boardUpdateData.GetAllEmptyFields()) {
            RemoveField(field);
        }
    }

    private void AdjustCenter() {
        Vector3 boardBalanceAmount = baordUpdater.GridBoard.GetGridBalanceOffset() / 2;
        Vector3 visualLocalOffsetBalance = new(boardBalanceAmount.x * visualCellSize.width, 0, boardBalanceAmount.z * visualCellSize.height);

        Vector3 boardGlobalCenter = origin.TransformPoint(visualLocalOffsetBalance);
        Vector3 offset = globalCenter.position - boardGlobalCenter;
        origin.DOMove(origin.position + offset, 0.5f)
            .SetEase(Ease.InOutSine);
    }

    public void AddField(Field field) {

        if (fieldControllers.ContainsKey(field)) {
            Debug.Log($"Trying to add exist field at {field.GetTextCoordinates()}");
            return;
        }
        Vector3 spawnPosition = origin.TransformPoint(new Vector3(
            field.GetColumn() * visualCellSize.width,
            0f,
            field.GetRow() * visualCellSize.height
        ));

        FieldController fieldController = pool.Get();

        fieldController.transform.localPosition = spawnPosition;
        fieldController.gameObject.name = $"Field {field.GetTextCoordinates()} {field.FieldType}";
        fieldController.Initialize(field);
        fieldController.InitializeLevitator(spawnPosition);

        fieldControllers.Add(field, fieldController);
    }

    public void RemoveField(Field field) {
        if (fieldControllers.TryGetValue(field, out FieldController fieldController)) {
            field.RemoveField();
            fieldControllers.Remove(field);
            fieldController.RemoveController().Forget();
        }
    }

    public void RemoveAll() {
        foreach (var field in fieldControllers.Keys) {
            RemoveField(field);
        }
    }

    public Vector2Int? GetGridIndex(Vector3 worldPosition) {
        if (baordUpdater == null || baordUpdater.GridBoard == null) {
            return null;
        }

        if (Mathf.Abs(worldPosition.y - origin.position.y) > yInteractionRange) {
            return null;
        }

        return baordUpdater.GridBoard.GetGridIndexByWorld(origin, worldPosition);
    }
}
