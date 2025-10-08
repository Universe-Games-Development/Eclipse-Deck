using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

#region Animation Modes

/// <summary>
/// Режими анімації для layout компонентів
/// </summary>
public enum LayoutAnimationMode {
    /// <summary>
    /// Анімація кожного елемента окремо (повільніше, але точніше)
    /// </summary>
    Individual,

    /// <summary>
    /// Анімація через батьківські Transform (швидше, оптимізовано)
    /// Елементи рухаються разом з батьківським контейнером
    /// </summary>
    Hierarchical
}

#endregion

#region Base Layout Component

/// <summary>
/// Базовий компонент для всіх layouts з спільною анімаційною логікою
/// </summary>
public abstract class LayoutComponent<T> : MonoBehaviour where T : Component {

    [Header("Base Settings")]
    [SerializeField] protected Vector3 defaultItemSize = Vector3.one;

    [Header("Animation")]
    [SerializeField] protected LayoutAnimationMode animationMode = LayoutAnimationMode.Individual;
    [SerializeField] protected float organizeDuration = 0.3f;
    [SerializeField] protected Ease organizeEase = Ease.OutQuad;

    [Header("Containers")]
    [SerializeField] protected Transform itemsContainer;

    protected readonly Dictionary<T, LayoutPoint> itemLayoutData = new();

    protected CancellationTokenSource _layoutAnimationCts;
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
        EnsureItemsContainer();
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

