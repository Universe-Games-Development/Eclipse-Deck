using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Gameboard prototype visualizer
// MonoBehaviour while prototyping
/// <summary>
/// Main presenter that manages the connection between Board logic and BoardView
/// </summary>
public class BoardPresenter : IDisposable {
    private BoardView boardView;
    private CellFactory cellFactory;
   

    private Board _board;
    private Dictionary<Cell, Cell3DView> cellViews = new Dictionary<Cell, Cell3DView>();

    public BoardPresenter(Board board, BoardView boardView) {
        _board = board;

        // Test
        _board = SetupInitialBoard();
        SubscribeToEvents();
        CreateBoard();
    }
    private void OnDestroy() {
        UnsubscribeFromEvents();
    }

    private Board SetupInitialBoard() {
        var config = new BoardConfiguration()
            .AddRow(2, 3, 2, 5)
            .AddRow(1, 4, 1, 3);
        return new Board(config);
    }

    private void SubscribeToEvents() {
        _board.ColumnAdded += OnColumnAdded;
        _board.ColumnRemoved += OnColumnRemoved;
    }

    private void UnsubscribeFromEvents() {
        if (_board != null) {
            _board.ColumnAdded -= OnColumnAdded;
            _board.ColumnRemoved -= OnColumnRemoved;
        }
    }

    private void CreateBoard() {
        var cellViews = new List<Cell3DView>();

        for (int rowIndex = 0; rowIndex < _board.RowCount; rowIndex++) {
            var row = _board.GetRow(rowIndex);
            for (int cellIndex = 0; cellIndex < row.CellCount; cellIndex++) {
                var cell = row.GetCell(cellIndex);
                if (cell == null) {
                    Debug.LogError($"Cell {cellIndex} in Row {rowIndex} is null.");
                    continue;
                }

                var cellView = CreateCellView(cell, rowIndex, cellIndex);
                cellViews.Add(cellView);
                this.cellViews[cell] = cellView;
            }
        }

        boardView.BuildBoardVisual(cellViews, _board.RowCount);
    }

    private Cell3DView CreateCellView(Cell cell, int rowIndex, int cellIndex) {
        var cellView = cellFactory.CreateCell(cell);

        // CellPresenter повідомляє про зміни розміру
        cellView.OnSizeChanged += OnCellSizeChanged;

        return cellView;
    }

    private void OnCellSizeChanged(Vector3 vector) {
        boardView.RecalculateLayout();
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

        boardView.AddColumn(newCells, e.NewColumnIndex);
    }

    private void OnColumnRemoved(object sender, ColumnRemovedEvent e) {
        var removedCells = new List<Cell3DView>();

        foreach (var cell in e.RemovedColumn) {
            if (cellViews.TryGetValue(cell, out Cell3DView view)) {
                removedCells.Add(view);
                cellViews.Remove(cell);
            }
        }

        boardView.RemoveColumn(removedCells, e.OldCellIndex);
    }

    #endregion

    public void UpdateLayout() {
        boardView.RecalculateLayout();
    }

    public void Dispose() {
    }
}

