using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class BoardView : MonoBehaviour {

    [Header("Pool")]
    [Inject] private IComponentPool<Cell3DView> cellPool;

    [Header("Components")]
    [SerializeField] Transform cellParent;
    [SerializeField] private CellLayoutComponent layoutComponent;

    [Header("Animation")]
    [SerializeField] private float removalDuration = 0.3f;
    [SerializeField] private Ease removalEase = Ease.InBack;

    [SerializeField] private float cellsOrganizeDuration = 0.3f;

    [Inject] private IVisualManager _visualManager;

    public event Action<Cell3DView> OnCellAdded;
    public event Action<Cell3DView> OnCellRemoved;
    public event Action<Cell3DView, Cell3DView> OnCellsSwapped;
    public event Action OnBoardCleared;

    private void Awake() {
        ValidateReferences();
    }

    private void ValidateReferences() {
        if (cellPool == null) {
            throw new UnassignedReferenceException(nameof(cellPool));
        }
        if (layoutComponent == null) {
            throw new UnassignedReferenceException(nameof(layoutComponent));
        }
    }

    public Cell3DView CreateCellView() {
        Cell3DView cell = cellPool.Get();
        cell.transform.SetParent(cellParent);
        cell.name = "Cell_Temp";
        cell.gameObject.SetActive(false); // 👈 Приховуємо до позиціонування

        return cell;
    }

    #region Public Synchronous API (для Presenter'ів)

    public void AddCellViewBatch(List<(Cell3DView cellView, int row, int column)> cellsData) {
        var batchTask = new AddCellBatchVisualTask(
            cellsData,
            layoutComponent,
            cellsOrganizeDuration
        );
        _visualManager.Push(batchTask);
        UpdateLayout();
    }


    public void RemoveCellsBatch(List<(int row, int column)> cellsData) {
        var batchTask = new RemoveCellBatchVisualTask(
            this,
            cellsData,
            layoutComponent,
            removalDuration
        );
        _visualManager.Push(batchTask);
        UpdateLayout();
    }

    public void UpdateLayout() {
        var layoutTask = new UpdateLayoutVisualTask(
            layoutComponent,
            cellsOrganizeDuration
        );
        _visualManager.Push(layoutTask);
    }

    public void AddCellView(Cell3DView cellView, int row, int column, bool doLayout = true) {
        var addCellTask = new AddCellVisualTask(
            cellView,
            row,
            column,
            layoutComponent,
            0.3f,
            OnCellAdded
        );
        _visualManager.Push(addCellTask);
    }

    public void RemoveCell(int row, int column) {
        var removeCellTask = new RemoveCellVisualTask(
            this,
            row,
            column,
            removalEase,
            layoutComponent,
            removalDuration,
            cellPool,
            OnCellRemoved
        );
        _visualManager.Push(removeCellTask);
    }

    public void SwapCells(int fromRow, int fromCol, int toRow, int toCol) {
        var swapTask = new SwapCellsVisualTask(
            layoutComponent,
            fromRow, fromCol,
            toRow, toCol,
            cellsOrganizeDuration,
            OnCellsSwapped
        );
        _visualManager.Push(swapTask);
    }

    public void MoveCell(int fromRow, int fromCol, int toRow, int toCol) {
        var moveTask = new MoveCellVisualTask(
            layoutComponent,
            fromRow, fromCol,
            toRow, toCol,
            cellsOrganizeDuration
        );
        _visualManager.Push(moveTask);
    }

    public void ClearBoard() {
        var clearTask = new ClearBoardVisualTask(
            this,
            cellPool,
            removalDuration,
            removalEase,
            OnBoardCleared
        );
        _visualManager.Push(clearTask);
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

    public bool IsAnimating => layoutComponent.IsAnimating;

    #endregion
}
public class AddCellBatchVisualTask : VisualTask {
    private readonly List<(Cell3DView cellView, int row, int column)> _cellsData;
    private readonly CellLayoutComponent _layout;
    private float _animationDuration;

    public AddCellBatchVisualTask(
        List<(Cell3DView cellView, int row, int column)> cellsData,
        CellLayoutComponent layout,
        float animationDuration = 0.3f) {
        _cellsData = cellsData;
        _layout = layout;
        _animationDuration = animationDuration;
    }

    public override async UniTask<bool> Execute() {
        if (_cellsData == null || _cellsData.Count == 0) {
            return false;
        }

        // 1. Додаємо всі клітинки до layout (без recalculate)
        foreach (var (cellView, row, column) in _cellsData) {
            if (_layout.IsCellOccupied(row, column)) {
                Debug.LogWarning($"Cell at ({row}, {column}) already exists, skipping");
                continue;
            }

            cellView.gameObject.SetActive(true);
            cellView.name = $"Cell_{row}_{column}";
            _layout.AddItem(cellView, row, column, recalculate: false);
        }
        await UniTask.WaitForSeconds(_animationDuration * TimeModifier);

        return true;
    }
}

public class RemoveCellBatchVisualTask : VisualTask {
    private BoardView _boardView;
    private List<(int row, int column)> cellsData;
    private CellLayoutComponent _layout;
    private float _animationDuration;
    public RemoveCellBatchVisualTask(BoardView boardView, List<(int row, int column)> cellsData, CellLayoutComponent layoutComponent, float removeDuration) {
        this.cellsData = cellsData;
        _layout = layoutComponent;
        _boardView = boardView;
    }

    public override async UniTask<bool> Execute() {
        var removalTasks = new List<UniTask>();

        for (int i = 0; i < cellsData.Count; i++) {
            int row = cellsData[i].row;
            int column = cellsData[i].column;

            var cellView = _boardView.GetCell(row, column);
            if (cellView == null) continue;

            _layout.RemoveAt(row, column, false);
            removalTasks.Add(AnimateCellRemoval(cellView).ContinueWith(() => cellView.Clear()));
        }

        await UniTask.WhenAll(removalTasks);

        return true;
    }


    private async UniTask AnimateCellRemoval(Cell3DView cell) {
        var sequence = DOTween.Sequence()
            .Append(cell.transform.DOScale(Vector3.zero, _animationDuration * TimeModifier))
            .SetEase(Ease.InBack);

        await sequence.Play().ToUniTask();
    }
}

public class AddCellVisualTask : VisualTask {
    private readonly Cell3DView _cellView;
    private readonly int _row;
    private readonly int _column;
    private readonly CellLayoutComponent _layout;
    private readonly float _animationDuration;
    private readonly Action<Cell3DView> _onComplete;

    public AddCellVisualTask(
        Cell3DView cellView,
        int row,
        int column,
        CellLayoutComponent layout,
        float animationDuration = 0.3f,
        System.Action<Cell3DView> onComplete = null
    ) {
        _cellView = cellView;
        _row = row;
        _column = column;
        _layout = layout;
        _animationDuration = animationDuration;
        _onComplete = onComplete;
    }

    public override async UniTask<bool> Execute() {
        // Перевірка чи позиція вільна
        if (_layout.IsCellOccupied(_row, _column)) {
            Debug.LogWarning($"Cell at ({_row}, {_column}) already exists");
            return false;
        }

        // Активуємо і налаштовуємо View
        _cellView.gameObject.SetActive(true);
        _cellView.name = $"Cell_{_row}_{_column}";

        // Додаємо до layout
        _layout.AddItem(_cellView, _row, _column);

        // Анімуємо з урахуванням TimeModifier
        float duration = _animationDuration * TimeModifier;
        await _layout.AnimateToLayoutPosition(_cellView, duration);

        // Викликаємо callback
        _onComplete?.Invoke(_cellView);

        return true;
    }
}

public class RemoveCellVisualTask : VisualTask {
    private readonly BoardView _boardView;
    private readonly int _row;
    private readonly int _column;

    private readonly CellLayoutComponent _layout;
    private readonly float _animationDuration;
    private readonly Action<Cell3DView> _onComplete;
    private IComponentPool<Cell3DView> _cellPool;
    private readonly Ease _removalEase;

    public RemoveCellVisualTask(BoardView boardView, int row, int column, Ease removalEase, CellLayoutComponent layout, float animationDuration, IComponentPool<Cell3DView> cellPool, Action<Cell3DView> onComplete = null) {
        _boardView = boardView;
        _row = row;
        _column = column;
        _removalEase = removalEase; 
        _layout = layout;
        _animationDuration = animationDuration;
        _cellPool = cellPool;
        _onComplete = onComplete;
    }

    public override async UniTask<bool> Execute() {
        var cell = _boardView.GetCell(_row, _column);
        if (cell == null) return false;

        await AnimateCellRemoval(cell);

        _layout.RemoveAt(_row, _column);
        _onComplete?.Invoke(cell);
        cell.Clear();
        _cellPool.Release(cell);
        return true;
    }

    private async UniTask AnimateCellRemoval(Cell3DView cell) {
        var sequence = DOTween.Sequence()
            .Append(cell.transform.DOScale(Vector3.zero, _animationDuration * TimeModifier))
            .SetEase(_removalEase);

        await sequence.Play().ToUniTask();
    }

}

public class SwapCellsVisualTask : VisualTask {
    private readonly CellLayoutComponent _layout;
    private readonly int _fromRow;
    private readonly int _fromCol;
    private readonly int _toRow;
    private readonly int _toCol;
    private readonly float _animationDuration;
    private readonly Action<Cell3DView, Cell3DView> _onComplete;

    public SwapCellsVisualTask(
        CellLayoutComponent layout,
        int fromRow, int fromCol,
        int toRow, int toCol,
        float animationDuration = 0.3f,
        Action<Cell3DView, Cell3DView> onComplete = null) {
        _layout = layout;
        _fromRow = fromRow;
        _fromCol = fromCol;
        _toRow = toRow;
        _toCol = toCol;
        _animationDuration = animationDuration;
        _onComplete = onComplete;
    }

    public override async UniTask<bool> Execute() {
        var cell1 = _layout.GetCellAt(_fromRow, _fromCol);
        var cell2 = _layout.GetCellAt(_toRow, _toCol);

        if (cell1 == null || cell2 == null) {
            Debug.LogWarning("Cannot swap - one or both cells are null");
            return false;
        }

        // Міняємо позиції в layout
        _layout.SwapItems(_fromRow, _fromCol, _toRow, _toCol);

        // Анімуємо обидві клітинки одночасно
        float duration = _animationDuration * TimeModifier;
        var task1 = _layout.AnimateToLayoutPosition(cell1, duration);
        var task2 = _layout.AnimateToLayoutPosition(cell2, duration);

        await UniTask.WhenAll(task1, task2);

        _onComplete?.Invoke(cell1, cell2);
        return true;
    }
}

public class MoveCellVisualTask : VisualTask {
    private readonly CellLayoutComponent _layout;
    private readonly int _fromRow;
    private readonly int _fromCol;
    private readonly int _toRow;
    private readonly int _toCol;
    private readonly float _animationDuration;

    public MoveCellVisualTask(
        CellLayoutComponent layout,
        int fromRow, int fromCol,
        int toRow, int toCol,
        float animationDuration = 0.3f) {
        _layout = layout;
        _fromRow = fromRow;
        _fromCol = fromCol;
        _toRow = toRow;
        _toCol = toCol;
        _animationDuration = animationDuration;
    }

    public override async UniTask<bool> Execute() {
        var cell = _layout.GetCellAt(_fromRow, _fromCol);
        if (cell == null) return false;

        if (_layout.IsCellOccupied(_toRow, _toCol)) {
            Debug.LogWarning($"Target position ({_toRow}, {_toCol}) is occupied");
            return false;
        }

        // Переміщуємо в layout
        _layout.MoveItem(cell, _toRow, _toCol);

        // Анімуємо переміщення
        float duration = _animationDuration * TimeModifier;
        await _layout.AnimateToLayoutPosition(cell, duration);

        return true;
    }
}

public class UpdateLayoutVisualTask : VisualTask {
    private readonly CellLayoutComponent _layout;
    private readonly float _animationDuration;

    public UpdateLayoutVisualTask(
        CellLayoutComponent layout,
        float animationDuration = 0.3f) {
        _layout = layout;
        _animationDuration = animationDuration;
    }

    public override async UniTask<bool> Execute() {
        _layout.RecalculateLayout();
        float duration = _animationDuration * TimeModifier;
        await _layout.AnimateAllToLayoutPositions(duration);
        return true;
    }
}

public class ClearBoardVisualTask : VisualTask {
    private readonly BoardView _boardView;
    private readonly IComponentPool<Cell3DView> _cellPool;
    private readonly float _animationDuration;
    private readonly Ease _removalEase;
    private readonly Action _onComplete;

    public ClearBoardVisualTask(
        BoardView boardView,
        IComponentPool<Cell3DView> cellPool,
        float animationDuration = 0.3f,
        Ease removalEase = Ease.InBack,
        Action onComplete = null) {
        _boardView = boardView;
        _cellPool = cellPool;
        _animationDuration = animationDuration;
        _removalEase = removalEase;
        _onComplete = onComplete;
    }

    public override async UniTask<bool> Execute() {
        var occupiedCells = _boardView.GetOccupiedCells().ToList();

        if (occupiedCells.Count == 0) {
            _onComplete?.Invoke();
            return true;
        }

        // Анімуємо видалення всіх клітинок одночасно
        var removalTasks = new List<UniTask>();

        foreach (var (row, column, cell) in occupiedCells) {
            var sequence = DOTween.Sequence()
                .Append(cell.transform.DOScale(Vector3.zero, _animationDuration * TimeModifier))
                .SetEase(_removalEase);

            removalTasks.Add(sequence.Play().ToUniTask());
        }

        await UniTask.WhenAll(removalTasks);

        // Очищаємо всі клітинки
        foreach (var (row, column, cell) in occupiedCells) {
            cell.Clear();
            _cellPool.Release(cell);
        }

        _boardView.ClearBoard();
        _onComplete?.Invoke();

        return true;
    }
}