    protected void EnsureItemsContainer() {
        if (itemsContainer == null) {
            var containerObj = new GameObject("ItemsContainer");
            itemsContainer = containerObj.transform;
            itemsContainer.SetParent(transform, false);
            itemsContainer.localPosition = Vector3.zero;
            itemsContainer.localRotation = Quaternion.identity;
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

    #region Layout Data Management

    protected void UpdateLayoutData(LayoutResult result) {
        var points = result.Points;
        var items = GetAllItems();

        var itemsSet = new HashSet<T>(items);
        var keysToRemove = itemLayoutData.Keys.Where(k => !itemsSet.Contains(k)).ToList();
        foreach (var key in keysToRemove) {
            itemLayoutData.Remove(key);
        }

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

    #region Position Queries

    public LayoutPoint? GetLayoutPoint(T item) {
        return itemLayoutData.TryGetValue(item, out var point) ? point : null;
    }

    public Vector3? GetPosition(T item) => GetLayoutPoint(item)?.Position;
    public Quaternion? GetRotation(T item) => GetLayoutPoint(item)?.Rotation;

    #endregion

    #region Animation - Abstract для перевизначення

    public async UniTask AnimateAllToLayoutPositions(float? customDuration = null) {
        CancelAllAnimations();

        if (animationMode == LayoutAnimationMode.Hierarchical) {
            await AnimateHierarchical(customDuration);
        } else {
            await AnimateIndividual(customDuration);
        }
    }

    private async UniTask AnimateIndividual(float? customDuration) {
        var tasks = GetAllItems()
            .Where(item => itemLayoutData.ContainsKey(item))
            .Select(item => AnimateToLayoutPosition(item, customDuration));

        try {
            await UniTask.WhenAll(tasks).AttachExternalCancellation(_layoutAnimationCts.Token);
        } catch (OperationCanceledException) { }
    }

    protected abstract UniTask AnimateHierarchical(float? customDuration);

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
    public Transform ItemsContainer => itemsContainer;
    public LayoutAnimationMode AnimationMode => animationMode;

    #endregion

    protected virtual void OnDestroy() {
        CancelAllAnimations();
        _layoutAnimationCts?.Dispose();
    }
}

#endregion

#region Linear Layout Component

/// <summary>
/// Layout для послідовного розміщення елементів
/// У Hierarchical режимі анімує itemsContainer замість окремих елементів
/// </summary>
public abstract class LinearLayoutComponent<T> : LayoutComponent<T> where T : Component {
    [Header("Linear Layout")]
    [SerializeField] protected LinearLayoutSettings layoutSettings;

    protected ILinearLayout layout;
    protected readonly List<T> orderedItems = new();

    // Зберігаємо початкові локальні позиції елементів відносно контейнера
    private readonly Dictionary<T, Vector3> _itemLocalPositions = new();
    private readonly Dictionary<T, Quaternion> _itemLocalRotations = new();

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

        // Прив'язуємо до контейнера
        item.transform.SetParent(itemsContainer, true);

        OnItemAdded?.Invoke(item);

        if (recalculate) RecalculateLayout();

        return true;
    }

    public override bool RemoveItem(T item, bool recalculate = true) {
        if (item == null || !orderedItems.Remove(item)) return false;

        CancelItemAnimation(item);
        itemLayoutData.Remove(item);
        _itemLocalPositions.Remove(item);
        _itemLocalRotations.Remove(item);

        OnItemRemoved?.Invoke(item);

        if (recalculate) RecalculateLayout();
        return true;
    }

    public virtual void ClearItems() {
        CancelAllAnimations();
        orderedItems.Clear();
        _itemLocalPositions.Clear();
        _itemLocalRotations.Clear();
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

        // Оновлюємо локальні позиції для Hierarchical режиму
        if (animationMode == LayoutAnimationMode.Hierarchical) {
            UpdateItemLocalPositions();
        }
    }

    #endregion

    #region Hierarchical Animation

    protected override async UniTask AnimateHierarchical(float? customDuration) {
        if (orderedItems.Count == 0) return;

        // Розставляємо елементи у локальних координатах контейнера
        SetItemsToLocalPositions();

        // Анімуємо контейнер (завжди в (0,0,0) у нашому випадку)
        // Оскільки елементи вже в правильних локальних позиціях
        var duration = customDuration ?? organizeDuration;

        var sequence = DOTween.Sequence()
            .Join(itemsContainer.DOLocalMove(Vector3.zero, duration))
            .Join(itemsContainer.DOLocalRotate(Vector3.zero, duration))
            .SetEase(organizeEase)
            .SetLink(gameObject);

        try {
            await sequence.Play().ToUniTask(TweenCancelBehaviour.Kill, _layoutAnimationCts.Token);
        } catch (OperationCanceledException) { }
    }

    private void UpdateItemLocalPositions() {
        _itemLocalPositions.Clear();
        _itemLocalRotations.Clear();

        foreach (var item in orderedItems) {
            if (itemLayoutData.TryGetValue(item, out var point)) {
                _itemLocalPositions[item] = point.Position;
                _itemLocalRotations[item] = point.Rotation;
            }
        }
    }

    private void SetItemsToLocalPositions() {
        foreach (var item in orderedItems) {
            if (_itemLocalPositions.TryGetValue(item, out var pos) &&
                _itemLocalRotations.TryGetValue(item, out var rot)) {
                item.transform.localPosition = pos;
                item.transform.localRotation = rot;
            }
        }
    }

    #endregion
}

#endregion


/// <summary>
/// Layout для сіткового розміщення з автоматичним керуванням
/// У Hierarchical режимі створює Transform контейнери для кожного ряду
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

    // Для Hierarchical режиму - контейнери рядів
    private readonly Dictionary<int, Transform> _rowContainers = new();
    private readonly Dictionary<int, LayoutPoint> _rowLayoutPoints = new();

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

        if (!grid.IsValid(row, col)) {
            if (autoExpand) {
                ExpandToFit(row, col);
            } else {
                Debug.LogWarning($"Position ({row}, {col}) out of bounds");
                return;
            }
        }

        var existing = grid.Get(row, col);
        if (existing != null && existing != item) {
            Debug.LogWarning($"Position ({row}, {col}) occupied by {existing.name}");
            return;
        }

        var oldPos = grid.FindPosition(item);
        if (oldPos.HasValue) {
            grid.Set(oldPos.Value.row, oldPos.Value.col, null);
        }

        grid.Set(row, col, item);

        // Прив'язуємо до правильного контейнера
        if (animationMode == LayoutAnimationMode.Hierarchical) {
            var rowContainer = GetOrCreateRowContainer(row);
            item.transform.SetParent(rowContainer, true);
        } else {
            item.transform.SetParent(itemsContainer, true);
        }

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

        // Оновлюємо батьківський контейнер
        if (animationMode == LayoutAnimationMode.Hierarchical && currentPos.Value.row != newRow) {
            var newRowContainer = GetOrCreateRowContainer(newRow);
            item.transform.SetParent(newRowContainer, true);
        }

        if (recalculate) RecalculateLayout();
    }

    public void SwapItems(int row1, int col1, int row2, int col2, bool recalculate = true) {
        var item1 = grid.Get(row1, col1);
        var item2 = grid.Get(row2, col2);

        grid.Set(row1, col1, item2);
        grid.Set(row2, col2, item1);

        // Оновлюємо батьківські контейнери
        if (animationMode == LayoutAnimationMode.Hierarchical && row1 != row2) {
            if (item1 != null) {
                var container1 = GetOrCreateRowContainer(row2);
                item1.transform.SetParent(container1, true);
            }
            if (item2 != null) {
                var container2 = GetOrCreateRowContainer(row1);
                item2.transform.SetParent(container2, true);
            }
        }

        if (recalculate) RecalculateLayout();
    }

    public virtual void ClearItems() {
        CancelAllAnimations();
        grid.Clear();
        ClearLayoutData();
        ClearRowContainers();
    }

    #endregion

    #region Row Container Management

