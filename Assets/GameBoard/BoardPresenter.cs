using System;
using System.Collections.Generic;
using Zenject;

public class BoardPresenter : IDisposable {
    public BoardView BoardView;
    public Board Board;

    private Dictionary<Cell, CellPresenter> cellPresenters = new();

    [Inject] IPresenterFactory _presenterFactory;

    public BoardPresenter(Board board, BoardView boardView, IPresenterFactory presenterFactory) {
        Board = board;
        BoardView = boardView;
        _presenterFactory = presenterFactory;

        SubscribeToEvents();

        List<Cell> removedCells = new();
        List<Cell> addedCells = new(board.GetAllCells());
        HandleNewSctructure(this, new BoardStructureChangedEvent(addedCells, removedCells));
    }

    private void SubscribeToEvents() {
        if (Board != null) {
            Board.StructureChanged += HandleNewSctructure;
        }
    }

    private void UnsubscribeFromEvents() {
        if (Board != null) {
            Board.StructureChanged -= HandleNewSctructure;
        }
    }

    private void HandleNewSctructure(object sender, BoardStructureChangedEvent eventData) {
        HandleCellsRemoved(eventData.RemovedCells);
        HandleCellsAdded(eventData.AddedCells);
    }

    private void HandleCellsRemoved(List<Cell> removedCells) {
        foreach (var cell in removedCells) {
            if (cellPresenters.TryGetValue(cell, out var presenter)) {
                presenter.OnSizeChanged -= HandleCellSizeChanged;
                cellPresenters.Remove(cell);
                presenter.Dispose();

                // Видаляємо клітинку
                BoardView.RemoveCell(cell.RowIndex, cell.ColumnIndex, false);
            }
        }

        BoardView.UpdateLayout();
    }

    private void HandleCellsAdded(List<Cell> addedCells) {
        // ✅ ТЕПЕР ПРАВИЛЬНИЙ ПОРЯДОК:
        // 1. Створюємо View СИНХРОННО
        // 2. Створюємо Presenter СИНХРОННО  
        // 3. Додаємо View на дошку АСИНХРОННО (через visualManager)
        foreach (var cell in addedCells) {
            CreateCellWithPresenter(cell);
        }

        BoardView.UpdateLayout();
    }

    private void CreateCellWithPresenter(Cell cell) {
        // ✅ КРОК 1: Створюємо View СИНХРОННО (невидиме)
        Cell3DView cellView = BoardView.CreateCellView();

        // ✅ КРОК 2: Створюємо Presenter СИНХРОННО (вже маємо View)
        var cellPresenter = _presenterFactory.CreatePresenter<CellPresenter>(cell, cellView);

        cellPresenters[cell] = cellPresenter;
        cellPresenter.OnSizeChanged += HandleCellSizeChanged;

        // ✅ КРОК 3: Додаємо View на дошку АСИНХРОННО (через visualManager)
        BoardView.AddCellView(cellView, cell.RowIndex, cell.ColumnIndex, false);
    }

    public void UpdateLayout() {
        BoardView.UpdateLayout();
    }

    private void HandleCellSizeChanged(CellPresenter presenter) {
        UpdateLayout();
    }

    public void AssignArea(int row, int column, Zone zone) {
        Board.AssignAreaModelToCell(row, column, zone);
    }

    public void AssignArea(Cell cell, Zone zone) {
        Board.AssignAreaModelToCell(cell, zone);
    }

    public void AssignAreas(List<Cell> cells, List<Zone> zones) {
        if (zones.Count < cells.Count) return;

        for (int i = 0; i < cells.Count; i++) {
            cells[i].AssignUnit(zones[i]);
        }
        UpdateLayout();
    }

    public void Dispose() {
        foreach (var presenter in cellPresenters.Values) {
            presenter.OnSizeChanged -= HandleCellSizeChanged;
            presenter.Dispose();
        }

        cellPresenters.Clear();
        UnsubscribeFromEvents();
    }
}