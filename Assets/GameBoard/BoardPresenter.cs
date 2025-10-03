using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using static UnityEngine.Rendering.DebugUI.Table;

public class BoardPresenter : UnitPresenter, IDisposable {
    public BoardView BoardView;
    public Board Board;

    private Dictionary<Cell, CellPresenter> cellPresenters = new();

    // Кеш для швидкого доступу до ячейок по індексах
    private Dictionary<(int row, int col), CellPresenter> cellPresentersByIndex = new();

    [Inject] IPresenterFactory presenterFactory;
    private ILayout3DHandler layout;

    public BoardPresenter(Board board, BoardView boardView) : base(board, boardView) {
        Board = board;
        BoardView = boardView;
        SubscribeToEvents();
        layout = new Grid3DLayout(boardView.layoutSettings);

        BoardView.OnUpdateRequest += UpdateLayout;
    }

    private void UpdateLayout() {
        RecalculateLayout();
    }

    private void SubscribeToEvents() {
        Board.ColumnAdded += OnColumnAdded;
        Board.ColumnRemoved += OnColumnRemoved;
    }

    private void UnsubscribeFromEvents() {
        if (Board != null) {
            Board.ColumnAdded -= OnColumnAdded;
            Board.ColumnRemoved -= OnColumnRemoved;
        }
    }

    public void CreateBoard() {
        for (int rowIndex = 0; rowIndex < Board.RowCount; rowIndex++) {
            var row = Board.GetRow(rowIndex);
            for (int cellIndex = 0; cellIndex < row.CellCount; cellIndex++) {
                var cell = row.GetCell(cellIndex);
                if (cell == null) continue;
                CreateCellPresenter(cell, rowIndex, cellIndex);
            }
        }

        RecalculateLayout();
    }

    private CellPresenter CreateCellPresenter(Cell cell, int rowIndex, int colIndex) {
        var cellView = BoardView.CreateCell(cell.RowIndex, cell.ColumnIndex);
        var cellPresenter = presenterFactory.CreatePresenter<CellPresenter>(cell, cellView);

        cellPresenter.OnDesiredSizeChanged += OnCellDesiredSizeChanged;

        cellPresenters[cell] = cellPresenter;
        cellPresentersByIndex[(rowIndex, colIndex)] = cellPresenter;

        return cellPresenter;
    }

    /// <summary>
    /// Коли ячейка повідомляє про свій бажаний розмір
    /// </summary>
    private void OnCellDesiredSizeChanged(CellPresenter cellPresenter, Vector3 desiredSize) {
        RecalculateLayout();
    }

    /// <summary>
    /// ГОЛОВНИЙ МЕТОД: Координує весь процес Layout
    /// </summary>
    public void RecalculateLayout() {
        // 1. Збираємо бажані розміри всіх ячейок
        var cellSizes = CollectCellDesiredSizes();

        // 2. Обчислюємо фактичні розміри (максимуми по колонках/рядах)
        var (columnWidths, rowHeights) = CalculateGridDimensions(cellSizes);

        // 3. Застосовуємо фактичні розміри до ячейок
        ApplySizesToCells(columnWidths, rowHeights);

        // 4. Просимо BoardView обчислити позиції на основі розмірів
        ApplyPositionsToClls(columnWidths, rowHeights);
    }

    /// <summary>
    /// Збирає бажані розміри всіх ячейок в структурований вигляд
    /// </summary>
    private List<Vector3> CollectCellDesiredSizes() {
        var sizes = new List<Vector3>(cellPresenters.Count);

        for (int row = 0; row < Board.RowCount; row++) {
            for (int col = 0; col < Board.ColumnCount; col++) {
                if (cellPresentersByIndex.TryGetValue((row, col), out var presenter)) {
                    sizes.Add(presenter.DesiredSize);
                } else {
                    sizes.Add(BoardView.layoutSettings.itemSizes);
                }
            }
        }

        return sizes;
    }

