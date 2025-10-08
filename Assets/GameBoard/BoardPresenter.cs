using System;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEditor.Build.Pipeline.Tasks;

public class BoardPresenter : IDisposable {
    public BoardView BoardView;
    public Board Board;

    private Dictionary<Cell, CellPresenter> cellPresenters = new();

    //  еш дл€ швидкого доступу до €чейок по ≥ндексах
    private Dictionary<(int row, int col), CellPresenter> cellPresentersByIndex = new();

    [Inject] IPresenterFactory presenterFactory;

    public BoardPresenter(Board board, BoardView boardView) {
        Board = board;
        BoardView = boardView;
        SubscribeToEvents();
    }

    private void SubscribeToEvents() {
        if (Board != null) {
            Board.ColumnRemoved += HandleColumnRemoved;
            Board.ColumnAdded += HandleColumnAdded;
        }
    }

    private void UnsubscribeFromEvents() {
        if (Board != null) {
            Board.ColumnRemoved += HandleColumnRemoved;
            Board.ColumnAdded += HandleColumnAdded;
        }
    }

    private void HandleColumnRemoved(object sender, ColumnRemovedEvent eventData) {
        List<Cell> removedColumn = eventData.RemovedColumn;

        foreach (Cell cell in removedColumn) {
            BoardView.RemoveCell(cell.RowIndex, cell.ColumnIndex, false);
        }
        BoardView.UpdateLayout().Forget();
    }

    private void HandleColumnAdded(object sender, ColumnAddedEvent eventData) {
        List<Cell> newColumn = eventData.NewColumn;

        foreach (Cell cell in newColumn) {
            BoardView.CreateCell(cell.RowIndex, cell.ColumnIndex, false);
        }
        BoardView.UpdateLayout().Forget();
    }

    public void CreateBoard() {
        for (int rowIndex = 0; rowIndex < Board.RowCount; rowIndex++) {
            var row = Board.GetRow(rowIndex);
            for (int cellIndex = 0; cellIndex < row.CellCount; cellIndex++) {
                var cell = row.GetCell(cellIndex);
                if (cell == null) continue;

                var cellView = BoardView.CreateCell(cell.RowIndex, cell.ColumnIndex, false);
                CellPresenter cellPresenter = CreateCellPresenter(cell, cellView);

                cellPresentersByIndex[(rowIndex, cellIndex)] = cellPresenter;
            }
        }
        BoardView.UpdateLayout().Forget();
    }

    private CellPresenter CreateCellPresenter(Cell cell, Cell3DView view) {
        if (view == null) {
            Debug.LogWarning("Failed to create view in " + this);
            return null;
        }
        var cellPresenter = presenterFactory.CreatePresenter<CellPresenter>(cell, view);

        cellPresenters[cell] = cellPresenter;
       
        cellPresenter.OnSizeChanged += HandleCellSizeChanged;
        return cellPresenter;
    }

    private void HandleCellSizeChanged(CellPresenter presenter, Vector3 size) {
        BoardView.UpdateLayout().Forget();
    }

    public void AssignArea(int row, int column, Zone zone) {
        Board.AssignAreaModelToCell(row, column, zone);
    }

    public void AssignArea(Cell cell, Zone zone) {
        Board.AssignAreaModelToCell(cell, zone);
    }

    public void Dispose() {
        foreach (var presenter in cellPresenters.Values) {
            presenter.Dispose();
        }

        cellPresenters.Clear();
        cellPresentersByIndex.Clear();
        UnsubscribeFromEvents();
    }
}