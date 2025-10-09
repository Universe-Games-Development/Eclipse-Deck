using System;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BoardPresenter : IDisposable {
    public BoardView BoardView;
    public Board Board;

    private Dictionary<Cell, CellPresenter> cellPresenters = new();

    [Inject] IPresenterFactory presenterFactory;
    [Inject] IVisualManager visualManager;

    public BoardPresenter(Board board, BoardView boardView) {
        Board = board;
        BoardView = boardView;
        SubscribeToEvents();
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
        var removeColumnTask = new UniversalVisualTask(async () => {
            var removalTasks = new List<UniTask>();

            foreach (var cell in removedCells) {
                if (cellPresenters.TryGetValue(cell, out var presenter)) {
                    // Видалення презентера/кешу (синхронна дія)
                    presenter.OnSizeChanged -= HandleCellSizeChanged;
                    cellPresenters.Remove(cell);
                    presenter.Dispose();
                }
                // Збираємо асинхронні завдання видалення View
                removalTasks.Add(BoardView.RemoveCellAsync(cell.RowIndex, cell.ColumnIndex, false));
            }

            // Чекаємо завершення паралельного видалення всіх View
            await UniTask.WhenAll(removalTasks);
            await BoardView.UpdateLayoutAsync();
            return true;
        }, $"Remove cells {removedCells.Count}");

        visualManager.Push(removeColumnTask);
    }

    private void HandleCellsAdded(List<Cell> addedCells) {
        var addColumnTask = new UniversalVisualTask(async () => {
            var creationTasks = new List<UniTask>();

            // Створюємо комірки та їх презентери
            foreach (var cell in addedCells) {
                var task = CreateCellWithPresenterAsync(cell);
                creationTasks.Add(task);
            }

            await UniTask.WhenAll(creationTasks);
            await BoardView.UpdateLayoutAsync();

            return true;
        }, $"Add cells {addedCells.Count}");

        visualManager.Push(addColumnTask);
    }

    private async UniTask CreateCellWithPresenterAsync(Cell cell) {
        var cellView = await BoardView.CreateCellAsync(cell.RowIndex, cell.ColumnIndex, false);
        CreateCellPresenter(cell, cellView);
    }

    public void CreateBoard() {
        var createBoardTask = new UniversalVisualTask(async () => {
            var creationTasks = new List<UniTask>();

            for (int rowIndex = 0; rowIndex < Board.RowCount; rowIndex++) {
                var row = Board.GetRow(rowIndex);
                for (int cellIndex = 0; cellIndex < row.CellCount; cellIndex++) {
                    var cell = row.GetCell(cellIndex);
                    if (cell == null) continue;

                    var task = CreateCellWithPresenterAsync(cell);
                    creationTasks.Add(task);
                }
            }

            await UniTask.WhenAll(creationTasks);
            await BoardView.UpdateLayoutAsync();
        }, $"Create Board");

        visualManager.Push(createBoardTask);
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

    public void UpdateLayout() {
        UniversalVisualTask universalVisualTask = new(BoardView.UpdateLayoutAsync, "Layout Update");
        visualManager.Push(universalVisualTask);
    }

    private void HandleCellSizeChanged(CellPresenter presenter, Vector3 size) {
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

