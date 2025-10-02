using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

/// <summary>
/// Main presenter that manages the connection between Board logic and BoardView
/// </summary>
public class BoardPresenter : UnitPresenter, IDisposable {
    public BoardView BoardView;
    public Board Board;

    private Dictionary<int, float> _maxColumnWidths = new Dictionary<int, float>();
    private Dictionary<int, float> _maxRowLengths = new Dictionary<int, float>();

    private Dictionary<Cell, CellPresenter> cellPresenters = new Dictionary<Cell, CellPresenter>();
    [Inject] IPresenterFactory presenterFactory;

    public BoardPresenter(Board board, BoardView boardView) : base(board, boardView) {
        Board = board;
        BoardView = boardView;
        InitializeLayoutMaps();
        boardView.Initialize();
        SubscribeToEvents();
    }

    private void InitializeLayoutMaps() {
        _maxColumnWidths.Clear();
        _maxRowLengths.Clear();

        Vector3 defaulCellSizes = BoardView.GetDefaultCellSize();

        // Ініціалізація всіх стовпців і рядків нульовими значеннями
        for (int c = 0; c < Board.ColumnCount; c++) {
            _maxColumnWidths[c] = defaulCellSizes.x;
        }

        for (int r = 0; r < Board.RowCount; r++) {
            _maxRowLengths[r] = defaulCellSizes.z;
        }
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
                CreateCellPresenter(cell);
            }
        }

        BoardView.BuildBoardVisual(
            cellPresenters.Values.Select(p => p.CellView).ToList(),
            Board.RowCount
        );

        // Початковий розрахунок Layout
        RecalculateLayout();
    }

    private CellPresenter CreateCellPresenter(Cell cell) {
        var cellView = BoardView.CreateCell(cell);
        var cellPresenter = presenterFactory.CreatePresenter<CellPresenter>(cell, cellView);
        cellPresenter.OnContentSizeChanged += OnCellContentSizeChanged;
        cellPresenters[cell] = cellPresenter;
        return cellPresenter;
    }

    private void OnCellContentSizeChanged(CellPresenter cellPresenter, Vector3 contentSize) {
        var cellModel = cellPresenter.Cell;
        int col = cellModel.ColumnIndex;
        int row = cellModel.RowIndex;

        bool layoutChanged = false;

        // 1. Оновлення ширини стовпця
        float newWidth = contentSize.x;
        if (newWidth > _maxColumnWidths[col]) {
            _maxColumnWidths[col] = newWidth;
            layoutChanged = true;
        }

        // 2. Оновлення висоти рядка
        float newHeight = contentSize.y; // Або z, залежно від вашої 3D-орієнтації
        if (newHeight > _maxRowLengths[row]) {
            _maxRowLengths[row] = newHeight;
            layoutChanged = true;
        }

        // 3. Перерахунок макету, якщо відбулася зміна
        if (layoutChanged) {
            RecalculateLayout();
        }
    }

    private void RecalculateLayout() {
        // 1. Проходимося по всіх презентерах комірок
        foreach (var (cellModel, cellPresenter) in cellPresenters) {
            int col = cellModel.ColumnIndex;
            int row = cellModel.RowIndex;

            // 2. Отримуємо максимальні розміри для цієї комірки
            float requiredWidth = _maxColumnWidths[col];
            float requiredLength = _maxRowLengths[row];

            // 3. Застосовуємо новий розмір до комірки
            // Викликаємо метод у CellPresenter, який оновлює CellView
            // Важливо: передаємо розмір, який повинен мати КОНТЕЙНЕР (CellView)
            Vector3 newCellSize = new Vector3(requiredWidth, cellPresenter.CellView.transform.localScale.y, requiredLength);
            if (cellPresenter.ContentSize.sqrMagnitude < newCellSize.sqrMagnitude) {
                cellPresenter.ChangeSize(newCellSize);
            }
            
        }

        // 4. Повідомляємо BoardView, щоб вона перерахувала позиції всіх комірок
        // (BoardView тепер відповідає за правильне розташування комірок на основі їхніх нових розмірів)
        BoardView.RecalculateLayout();
    }

    #region Event Handlers


    private void OnColumnAdded(object sender, ColumnAddedEvent e) {
        var newCellPresenters = new List<CellPresenter>();

        for (int i = 0; i < e.NewColumn.Count; i++) {
            var cell = e.NewColumn[i];
            var cellPresenter = CreateCellPresenter(cell);
            newCellPresenters.Add(cellPresenter);
        }

        BoardView.AddColumn(newCellPresenters.Select(p => p.CellView).ToList(), e.NewColumnIndex);
    }

    private void OnColumnRemoved(object sender, ColumnRemovedEvent e) {
        List<Cell3DView> cellViews = new();
        foreach (var cell in e.RemovedColumn) {
            if (cellPresenters.TryGetValue(cell, out CellPresenter presenter)) {
                cellViews.Add(presenter.CellView);
                cellPresenters.Remove(cell);
            }
        }

        BoardView.RemoveColumn(cellViews, e.OldCellIndex);
    }

    #endregion

    public void UpdateLayout() {
        BoardView.RecalculateLayout();
    }

    public void Dispose() {
        foreach (var presenter in cellPresenters.Values)
            presenter.Dispose();

        cellPresenters.Clear();
        UnsubscribeFromEvents();
    }

    public void AssignArea(int row, int column, Zone zone) {
        Board.AssignAreaModelToCell(row, column, zone);
    }
}

