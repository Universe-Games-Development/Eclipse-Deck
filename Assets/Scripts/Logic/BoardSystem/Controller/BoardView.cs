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

    [Inject] private IVisualManager _visualManager;

    public event Action<Cell3DView> OnCellCreated;
    public event Action<Cell3DView> OnCellRemoved;
    public event Action<Cell3DView, Cell3DView> OnCellsSwapped;
    public event Action OnBoardCreated;
    public event Action OnBoardCleared;

    private void Awake() {
        ValidateReferences();

        if (createOnAwake) {
            CreateBoard();
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

    #region Public Synchronous API (для Presenter'ів)

    /// <summary>
    /// Синхронний метод для створення дошки
    /// </summary>
    public void CreateBoard() {
        var createTask = new UniversalVisualTask(
            CreateBoardInternal,
            "Create Board"
        );
        _visualManager.Push(createTask);
    }

    /// <summary>
    /// Синхронний метод для оновлення layout
    /// </summary>
    public void UpdateLayout() {
        var layoutTask = new UniversalVisualTask(
            UpdateLayoutInternal,
            "Layout Update"
        );
        _visualManager.Push(layoutTask);
    }

    /// <summary>
    /// ✅ СТВОРИТИ CellView БЕЗ позиціонування (для негайного використання)
    /// </summary>
    public Cell3DView CreateCellView() {
        Cell3DView cell = cellPool.Get();
        cell.transform.SetParent(cellParent);
        cell.name = "Cell_Temp";
        cell.gameObject.SetActive(false); // 👈 Приховуємо до позиціонування

        return cell;
    }

    /// <summary>
    /// ✅ ДОДАТИ CellView НА конкретну позицію (через visualManager)
    /// </summary>
    public void AddCellView(Cell3DView cellView, int row, int column, bool doLayout = true) {
        var addCellTask = new UniversalVisualTask(
            () => AddCellViewInternal(cellView, row, column, doLayout),
            $"Add Cell View to ({row}, {column})"
        );
        _visualManager.Push(addCellTask);
    }

    /// <summary>
    /// Синхронний метод для видалення клітинки
    /// </summary>
    public void RemoveCell(int row, int column, bool animate = true) {
        var removeCellTask = new UniversalVisualTask(
            () => RemoveCellInternal(row, column, animate),
            $"Remove Cell ({row}, {column})"
        );
        _visualManager.Push(removeCellTask);
    }

    // Інші методи залишаються без змін...
    public void ResizeBoard(int newRows, int newColumns) {
        var resizeTask = new UniversalVisualTask(
            () => ResizeBoardInternal(newRows, newColumns),
            $"Resize Board to {newRows}x{newColumns}"
        );
        _visualManager.Push(resizeTask);
    }

    public void SwapCells(int fromRow, int fromCol, int toRow, int toCol) {
        var swapTask = new UniversalVisualTask(
            () => SwapCellsInternal(fromRow, fromCol, toRow, toCol),
            $"Swap Cells ({fromRow},{fromCol}) <-> ({toRow},{toCol})"
        );
        _visualManager.Push(swapTask);
    }

    public void MoveCell(int fromRow, int fromCol, int toRow, int toCol) {
        var moveTask = new UniversalVisualTask(
            () => MoveCellInternal(fromRow, fromCol, toRow, toCol),
            $"Move Cell ({fromRow},{fromCol}) -> ({toRow},{toCol})"
        );
        _visualManager.Push(moveTask);
    }

    #endregion

    #region Internal Async Implementation

    /// <summary>
    /// Внутрішня асинхронна реалізація створення дошки
    /// </summary>
    private async UniTask<bool> CreateBoardInternal() {
        ClearBoard();

        // Встановлюємо розмір сітки
        layoutComponent.ResizeGrid(initialRows, initialColumns, recalculate: false);

        // Створюємо клітинки
        for (int row = 0; row < initialRows; row++) {
            for (int col = 0; col < initialColumns; col++) {
                await CreateCellInternal(row, col, doLayout: false);

                if (animateCellCreation && cellCreationDelay > 0) {
                    await UniTask.Delay(TimeSpan.FromSeconds(cellCreationDelay));
                }
            }
        }

        layoutComponent.RecalculateLayout();

        if (animateCellCreation) {
            await layoutComponent.AnimateAllToLayoutPositions();
        }

        OnBoardCreated?.Invoke();
        Debug.Log($"Board created: {initialRows}x{initialColumns}");
        return true;
    }

    /// <summary>
    /// ✅ Внутрішня асинхронна реалізація додавання вже створеного CellView
    /// </summary>
    private async UniTask<bool> AddCellViewInternal(Cell3DView cellView, int row, int column, bool doLayout = true) {
        if (layoutComponent.IsCellOccupied(row, column)) {
            Debug.LogWarning($"Cell at ({row}, {column}) already exists");
            return false;
        }

        // Активуємо і налаштовуємо View
        cellView.gameObject.SetActive(true);
        cellView.name = $"Cell_{row}_{column}";

        // Додаємо до layout
        layoutComponent.AddItem(cellView, row, column, false);

        if (doLayout) {
            layoutComponent.RecalculateLayout();
            await layoutComponent.AnimateToLayoutPosition(cellView);
        }

        OnCellCreated?.Invoke(cellView);
        return true;
    }

    /// <summary>
    /// Внутрішня асинхронна реалізація створення клітинки (для CreateBoard)
    /// </summary>
    private async UniTask<bool> CreateCellInternal(int row, int column, bool doLayout = true) {
        if (layoutComponent.IsCellOccupied(row, column)) {
            Debug.LogWarning($"Cell at ({row}, {column}) already exists");
            return false;
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
        return true;
    }

    // Інші внутрішні методи залишаються без змін...
    private async UniTask<bool> UpdateLayoutInternal() {
        layoutComponent.RecalculateLayout();
        await layoutComponent.AnimateAllToLayoutPositions();
        return true;
    }

    private async UniTask<bool> RemoveCellInternal(int row, int column, bool animate = true) {
        var cell = GetCell(row, column);
        if (cell == null) return false;

        if (animate) {
            await AnimateCellRemoval(cell);
        }

        layoutComponent.RemoveAt(row, column);
        OnCellRemoved?.Invoke(cell);
        cell.Clear();
        cellPool.Release(cell);
        return true;
    }

    private async UniTask<bool> ResizeBoardInternal(int newRows, int newColumns) {
        if (newRows == initialRows && newColumns == initialColumns) return true;

        initialRows = newRows;
        initialColumns = newColumns;

        await CreateBoardInternal();
        return true;
    }

    private async UniTask<bool> SwapCellsInternal(int fromRow, int fromCol, int toRow, int toCol) {
        var cell1 = GetCell(fromRow, fromCol);
        var cell2 = GetCell(toRow, toCol);

        if (cell1 == null || cell2 == null) {
            Debug.LogWarning("Cannot swap - one or both cells are null");
            return false;
        }

        var task1 = layoutComponent.AnimateToLayoutPosition(cell1);
        var task2 = layoutComponent.AnimateToLayoutPosition(cell2);

        await UniTask.WhenAll(task1, task2);
        OnCellsSwapped?.Invoke(cell1, cell2);
        return true;
    }

    private async UniTask<bool> MoveCellInternal(int fromRow, int fromCol, int toRow, int toCol) {
        var cell = GetCell(fromRow, fromCol);
        if (cell == null) return false;

        if (IsCellOccupied(toRow, toCol)) {
            Debug.LogWarning($"Target position ({toRow}, {toCol}) is occupied");
            return false;
        }

        layoutComponent.MoveItem(cell, toRow, toCol);
        await layoutComponent.AnimateToLayoutPosition(cell);
        return true;
    }

    #endregion

    #region Utility Methods

    public Cell3DView GetCell(int row, int column) {
        return layoutComponent.GetCellAt(row, column);
    }

    public (int row, int column)? GetCellPosition(Cell3DView cell) {
        return layoutComponent.GetItemPosition(cell);
    }

    public bool IsCellOccupied(int row, int column) {
        return layoutComponent.IsCellOccupied(row, column);
    }

    public IEnumerable<(int row, int column, Cell3DView cell)> GetOccupiedCells() {
        return layoutComponent.GetOccupiedPositions();
    }

    public Cell3DView[] GetNeighborCells(int row, int column) {
        return layoutComponent.GetNeighborCells(row, column);
    }

    public int GetOccupiedCellCount() {
        return layoutComponent.OccupiedCells;
    }

    public bool IsBoardEmpty() {
        return GetOccupiedCellCount() == 0;
    }

    private async UniTask AnimateCellRemoval(Cell3DView cell) {
        var sequence = DOTween.Sequence()
            .Append(cell.transform.DOScale(Vector3.zero, 0.3f))
            .SetEase(Ease.InBack);

        await sequence.Play().ToUniTask();
    }

    public void ClearBoard() {
        layoutComponent.ClearItems();
        OnBoardCleared?.Invoke();
    }

    public bool IsAnimating => layoutComponent.IsAnimating;

    #endregion
}