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
        SyncLogicBoardToView();
    }

    private void SyncLogicBoardToView() {
        List<Cell> removedCells = new();
        List<Cell> addedCells = new(Board.GetAllCells());


        HandleNewStructure(this, new BoardStructureChangedEvent(addedCells, removedCells));
    }

    private void SubscribeToEvents() {
        if (Board != null) {
            Board.StructureChanged += HandleNewStructure;
        }
    }

    private void UnsubscribeFromEvents() {
        if (Board != null) {
            Board.StructureChanged -= HandleNewStructure;
        }
    }

    private void HandleNewStructure(object sender, BoardStructureChangedEvent eventData) {
        HandleCellsRemoved(eventData.RemovedCells);
        HandleCellsAdded(eventData.AddedCells);
    }

    private void HandleCellsRemoved(List<Cell> removedCells) {
        if (removedCells == null || removedCells.Count == 0) return;

        List<(int row, int column)> cellsData = new();

        foreach (var cell in removedCells) {
            if (cellPresenters.TryGetValue(cell, out var presenter)) {
                presenter.OnSizeChanged -= HandleCellSizeChanged;
                cellPresenters.Remove(cell);
                presenter.Dispose();

                cellsData.Add((cell.RowIndex, cell.ColumnIndex));
            }
        }

        if (cellsData.Count > 0) {
            BoardView.RemoveCellsBatch(cellsData);
        }
    }

    private void HandleCellsAdded(List<Cell> addedCells) {
        if (addedCells == null || addedCells.Count == 0) return;

        var cellsData = new List<(Cell3DView cellView, int row, int column)>();

        foreach (var cell in addedCells) {
            // ✅ КРОК 1: Створюємо View СИНХРОННО (невидиме)
            Cell3DView cellView = BoardView.CreateCellView();

            // ✅ КРОК 2: Створюємо Presenter СИНХРОННО (вже маємо View)
            var cellPresenter = _presenterFactory.CreatePresenter<CellPresenter>(cell, cellView);
            cellPresenters[cell] = cellPresenter;
            cellPresenter.OnSizeChanged += HandleCellSizeChanged;

            // ✅ КРОК 3: Додаємо до списку для батч-операції
            cellsData.Add((cellView, cell.RowIndex, cell.ColumnIndex));
        }

        // ✅ КРОК 4: Додаємо всі View на дошку ОДНИМ батчем (через visualManager)
        if (cellsData.Count > 0) {
            BoardView.AddCellViewBatch(cellsData);
        }
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