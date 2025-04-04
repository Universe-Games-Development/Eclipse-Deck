using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BoardView : MonoBehaviour {
    private FieldPool pool;

    GameboardBuilder boardManager;

    [SerializeField] private CellSize visualCellSize = new(1.0f, 1.0f);
    [Header("Field Data")]
    [SerializeField] private FieldPresenter fieldPrefab;
    [Header("Board Adjuster")]
    [SerializeField] private Transform origin;
    [SerializeField] private Transform globalCenter;

    private Dictionary<Field, FieldPresenter> fieldControllers = new();

    [Inject]
    public void Construct(GameboardBuilder boardManager) {
        this.boardManager = boardManager;
        boardManager.OnGridInitialized += UpdateVisualGrid;
        boardManager.OnBoardChanged += UpdateVisualGrid;
        if (fieldPrefab == null) throw new ArgumentNullException("field prefab is null"); 
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
        Vector3 boardBalanceAmount = boardManager.GridBoard.GetGridBalanceOffset() / 2;
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

        FieldPresenter fieldController = pool.Get();
        fieldControllers.Add(field, fieldController);

        fieldController.transform.localPosition = spawnPosition;
        fieldController.gameObject.name = $"Field {field.GetTextCoordinates()} {field.FieldType}";
        fieldController.Initialize(field);
        fieldController.InitializeLevitator(spawnPosition);
    }

    public void RemoveField(Field field) {
        if (fieldControllers.TryGetValue(field, out FieldPresenter fieldController)) {
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

    internal Transform GetBoardOrigin() {
        return origin;
    }

    internal FieldPresenter GetController(Field targetField) {
        if (!fieldControllers.TryGetValue(targetField, out FieldPresenter controller)) {
            Debug.LogWarning("No controller for field : " + targetField.GetCoordinates());
            return null;
        }
        return controller;
    }

    private void OnDestroy() {
        foreach(var pair in fieldControllers) {
            Destroy(pair.Value);
        }
    }
}