    private Transform GetOrCreateRowContainer(int rowIndex) {
        if (_rowContainers.TryGetValue(rowIndex, out var container) && container != null) {
            return container;
        }

        var rowObj = new GameObject($"Row_{rowIndex}");
        container = rowObj.transform;
        container.SetParent(itemsContainer, false);
        container.localPosition = Vector3.zero;
        container.localRotation = Quaternion.identity;

        _rowContainers[rowIndex] = container;
        return container;
    }

    private void ClearRowContainers() {
        foreach (var container in _rowContainers.Values) {
            if (container != null) {
                Destroy(container.gameObject);
            }
        }
        _rowContainers.Clear();
        _rowLayoutPoints.Clear();
    }

    private void CleanupEmptyRowContainers() {
        var rowsToRemove = new List<int>();

        foreach (var kvp in _rowContainers) {
            int rowIndex = kvp.Key;
            bool hasItems = false;

            for (int col = 0; col < grid.ColumnCount; col++) {
                if (grid.Get(rowIndex, col) != null) {
                    hasItems = true;
                    break;
                }
            }

            if (!hasItems) {
                if (kvp.Value != null) {
                    Destroy(kvp.Value.gameObject);
                }
                rowsToRemove.Add(rowIndex);
            }
        }

        foreach (var row in rowsToRemove) {
            _rowContainers.Remove(row);
            _rowLayoutPoints.Remove(row);
        }
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
        int newRows = Mathf.Min(grid.RowCount + 1, maxRows);
        int newCols = Mathf.Min(grid.ColumnCount + 1, maxColumns);

        if (newRows == grid.RowCount && newCols == grid.ColumnCount) {
            return false;
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

        if (animationMode == LayoutAnimationMode.Hierarchical) {
            CleanupEmptyRowContainers();
        }

        OnGridResized?.Invoke(newRows, newCols);

        if (recalculate) RecalculateLayout();
    }

    #endregion

    #region Hierarchical Animation

    protected override async UniTask AnimateHierarchical(float? customDuration) {
        if (GetItemCount() == 0) return;

        // Крок 1: Розставити елементи у локальних координатах їх row контейнерів
        SetItemsToRowLocalPositions();

        // Крок 2: Анімувати row контейнери
        var duration = customDuration ?? organizeDuration;
        var tasks = new List<UniTask>();

        foreach (var kvp in _rowContainers) {
            int rowIndex = kvp.Key;
            var container = kvp.Value;

            if (container != null && _rowLayoutPoints.TryGetValue(rowIndex, out var point)) {
                var sequence = DOTween.Sequence()
                    .Join(container.DOLocalMove(point.Position, duration))
                    .Join(container.DOLocalRotate(point.Rotation.eulerAngles, duration))
                    .SetEase(organizeEase)
                    .SetLink(container.gameObject);

                tasks.Add(sequence.Play().ToUniTask(TweenCancelBehaviour.Kill, _layoutAnimationCts.Token));
            }
        }

        try {
            await UniTask.WhenAll(tasks);
        } catch (OperationCanceledException) { }
    }

    private void SetItemsToRowLocalPositions() {
        // Обчислюємо позиції рядів та локальні позиції елементів
        _rowLayoutPoints.Clear();

        foreach (var (row, col, item) in grid.EnumerateAll()) {
            if (item == null) continue;

            if (!itemLayoutData.TryGetValue(item, out var itemPoint)) continue;

            // Отримуємо або обчислюємо позицію ряду
            if (!_rowLayoutPoints.TryGetValue(row, out var rowPoint)) {
                rowPoint = CalculateRowLayoutPoint(row);
                _rowLayoutPoints[row] = rowPoint;
            }

            // Обчислюємо локальну позицію елемента відносно ряду
            Vector3 localPos = itemPoint.Position - rowPoint.Position;
            Quaternion localRot = Quaternion.Inverse(rowPoint.Rotation) * itemPoint.Rotation;

            item.transform.localPosition = localPos;
            item.transform.localRotation = localRot;
        }
    }

    private LayoutPoint CalculateRowLayoutPoint(int rowIndex) {
        // Знаходимо середню позицію всіх елементів у ряді
        Vector3 avgPosition = Vector3.zero;
        Quaternion avgRotation = Quaternion.identity;
        int count = 0;

        for (int col = 0; col < grid.ColumnCount; col++) {
            var item = grid.Get(rowIndex, col);
            if (item != null && itemLayoutData.TryGetValue(item, out var point)) {
                avgPosition += point.Position;
                // Для простоти беремо ротацію першого елемента
                if (count == 0) avgRotation = point.Rotation;
                count++;
            }
        }

        if (count > 0) {
            avgPosition /= count;
        }

        return new LayoutPoint(avgPosition, avgRotation, $"Row_{rowIndex}", rowIndex, 0, Vector3.zero);
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
    public IReadOnlyDictionary<int, Transform> RowContainers => _rowContainers;

    #endregion

    protected override void OnDestroy() {
        base.OnDestroy();
        ClearRowContainers();
    }
}
