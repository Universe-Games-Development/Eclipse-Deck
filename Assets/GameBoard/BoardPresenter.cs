using System;
using System.Collections.Generic;
using Zenject;
using UnityEngine;

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
        }
    }

    private void UnsubscribeFromEvents() {
        if (Board != null) {
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
    }

    private CellPresenter CreateCellPresenter(Cell cell, int rowIndex, int colIndex) {
        var cellView = BoardView.CreateCell(cell.RowIndex, cell.ColumnIndex);

        if (cellView == null) {
            Debug.LogWarning("Failed to create view in " + this);
            return null;
        }
        var cellPresenter = presenterFactory.CreatePresenter<CellPresenter>(cell, cellView);

        cellPresenters[cell] = cellPresenter;
        cellPresentersByIndex[(rowIndex, colIndex)] = cellPresenter;

        return cellPresenter;
    }


    public void Dispose() {
        foreach (var presenter in cellPresenters.Values) {
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