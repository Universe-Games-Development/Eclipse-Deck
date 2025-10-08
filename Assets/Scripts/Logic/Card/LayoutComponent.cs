using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

/// <summary>
/// Базовий компонент для всіх layouts з спільною анімаційною логікою
/// </summary>
public abstract class LayoutComponent<T> : MonoBehaviour where T : Component {

    [Header("Base Settings")]
    [SerializeField] protected Vector3 defaultItemSize = Vector3.one;

    [Header("Animation")]
    [SerializeField] protected float organizeDuration = 0.3f;
    [SerializeField] protected Ease organizeEase = Ease.OutQuad;

    protected readonly Dictionary<T, LayoutPoint> itemLayoutData = new();

    private CancellationTokenSource _layoutAnimationCts;
    private readonly Dictionary<T, CancellationTokenSource> _itemAnimationTokens = new();

    public Action<LayoutResult> OnLayoutCalculated;
    public Action<T, LayoutPoint> OnItemPositioned;
    public Action<T> OnItemAdded;
    public Action<T> OnItemRemoved;

    [Header("Debug")]
    [SerializeField] private bool doTestUpdate = false;
    [SerializeField] private float updateDelay = 1f;
    private float updateTimer;

    protected virtual void Awake() {
        _layoutAnimationCts = new CancellationTokenSource();
    }
   
    private void Update() {
        if (doTestUpdate) {
            updateTimer += Time.deltaTime;
            if (updateTimer > updateDelay) {
                RecalculateLayout();
                AnimateAllToLayoutPositions().Forget();
                updateTimer = 0f;
            }
        }
    }


    #region Abstract Core Methods

    public abstract void RecalculateLayout();
    public abstract IReadOnlyList<T> GetAllItems();
    public abstract bool Contains(T item);
    public abstract int GetItemCount();
    public abstract bool AddItem(T item, bool recalculate = true);
    public abstract bool RemoveItem(T item, bool recalculate = true);
    public virtual async UniTask AddItemAnimated(T item, float? duration = null) {
        AddItem(item);
        await AnimateToLayoutPosition(item, duration);
    }

    #endregion

    #region Layout Data Management (спільна для всіх)

    protected void UpdateLayoutData(LayoutResult result) {
        var points = result.Points;
        var items = GetAllItems();

        // Очищення застарілих даних
        var itemsSet = new HashSet<T>(items);
        var keysToRemove = itemLayoutData.Keys.Where(k => !itemsSet.Contains(k)).ToList();
        foreach (var key in keysToRemove) {
            itemLayoutData.Remove(key);
        }

        // Оновлення даних
        for (int i = 0; i < points.Length && i < items.Count; i++) {
            itemLayoutData[items[i]] = points[i];
            OnItemPositioned?.Invoke(items[i], points[i]);
        }

        OnLayoutCalculated?.Invoke(result);
    }

    protected void ClearLayoutData() {
        itemLayoutData.Clear();
    }

    #endregion

    #region Position Queries (спільні для всіх)

    public LayoutPoint? GetLayoutPoint(T item) {
        return itemLayoutData.TryGetValue(item, out var point) ? point : null;
    }

    public Vector3? GetPosition(T item) => GetLayoutPoint(item)?.Position;
    public Quaternion? GetRotation(T item) => GetLayoutPoint(item)?.Rotation;

    #endregion

    #region Animation (спільна логіка)
    public async UniTask AnimateAllToLayoutPositions(float? customDuration = null) {
        CancelAllAnimations();

        var tasks = GetAllItems()
            .Where(item => itemLayoutData.ContainsKey(item))
            .Select(item => AnimateToLayoutPosition(item, customDuration));

        try {
            await UniTask.WhenAll(tasks).AttachExternalCancellation(_layoutAnimationCts.Token);
        } catch (OperationCanceledException) { }
    }

    public async UniTask AnimateToLayoutPosition(T item, float? customDuration = null) {
        if (!itemLayoutData.TryGetValue(item, out var point)) {
            Debug.LogWarning($"No layout data for {item.name}");
            return;
        }

        await AnimateItem(item, point, customDuration ?? organizeDuration);
    }

