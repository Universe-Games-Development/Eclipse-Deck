using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BoardView : MonoBehaviour {

    [Header("Pool")]
    [Inject] private IComponentPool<Cell3DView> cellPool;

    [Header("Components")]
    [SerializeField] Transform cellParent;
    [SerializeField] private CellLayoutComponent layoutComponent;

    [Header("Board Settings")]
    [SerializeField] private int initialRows = 8;
    [SerializeField] private int initialColumns = 8;
    [SerializeField] private bool createOnAwake = true;

    [Header("Animation")]
    [SerializeField] private float cellCreationDelay = 0.05f;
    [SerializeField] private bool animateCellCreation = true;

    public event Action<Cell3DView> OnCellCreated;
    public event Action<Cell3DView> OnCellRemoved;
    public event Action<Cell3DView, Cell3DView> OnCellsSwapped;
    public event Action OnBoardCreated;
    public event Action OnBoardCleared;

    private void Awake() {
        ValidateReferences();

        if (createOnAwake) {
            CreateBoard().Forget();
        }
    }

    private void ValidateReferences() {
        if (cellPool == null) {
            throw new UnassignedReferenceException(nameof(cellPool));
        }
        if (layoutComponent == null) {
            throw new UnassignedReferenceException(nameof(layoutComponent));
        }
    }

    #region Board Creation & Management

    /// <summary>
    /// �������� ��� �����
    /// </summary>
    public async UniTask CreateBoard() {
        ClearBoard();

        // ������������ ����� ����
        layoutComponent.ResizeGrid(initialRows, initialColumns, recalculate: false);

        // ��������� �������
        for (int row = 0; row < initialRows; row++) {
            for (int col = 0; col < initialColumns; col++) {
                await CreateCellAsync(row, col, doLayout: false);

                if (animateCellCreation && cellCreationDelay > 0) {
                    await UniTask.Delay(TimeSpan.FromSeconds(cellCreationDelay));
                }
            }
        }

        // ���� ��� ������������ layout ���� ��������� ��� �������
        layoutComponent.RecalculateLayout();

        // ������ �� �������
        if (animateCellCreation) {
            await layoutComponent.AnimateAllToLayoutPositions();
        }

        OnBoardCreated?.Invoke();
        Debug.Log($"Board created: {initialRows}x{initialColumns}");
    }

    /// <summary>
    /// �������� ��� �����
    /// </summary>
    public void ClearBoard() {
        layoutComponent.ClearItems();
        OnBoardCleared?.Invoke();
    }

    /// <summary>
    /// ������ ����� �����
    /// </summary>
    public async UniTask ResizeBoard(int newRows, int newColumns) {
        if (newRows == initialRows && newColumns == initialColumns) return;

        initialRows = newRows;
        initialColumns = newColumns;

        await CreateBoard();
    }

    #endregion

    #region Cell Operations

    /// <summary>
    /// �������� ���� �������
    /// </summary>
    public async UniTask<Cell3DView> CreateCellAsync(
        int row, int column, bool doLayout = true) {

        if (layoutComponent.IsCellOccupied(row, column)) {
            Debug.LogWarning($"Cell at ({row}, {column}) already exists");
            return layoutComponent.GetCellAt(row, column);
        }

        Cell3DView cell = cellPool.Get();
        cell.transform.SetParent(cellParent);
        cell.name = $"Cell_{row}_{column}";

        layoutComponent.AddItem(cell, row, column, false);

        if (doLayout) {
            layoutComponent.RecalculateLayout();
            await layoutComponent.AnimateToLayoutPosition(cell);
        }

        OnCellCreated?.Invoke(cell);
        return cell;
    }

    /// <summary>
    /// �������� �������
    /// </summary>
    public async UniTask RemoveCellAsync(int row, int column, bool animate = true) {
        var cell = layoutComponent.GetCellAt(row, column);
        if (cell == null) return;

        if (animate) {
            await AnimateCellRemoval(cell);
        }

        layoutComponent.RemoveAt(row, column);
        OnCellRemoved?.Invoke(cell);
        cell.Clear();
        cellPool.Release(cell);
    }


    /// <summary>
    /// �������� ������� �� ��������
    /// </summary>
    public Cell3DView GetCell(int row, int column) {
        return layoutComponent.GetCellAt(row, column);
    }

    /// <summary>
    /// �������� ������� �������
    /// </summary>
    public (int row, int column)? GetCellPosition(Cell3DView cell) {
        return layoutComponent.GetItemPosition(cell);
    }

    /// <summary>
    /// ��������� �� ������� �������
    /// </summary>
    public bool IsCellOccupied(int row, int column) {
        return layoutComponent.IsCellOccupied(row, column);
    }

    /// <summary>
    /// �������� �� ������ �������
    /// </summary>
    public IEnumerable<(int row, int column, Cell3DView cell)> GetOccupiedCells() {
        return layoutComponent.GetOccupiedPositions();
    }

    #endregion

    #region Cell Interactions

    /// <summary>
    /// ������� ������ �� �������
    /// </summary>
    public async UniTask SwapCells(int fromRow, int fromCol, int toRow, int toCol) {
        var cell1 = GetCell(fromRow, fromCol);
        var cell2 = GetCell(toRow, toCol);

        if (cell1 == null || cell2 == null) {
            Debug.LogWarning("Cannot swap - one or both cells are null");
            return;
        }

        // ������ ���� ���������
        var task1 = layoutComponent.AnimateToLayoutPosition(cell1);
        var task2 = layoutComponent.AnimateToLayoutPosition(cell2);

        await UniTask.WhenAll(task1, task2);

        OnCellsSwapped?.Invoke(cell1, cell2);
    }

    /// <summary>
    /// ���������� �������
    /// </summary>
    public async UniTask MoveCell(int fromRow, int fromCol, int toRow, int toCol) {
        var cell = GetCell(fromRow, fromCol);
        if (cell == null) return;

        if (IsCellOccupied(toRow, toCol)) {
            Debug.LogWarning($"Target position ({toRow}, {toCol}) is occupied");
            return;
        }

        layoutComponent.MoveItem(cell, toRow, toCol);
        await layoutComponent.AnimateToLayoutPosition(cell);
    }

    /// <summary>
    /// �������� ����� �������
    /// </summary>
    public Cell3DView[] GetNeighborCells(int row, int column) {
        return layoutComponent.GetNeighborCells(row, column);
    }

    #endregion

    #region Board State & Validation
    /// <summary>
    /// �������� ������� �������� �������
    /// </summary>
    public int GetOccupiedCellCount() {
        return layoutComponent.OccupiedCells;
    }


    /// <summary>
    /// ��������� �� ����� �������
    /// </summary>
    public bool IsBoardEmpty() {
        return GetOccupiedCellCount() == 0;
    }

    #endregion

    #region Animation Helpers

    /// <summary>
    /// ������� ��������� �������
    /// </summary>
    private async UniTask AnimateCellRemoval(Cell3DView cell) {
        // ����� ������ ����� ���������, ������������� ����
        var sequence = DOTween.Sequence()
            .Append(cell.transform.DOScale(Vector3.zero, 0.3f))
            .SetEase(Ease.InBack);

        await sequence.Play().ToUniTask();
    }

    public async UniTask UpdateLayoutAsync() {
        layoutComponent.RecalculateLayout();
        await layoutComponent.AnimateAllToLayoutPositions();
    }

    #endregion

    public bool IsAnimating => layoutComponent.IsAnimating;
}
