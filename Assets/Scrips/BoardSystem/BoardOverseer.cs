using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class BoardOverseer {
    public Grid MainGrid {
        get {
            return mainGrid;
        }
        private set {  mainGrid = value; }
    }
    private Grid mainGrid;
    private SubGrid playerGrid, enemyGrid;
    private readonly Dictionary<Opponent, SubGrid> opponentGrids = new();

    public BoardOverseer(BoardSettings config) {
        ValidateBoardSettings(config);
        InitializeGrids(config);
    }

    public void UpdateBoard(BoardSettings config) {
        ValidateBoardSettings(config);
        UpdateGrids(config);
    }

    private void InitializeGrids(BoardSettings config) {
        int rows = config.rowTypes.Count;
        int columns = config.columns;
        int divider = config.rowTypes.FindIndex(row => row == FieldType.Attack);

        mainGrid = new Grid(rows, columns);
        playerGrid = new SubGrid(mainGrid, 0, divider);
        enemyGrid = new SubGrid(mainGrid, divider + 1, rows - 1);

        UpdateFieldTypes(config);
        Debug.Log("Initial Board:");
    }

    private void UpdateGrids(BoardSettings config) {
        int rows = config.rowTypes.Count;
        int columns = config.columns;
        int divider = config.rowTypes.FindIndex(row => row == FieldType.Attack);

        mainGrid.UpdateGridSize(rows, columns);
        playerGrid.BoundToMainGrid(mainGrid, 0, divider);
        enemyGrid.BoundToMainGrid(mainGrid, divider + 1, rows - 1);

        UpdateFieldTypes(config);
    }


    public void OccupyGrid(Opponent opponent) {
        if (opponent == null || opponent is not (Player or Enemy)) {
            throw new ArgumentException("Invalid opponent type.");
        }

        SubGrid gridToAssign = opponent switch {
            Player => playerGrid,
            Enemy => enemyGrid,
            _ => throw new ArgumentException("Can't assign grid to this opponent type.")
        };

        if (opponentGrids.ContainsKey(opponent)) {
            throw new InvalidOperationException("This opponent already has assigned fields.");
        }

        opponent.OnDefeat += UnoccupyGrid;
        gridToAssign.Fields.ForEach(row => row.ForEach(field => field.AssignOwner(opponent)));

        opponentGrids.Add(opponent, gridToAssign);
    }

    private void UnoccupyGrid(Opponent opponent) {
        if (opponentGrids.TryGetValue(opponent, out var grid)) {
            grid.Fields.ForEach(row => row.ForEach(field => field.AssignOwner(null)));
            opponentGrids.Remove(opponent); opponent.OnDefeat -= UnoccupyGrid;
        } else {
            throw new ArgumentException("There are no fields assigned to this opponent in the dictionary.");
        }
    }


    private void UpdateFieldTypes(BoardSettings config) {
        mainGrid.Fields
            .ForEach(row =>
            row.ForEach(field => { field.Type = config.rowTypes[field.row] == FieldType.Attack ? FieldType.Attack : FieldType.Support; }));
    }


    public List<Field> GetGridPath(Field currentField, int pathAmount, Direction direction) {
        List<Field> path = new List<Field>();

        var (rowOffset, colOffset) = DirectionHelper.DirectionOffsets[direction];

        for (int i = 1; i <= pathAmount; i++) {
            int newRow = currentField.row + rowOffset * i;
            int newCol = currentField.column + colOffset * i;

            if (newRow >= 0 && newRow < mainGrid.Fields.Count && newCol >= 0 && newCol < mainGrid.Fields[0].Count) {
                path.Add(mainGrid.Fields[newRow][newCol]);
            } else {
                break;
            }
        }

        return path;
    }

    public Grid GetGrid() {
        return mainGrid;
    }

    public SubGrid GetGrid(Opponent opponent) {
        opponentGrids.TryGetValue(opponent, out SubGrid grid);
        if (grid == null) throw new ArgumentException("There are no fields assigned to this opponent in the dictionary.");
        return grid;
    }

    public Field GetFieldAt(int row, int column) {
        return MainGrid.Fields[row][column];
    }

    public bool FieldExists(Field field) {
        foreach (var column in MainGrid.Fields) {
            if (column != null && column.Contains(field)) {
                return true;
            }
        }
        return false;
    }


    private void ValidateBoardSettings(BoardSettings settings) {
        if (settings.rowTypes.Count < 2 || settings.columns < 2) {
            throw new System.ArgumentException("BoardSettings must have at least 2 rows and 2 columns.");
        }

        var attackIndices = settings.rowTypes
            .Select((type, index) => new { type, index })
            .Where(x => x.type == FieldType.Attack)
            .Select(x => x.index)
            .ToList();

        if (attackIndices.Count != 2 || Mathf.Abs(attackIndices[1] - attackIndices[0]) != 1) {
            throw new System.ArgumentException("BoardSettings must have exactly 2 adjacent Attack rows.");
        }
    }
}