    protected virtual async UniTask AnimateItem(T item, LayoutPoint point, float duration) {
        CancelItemAnimation(item);

        var itemCts = CancellationTokenSource.CreateLinkedTokenSource(_layoutAnimationCts.Token);
        _itemAnimationTokens[item] = itemCts;

        var sequence = DOTween.Sequence()
            .Join(item.transform.DOLocalMove(point.Position, duration))
            .Join(item.transform.DOLocalRotate(point.Rotation.eulerAngles, duration))
            .SetEase(organizeEase)
            .SetLink(item.gameObject);

        try {
            await sequence.Play().ToUniTask(TweenCancelBehaviour.Kill, itemCts.Token);
        } catch (OperationCanceledException) { } finally {
            if (_itemAnimationTokens.TryGetValue(item, out var cts) && cts == itemCts) {
                _itemAnimationTokens.Remove(item);
            }
            itemCts.Dispose();
        }
    }

    public void CancelAllAnimations() {
        _layoutAnimationCts?.Cancel();
        _layoutAnimationCts?.Dispose();
        _layoutAnimationCts = new CancellationTokenSource();

        foreach (var cts in _itemAnimationTokens.Values) {
            cts.Cancel();
            cts.Dispose();
        }
        _itemAnimationTokens.Clear();
    }

    public void CancelItemAnimation(T item) {
        if (_itemAnimationTokens.TryGetValue(item, out var cts)) {
            cts.Cancel();
            cts.Dispose();
            _itemAnimationTokens.Remove(item);
        }
    }

    #endregion

    #region Virtual Helpers

    protected virtual Vector3 GetItemSize(T item) => defaultItemSize;
    protected virtual string GetItemId(T item) => item.name;

    #endregion

    #region Properties

    public bool IsAnimating => _itemAnimationTokens.Count > 0;
    public IReadOnlyDictionary<T, LayoutPoint> LayoutData => itemLayoutData;

    #endregion

    protected virtual void OnDestroy() {
        CancelAllAnimations();
        _layoutAnimationCts?.Dispose();
    }
}


/// <summary>
/// Layout для послідовного розміщення елементів
/// </summary>
public abstract class LinearLayoutComponent<T> : LayoutComponent<T> where T : Component {
    [Header("Linear Layout")]
    [SerializeField] protected LinearLayoutSettings layoutSettings;

    protected ILinearLayout layout;
    protected readonly List<T> orderedItems = new();

    protected override void Awake() {
        base.Awake();
        if (layoutSettings == null) {
            throw new UnassignedReferenceException("layoutSettings is not assigned!");
        }
        layout = new Linear3DLayout(layoutSettings);
    }

    #region Item Management

    public override bool AddItem(T item, bool recalculate = true) {
        if (item == null || orderedItems.Contains(item)) return false;

        orderedItems.Add(item);
        OnItemAdded?.Invoke(item);

        if (recalculate) RecalculateLayout();

        return true;
    }

    public override bool RemoveItem(T item, bool recalculate = true) {
        if (item == null || !orderedItems.Remove(item)) return false;

        CancelItemAnimation(item);
        itemLayoutData.Remove(item);
        OnItemRemoved?.Invoke(item);

        if (recalculate) RecalculateLayout();
        return true;
    }

    public virtual void ClearItems() {
        CancelAllAnimations();
        orderedItems.Clear();
        ClearLayoutData();
    }

    public void ReorderItem(T item, int newIndex, bool recalculate = true) {
        if (!orderedItems.Remove(item)) return;

        newIndex = Mathf.Clamp(newIndex, 0, orderedItems.Count);
        orderedItems.Insert(newIndex, item);

        if (recalculate) RecalculateLayout();
    }

    public int GetItemIndex(T item) => orderedItems.IndexOf(item);

    #endregion

    #region Override Abstract

    public override IReadOnlyList<T> GetAllItems() => orderedItems.AsReadOnly();
    public override bool Contains(T item) => orderedItems.Contains(item);
    public override int GetItemCount() => orderedItems.Count;

    public override void RecalculateLayout() {
        if (orderedItems.Count == 0) {
            OnLayoutCalculated?.Invoke(LayoutResult.Empty);
            return;
        }

        var items = orderedItems.Select(item =>
            new ItemLayoutInfo(GetItemId(item), GetItemSize(item))
        ).ToArray();

        var result = layout.Calculate(items);
        UpdateLayoutData(result);
    }

    #endregion
}


