using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using Zenject;

public class BoardVisual: MonoBehaviour {

    [SerializeField] private FieldPool pool;

    [Inject] GridManager gridManager;

    private CellSize cellSize;
    private float xOffset;
    private float yOffset;
    [SerializeField] private Transform origin;
    [SerializeField] private Transform globalCenter;

    [Header("Board Spawner")]
    [SerializeField] private int spawnDelayMS = 15;
    public GameObject fieldPrefab;

    [Header("Board Adjuster")]

    private int columns;
    private int rows;

    private float visualHeight;
    private float visualWidth;

    [Header("Grid Interaction Params")]
    [Range(0, 10)]
    public float yInteractionRange = 1f;

    private Grid grid;
    private List<List<FieldController>> fieldControllers;


    private void Awake() {
        gridManager.OnGridInitialized += async () => {
            await SetGrid(gridManager.MainGrid);
        };
    }

    public async UniTask SetGrid(Grid grid) {
        if (this.grid != null && this.grid != grid) {
            this.grid.OnGridChanged -= UpdateCenter;
        }
        this.grid = grid;
        grid.OnGridChanged += UpdateCenter;

        rows = grid.Fields.Count;
        columns = grid.Fields[0].Count;

        cellSize = grid.cellSize;
        xOffset = cellSize.width / 2;
        yOffset = cellSize.height / 2;

        visualHeight = cellSize.height * columns;
        visualWidth = cellSize.width * rows;

        fieldControllers = new List<List<FieldController>>(rows);
        for (int i = 0; i < rows; i++) {
            fieldControllers.Add(new List<FieldController>(columns));
            for (int j = 0; j < columns; j++) {
                fieldControllers[i].Add(null);
            }
        }

        await UpdateCenter(grid);
        await SpawnFields();
    }

    private async UniTask SpawnFields() {
        for (int x = 0; x < rows; x++) {
            for (int y = 0; y < columns; y++) {
                Vector3 spawnPosition = origin.TransformPoint(new Vector3(x * cellSize.width + xOffset, 0f, y * cellSize.height + yOffset));
                fieldControllers[x][y] = pool.GetField(grid.Fields[x][y], spawnPosition);
            }
        }
        await UniTask.Delay(spawnDelayMS);
    }


    private async UniTask UpdateBoard(Grid grid) {

    }

    private async UniTask UpdateCenter(Grid grid) {
        visualHeight = cellSize.height * grid.Fields[0].Count; // grid rows
        visualWidth = cellSize.width * grid.Fields.Count;       // grid columns 

        Vector3 boardLocalCenter = new Vector3(visualWidth / 2, 0, visualHeight / 2);
        Vector3 boardGlobalCenter = origin.TransformPoint(boardLocalCenter);
        Vector3 offset = globalCenter.position - boardGlobalCenter;
        await origin.DOMove(origin.position + offset, 0.5f)
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
    }

    public Vector2Int? GetGridIndex(Vector3 worldPosition) {
        if (Mathf.Abs(worldPosition.y - origin.position.y) > yInteractionRange) {
            return null;
        }

        return grid.GetGridIndexByWorld(origin, worldPosition);
    }
}
