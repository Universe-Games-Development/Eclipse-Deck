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

    // Події дошки
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
    /// Створити всю дошку
    /// </summary>
    public async UniTask CreateBoard() {
        // Очищуємо попередню дошку
        ClearBoard();

        // Встановлюємо розмір сітки
        layoutComponent.ResizeGrid(initialRows, initialColumns, recalculate: false);

        // Створюємо клітинки
        for (int row = 0; row < initialRows; row++) {
            for (int col = 0; col < initialColumns; col++) {
                CreateCell(row, col, recalculate: false);

                if (animateCellCreation && cellCreationDelay > 0) {
                    await UniTask.Delay(TimeSpan.FromSeconds(cellCreationDelay));
                }
            }
        }

        // Один раз перераховуємо layout після створення всіх клітинок
        layoutComponent.RecalculateLayout();

        // Анімуємо всі клітинки
        if (animateCellCreation) {
            await layoutComponent.AnimateAllToLayoutPositions();
        }

        OnBoardCreated?.Invoke();
        Debug.Log($"Board created: {initialRows}x{initialColumns}");
    }

    /// <summary>
    /// Очистити всю дошку
    /// </summary>
    public void ClearBoard() {
        layoutComponent.ClearItems();
        OnBoardCleared?.Invoke();
    }

    /// <summary>
    /// Змінити розмір дошки
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
    /// Створити одну клітинку
    /// </summary>
    public Cell3DView CreateCell(int row, int column, bool recalculate = true) {

        // Перевірити чи позиція вже зайнята
        if (layoutComponent.IsCellOccupied(row, column)) {
            Debug.LogWarning($"Cell at ({row}, {column}) already exists");
            return layoutComponent.GetCellAt(row, column);
        }

        // Створити клітинку через factory
        Cell3DView cell = cellPool.Get();
        cell.transform.SetParent(cellParent);
        cell.name = $"Cell_{row}_{column}";

        // Додати в layout
        layoutComponent.AddItem(cell, row, column, recalculate);

        // Анімувати до позиції
        if (recalculate) {
            layoutComponent.RecalculateLayout();
            layoutComponent.AnimateToLayoutPosition(cell).Forget();
        }

        OnCellCreated?.Invoke(cell);
        return cell;
    }

    /// <summary>
    /// Видалити клітинку
    /// </summary>
    public void RemoveCell(int row, int column, bool animate = true) {
        var cell = layoutComponent.GetCellAt(row, column);
        if (cell == null) return;

        if (animate) {
            // Анімація видалення перед фактичним видаленням
            AnimateCellRemoval(cell).ContinueWith(() => {
                OnCellRemove(cell, row, column);
            }).Forget();
        } else {
            OnCellRemove(cell, row, column);
        }
    }

    private void OnCellRemove(Cell3DView cell, int row, int column) {
        layoutComponent.RemoveAt(row, column);
        OnCellRemoved?.Invoke(cell);
        cell.Clear();
        cellPool.Release(cell);
    }

    /// <summary>
    /// Отримати клітинку за позицією
    /// </summary>
    public Cell3DView GetCell(int row, int column) {
        return layoutComponent.GetCellAt(row, column);
    }

    /// <summary>
    /// Отримати позицію клітинки
    /// </summary>
    public (int row, int column)? GetCellPosition(Cell3DView cell) {
        return layoutComponent.GetItemPosition(cell);
    }

    /// <summary>
    /// Перевірити чи позиція зайнята
    /// </summary>
    public bool IsCellOccupied(int row, int column) {
        return layoutComponent.IsCellOccupied(row, column);
    }

    /// <summary>
    /// Отримати всі зайняті позиції
    /// </summary>
    public IEnumerable<(int row, int column, Cell3DView cell)> GetOccupiedCells() {
        return layoutComponent.GetOccupiedPositions();
    }

    #endregion

    #region Cell Interactions

    /// <summary>
    /// Поміняти місцями дві клітинки
    /// </summary>
    public async UniTask SwapCells(int fromRow, int fromCol, int toRow, int toCol) {
        var cell1 = GetCell(fromRow, fromCol);
        var cell2 = GetCell(toRow, toCol);

        if (cell1 == null || cell2 == null) {
            Debug.LogWarning("Cannot swap - one or both cells are null");
            return;
        }

        // Анімуємо обмін одночасно
        var task1 = layoutComponent.AnimateToLayoutPosition(cell1);
        var task2 = layoutComponent.AnimateToLayoutPosition(cell2);

        await UniTask.WhenAll(task1, task2);

        OnCellsSwapped?.Invoke(cell1, cell2);
    }

    /// <summary>
    /// Перемістити клітинку
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
    /// Отримати сусідні клітинки
    /// </summary>
    public Cell3DView[] GetNeighborCells(int row, int column) {
        return layoutComponent.GetNeighborCells(row, column);
    }

    #endregion

    #region Board State & Validation
    /// <summary>
    /// Отримати кількість зайнятих клітинок
    /// </summary>
    public int GetOccupiedCellCount() {
        return layoutComponent.OccupiedCells;
    }


    /// <summary>
    /// Перевірити чи дошка порожня
    /// </summary>
    public bool IsBoardEmpty() {
        return GetOccupiedCellCount() == 0;
    }

    #endregion

    #region Animation Helpers

    /// <summary>
    /// Анімація видалення клітинки
    /// </summary>
    private async UniTask AnimateCellRemoval(Cell3DView cell) {
        // Можна додати ефект зникнення, масштабування тощо
        var sequence = DOTween.Sequence()
            .Append(cell.transform.DOScale(Vector3.zero, 0.3f))
            .SetEase(Ease.InBack);

        await sequence.Play().ToUniTask();
    }

    public async UniTask UpdateLayout() {
        layoutComponent.RecalculateLayout();
        await layoutComponent.AnimateAllToLayoutPositions();
    }

    #endregion

    #region Properties
    public bool IsAnimating => layoutComponent.IsAnimating;

    #endregion
}