/// <summary>
/// Layout для сіткового розміщення з автоматичним керуванням
/// Містить всю бізнес-логіку роботи з сіткою
/// </summary>
public abstract class GridLayoutComponent<T> : LayoutComponent<T> where T : Component {
    [Header("Grid Layout")]
    [SerializeField] protected GridLayoutSettings gridSettings;
    [SerializeField] protected int initialRows = 3;
    [SerializeField] protected int initialColumns = 3;

    [Header("Auto Management")]
    [SerializeField] protected bool autoExpand = true;
    [SerializeField] protected bool autoShrink = false;
    [SerializeField] protected int maxRows = 20;
    [SerializeField] protected int maxColumns = 20;

    protected IGridLayout layout;
    protected Grid2D<T> grid;

    public event Action<int, int> OnGridResized;

    protected override void Awake() {
        base.Awake();
        if (gridSettings == null) {
            throw new UnassignedReferenceException("gridSettings is not assigned!");
        }
        layout = new Grid3DLayout(gridSettings);
        grid = new Grid2D<T>(initialRows, initialColumns);
    }

    #region Core Item Management

    public virtual void AddItem(T item, int row, int col, bool recalculate = true) {
        if (item == null) return;

        // Автоматичне розширення якщо потрібно
        if (!grid.IsValid(row, col)) {
            if (autoExpand) {
                ExpandToFit(row, col);
            } else {
                Debug.LogWarning($"Position ({row}, {col}) out of bounds");
                return;
            }
        }

        // Перевірка зайнятості
        var existing = grid.Get(row, col);
        if (existing != null && existing != item) {
            Debug.LogWarning($"Position ({row}, {col}) occupied by {existing.name}");
            return;
        }

        // Видалити з попередньої позиції
        var oldPos = grid.FindPosition(item);
        if (oldPos.HasValue) {
            grid.Set(oldPos.Value.row, oldPos.Value.col, null);
        }

        grid.Set(row, col, item);
        OnItemAdded?.Invoke(item);

        if (recalculate) RecalculateLayout();
    }

    public override bool AddItem(T item, bool recalculate = true) {
        var freePos = FindFirstFreePosition();

        if (!freePos.HasValue) {
            if (!autoExpand || !TryExpandGrid()) {
                Debug.LogWarning("No free positions and cannot expand");
                return false;
            }
            freePos = FindFirstFreePosition();
        }

        if (freePos.HasValue) {
            AddItem(item, freePos.Value.row, freePos.Value.col, recalculate);
            return true;
        }

        return false;
    }

    public override bool RemoveItem(T item, bool recalculate = true) {
        var pos = grid.FindPosition(item);
        if (!pos.HasValue) return false;

        CancelItemAnimation(item);
        grid.Set(pos.Value.row, pos.Value.col, null);
        itemLayoutData.Remove(item);
        OnItemRemoved?.Invoke(item);

        if (autoShrink) {
            ShrinkToFit(recalculate: false);
        }

        if (recalculate) RecalculateLayout();
        return true;
    }

    public virtual void RemoveAt(int row, int col, bool recalculate = true) {
        var item = grid.Get(row, col);
        if (item != null) {
            RemoveItem(item, recalculate);
        }
    }

    public virtual void MoveItem(T item, int newRow, int newCol, bool recalculate = true) {
        var currentPos = grid.FindPosition(item);
        if (!currentPos.HasValue) return;

        // Розширення якщо потрібно
        if (!grid.IsValid(newRow, newCol)) {
            if (!autoExpand || !ExpandToFit(newRow, newCol)) {
                Debug.LogWarning($"Cannot move to ({newRow}, {newCol})");
                return;
            }
        }

        var target = grid.Get(newRow, newCol);
        if (target != null && target != item) {
            Debug.LogWarning("Target position occupied");
            return;
        }

        grid.Set(currentPos.Value.row, currentPos.Value.col, null);
        grid.Set(newRow, newCol, item);

        if (recalculate) RecalculateLayout();
    }

    public void SwapItems(int row1, int col1, int row2, int col2, bool recalculate = true) {
        var item1 = grid.Get(row1, col1);
        var item2 = grid.Get(row2, col2);

        grid.Set(row1, col1, item2);
        grid.Set(row2, col2, item1);

        if (recalculate) RecalculateLayout();
    }