    /// <summary>
    /// Обчислює максимальні ширини колонок і висоти рядів
    /// </summary>
    private (float[] columnWidths, float[] rowHeights) CalculateGridDimensions(List<Vector3> cellSizes) {
        float[] columnWidths = new float[Board.ColumnCount];
        float[] rowHeights = new float[Board.RowCount];

        Vector3 defaultSize = BoardView.layoutSettings.itemSizes;

        // Ініціалізація мінімальними значеннями
        for (int c = 0; c < Board.ColumnCount; c++) {
            columnWidths[c] = defaultSize.x;
        }
        for (int r = 0; r < Board.RowCount; r++) {
            rowHeights[r] = defaultSize.z;
        }

        // Знаходимо максимуми
        int index = 0;
        for (int row = 0; row < Board.RowCount; row++) {
            for (int col = 0; col < Board.ColumnCount; col++) {
                if (index < cellSizes.Count) {
                    Vector3 size = cellSizes[index];
                    columnWidths[col] = Mathf.Max(columnWidths[col], size.x);
                    rowHeights[row] = Mathf.Max(rowHeights[row], size.z);
                }
                index++;
            }
        }

        return (columnWidths, rowHeights);
    }

    /// <summary>
    /// Застосовує фактичні розміри до ячейок
    /// </summary>
    private void ApplySizesToCells(float[] columnWidths, float[] rowHeights) {
        for (int row = 0; row < Board.RowCount; row++) {
            for (int col = 0; col < Board.ColumnCount; col++) {
                if (cellPresentersByIndex.TryGetValue((row, col), out var presenter)) {
                    Vector3 actualSize = new Vector3(
                        columnWidths[col],
                        presenter.ActualSize.y, // Y не змінюємо
                        rowHeights[row]
                    );

                    presenter.ApplyActualSize(actualSize);
                }
            }
        }
    }

    /// <summary>
    /// Просимо BoardView обчислити позиції і застосовуємо їх
    /// </summary>
    private void ApplyPositionsToClls(float[] columnWidths, float[] rowHeights) {
        // Створюємо список розмірів для Layout
        ItemLayoutInfo[] itemLayoutInfos = new ItemLayoutInfo[Board.RowCount * Board.ColumnCount];

        int cellIndex = 0;
        for (int row = 0; row < Board.RowCount; row++) {
            for (int col = 0; col < Board.ColumnCount; col++) {
                if (cellPresentersByIndex.TryGetValue((row, col), out var presenter)) {
                    Vector3 dimension = new Vector3(columnWidths[col], 1f, rowHeights[row]);
                    itemLayoutInfos[cellIndex] = new ItemLayoutInfo(presenter.Cell.Id, dimension);
                }
                cellIndex++;
            }
        }

        Grid<ItemLayoutInfo> grid = new Grid<ItemLayoutInfo>(itemLayoutInfos, Board.ColumnCount);
        var layoutResult = layout.Calculate(grid, false);

        for (int i = 0; i < layoutResult.Points.Length; i++) {
            LayoutPoint layoutPoint = layoutResult.Points[i];
            if (cellPresentersByIndex.TryGetValue((layoutPoint.Row, layoutPoint.Column), out var presenter)) {
                presenter.ApplyPosition(layoutPoint.Position, layoutPoint.Rotation);
            }
        }
    }

    #region Event Handlers
    private void OnColumnAdded(object sender, ColumnAddedEvent e) {
        var newCellPresenters = new List<CellPresenter>();

        for (int i = 0; i < e.NewColumn.Count; i++) {
            var cell = e.NewColumn[i];
            var cellPresenter = CreateCellPresenter(cell, i, e.NewColumnIndex);
            newCellPresenters.Add(cellPresenter);
        }

        RecalculateLayout();
    }

    private void OnColumnRemoved(object sender, ColumnRemovedEvent e) {
        foreach (var cell in e.RemovedColumn) {
            if (cellPresenters.TryGetValue(cell, out CellPresenter presenter)) {
                presenter.OnDesiredSizeChanged -= OnCellDesiredSizeChanged;
                cellPresenters.Remove(cell);

                // Видаляємо з кешу
                var key = cellPresentersByIndex.FirstOrDefault(x => x.Value == presenter).Key;
                cellPresentersByIndex.Remove(key);

                presenter.Dispose();
            }
        }

        RecalculateLayout();
    }
    #endregion

    public void Dispose() {
        foreach (var presenter in cellPresenters.Values) {
            presenter.OnDesiredSizeChanged -= OnCellDesiredSizeChanged;
            presenter.Dispose();
        }

        cellPresenters.Clear();
        cellPresentersByIndex.Clear();
        UnsubscribeFromEvents();
    }

    public void AssignArea(int row, int column, Zone zone) {
        Board.AssignAreaModelToCell(row, column, zone);
    }

    public void AssignArea(Cell cell, Zone zone) {
        Board.AssignAreaModelToCell(cell, zone);
    }
}