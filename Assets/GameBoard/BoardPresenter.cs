using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Main presenter that manages the connection between Board logic and BoardView
/// </summary>
public class BoardPresenter : UnitPresenter, IDisposable {
    public BoardView BoardView;
    public Board Board;
    
    private Dictionary<Cell, Cell3DView> cellViews = new Dictionary<Cell, Cell3DView>();

    public BoardPresenter(Board board, BoardView boardView) : base(board, boardView) {
        Board = board;
        BoardView = boardView;
        boardView.Initialize();
        SubscribeToEvents();
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
                if (cell == null) {
                    Debug.LogError($"Cell {cellIndex} in Row {rowIndex} is null.");
                    continue;
                }

                var cellView = CreateCellView(cell, rowIndex, cellIndex);
                this.cellViews[cell] = cellView;
            }
        }

        BoardView.BuildBoardVisual(cellViews.Values.ToList(), Board.RowCount);
    }

    private Cell3DView CreateCellView(Cell cell, int rowIndex, int cellIndex) {
        var cellView = BoardView.CreateCell(cell);

        // CellPresenter повідомляє про зміни розміру
        cellView.OnSizeChanged += OnCellSizeChanged;

        return cellView;
    }

    private void OnCellSizeChanged(Vector3 vector) {
        BoardView.RecalculateLayout();
    }

    #region Event Handlers


    private void OnColumnAdded(object sender, ColumnAddedEvent e) {
        var newCells = new List<Cell3DView>();

        for (int i = 0; i < e.NewColumn.Count; i++) {
            var cell = e.NewColumn[i];
            var cell3DView = CreateCellView(cell, i, e.NewColumnIndex);
            newCells.Add(cell3DView);
            cellViews[cell] = cell3DView;
        }

        BoardView.AddColumn(newCells, e.NewColumnIndex);
    }

    private void OnColumnRemoved(object sender, ColumnRemovedEvent e) {
        var removedCells = new List<Cell3DView>();

        foreach (var cell in e.RemovedColumn) {
            if (cellViews.TryGetValue(cell, out Cell3DView view)) {
                removedCells.Add(view);
                cellViews.Remove(cell);
            }
        }

        BoardView.RemoveColumn(removedCells, e.OldCellIndex);
    }

    #endregion

    public void UpdateLayout() {
        BoardView.RecalculateLayout();
    }

    public void Dispose() {
        UnsubscribeFromEvents();
    }

    public void AssignArea(int row, int column, Zone zone) {
        Board.AssignAreaModelToCell(row, column, zone);
    }
}