    public virtual void ClearItems() {
        CancelAllAnimations();
        grid.Clear();
        ClearLayoutData();
    }

    #endregion

    #region Grid Management

    private bool ExpandToFit(int targetRow, int targetCol) {
        int newRows = Mathf.Max(grid.RowCount, targetRow + 1);
        int newCols = Mathf.Max(grid.ColumnCount, targetCol + 1);

        if (newRows > maxRows || newCols > maxColumns) {
            Debug.LogWarning($"Cannot expand beyond max size ({maxRows}x{maxColumns})");
            return false;
        }

        ResizeGrid(newRows, newCols, recalculate: false);
        return true;
    }

    private bool TryExpandGrid() {
        // Розширюємо по одному рядку/стовпцю
        int newRows = Mathf.Min(grid.RowCount + 1, maxRows);
        int newCols = Mathf.Min(grid.ColumnCount + 1, maxColumns);

        if (newRows == grid.RowCount && newCols == grid.ColumnCount) {
            return false; // Досягнуто максимуму
        }

        ResizeGrid(newRows, newCols, recalculate: false);
        return true;
    }

    public void ShrinkToFit(bool recalculate = true) {
        if (grid.CountOccupied() == 0) {
            ResizeGrid(1, 1, recalculate);
            return;
        }

        int maxRow = 0, maxCol = 0;
        foreach (var (row, col, _) in grid.EnumerateAll()) {
            maxRow = Mathf.Max(maxRow, row);
            maxCol = Mathf.Max(maxCol, col);
        }

        ResizeGrid(maxRow + 1, maxCol + 1, recalculate);
    }

    public void ResizeGrid(int newRows, int newCols, bool recalculate = true) {
        if (newRows == grid.RowCount && newCols == grid.ColumnCount) return;

        grid.Resize(newRows, newCols);
        OnGridResized?.Invoke(newRows, newCols);

        if (recalculate) RecalculateLayout();
    }

    #endregion

    #region Queries

    public T GetItemAt(int row, int col) => grid.Get(row, col);

    public (int row, int col)? GetItemPosition(T item) => grid.FindPosition(item);

    public IEnumerable<(int row, int col, T item)> GetOccupiedPositions() {
        return grid.EnumerateAll();
    }

    public (int row, int col)? FindFirstFreePosition() {
        for (int r = 0; r < grid.RowCount; r++) {
            for (int c = 0; c < grid.ColumnCount; c++) {
                if (grid.Get(r, c) == null) {
                    return (r, c);
                }
            }
        }
        return null;
    }

    #endregion

    #region Override Abstract

    public override IReadOnlyList<T> GetAllItems() {
        var items = new List<T>(grid.RowCount * grid.ColumnCount);
        foreach (var (_, _, item) in grid.EnumerateAll()) {
            if (item != null) items.Add(item);
        }
        return items.AsReadOnly();
    }

    public override bool Contains(T item) => grid.Contains(item);
    public override int GetItemCount() => grid.CountOccupied();

    public override void RecalculateLayout() {
        if (GetItemCount() == 0) {
            OnLayoutCalculated?.Invoke(LayoutResult.Empty);
            return;
        }

        var gridData = BuildGridData();
        var result = layout.Calculate(gridData);
        UpdateLayoutData(result);
    }

    private Grid<ItemLayoutInfo> BuildGridData() {
        var rows = new List<GridRow<ItemLayoutInfo>>();

        for (int r = 0; r < grid.RowCount; r++) {
            var cells = new List<ItemLayoutInfo>();

            for (int c = 0; c < grid.ColumnCount; c++) {
                var item = grid.Get(r, c);
                if (item != null) {
                    cells.Add(new ItemLayoutInfo(GetItemId(item), GetItemSize(item)));
                }
            }

            if (cells.Count > 0) {
                rows.Add(new GridRow<ItemLayoutInfo>(cells.ToArray()));
            }
        }

        return new Grid<ItemLayoutInfo>(rows.ToArray());
    }

    #endregion

    #region Properties

    public int GridRows => grid.RowCount;
    public int GridColumns => grid.ColumnCount;
    public int TotalCells => grid.RowCount * grid.ColumnCount;
    public int OccupiedCells => grid.CountOccupied();
    public int FreeCells => TotalCells - OccupiedCells;

    #endregion
}
