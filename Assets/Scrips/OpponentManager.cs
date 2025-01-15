using System.Collections.Generic;
using System;
using UnityEngine;

public class OpponentManager {
    private readonly Dictionary<Opponent, SubGrid> opponentGrids = new();
    public List<Opponent> registeredOpponents = new List<Opponent>();

    private GridManager gridManager;
    public OpponentManager(GridManager gridManager) {
        this.gridManager = gridManager;
    }

    public Opponent GetNextOpponent(Opponent current) {
        if (registeredOpponents.Count == 0) {
            throw new InvalidOperationException("No registered opponents to get the next opponent.");
        }

        int currentIndex = registeredOpponents.IndexOf(current);
        return registeredOpponents[(currentIndex + 1) % registeredOpponents.Count];
    }

    public void RegisterOpponent(Opponent opponent) {
        if (!registeredOpponents.Contains(opponent)) {
            registeredOpponents.Add(opponent);
            AssignGridToOpponent(opponent, opponent is Player ? gridManager.PlayerGrid : gridManager.EnemyGrid);
            opponent.OnDefeat += UnRegisterOpponent;
            Debug.Log($"Opponent {opponent.Name} registered.");
        }
    }

    public void UnRegisterOpponent(Opponent opponent) {
        if (registeredOpponents.Contains(opponent)) {
            registeredOpponents.Remove(opponent);
            opponent.OnDefeat -= UnRegisterOpponent;
            Debug.Log($"Opponent {opponent.Name} unregistered.");
        }
    }

    public void OccupyGrid(Opponent opponent) {
        if (opponent == null || opponent is not (Player or Enemy)) {
            throw new ArgumentException("Invalid opponent type.");
        }

        AssignGridToOpponent(opponent, opponent is Player ? gridManager.PlayerGrid : gridManager.EnemyGrid);
    }

    public void AssignGridToOpponent(Opponent opponent, SubGrid grid) {
        if (opponent == null) {
            throw new ArgumentException("Opponent cannot be null.");
        }

        if (opponentGrids.ContainsKey(opponent)) {
            throw new InvalidOperationException("This opponent already has assigned fields.");
        }

        grid.Fields.ForEach(row => row.ForEach(field => field.AssignOwner(opponent)));
        opponentGrids.Add(opponent, grid);

        opponent.OnDefeat += opponent => UnassignGrid(opponent);
        Debug.Log($"Grid assigned to opponent {opponent.Name}.");
    }

    public void UnassignGrid(Opponent opponent) {
        if (opponentGrids.TryGetValue(opponent, out var grid)) {
            grid.Fields.ForEach(row => row.ForEach(field => field.AssignOwner(null)));
            opponentGrids.Remove(opponent);
            Debug.Log($"Grid unassigned from opponent {opponent.Name}.");
        }
    }

    public Field GetFieldAt(Opponent opponent, int row, int column) {
        opponentGrids.TryGetValue(opponent, out SubGrid grid);
        return grid.Fields[row][column];
    }

    public SubGrid GetGrid(Opponent opponent) {
        if (!opponentGrids.TryGetValue(opponent, out var grid)) {
            throw new ArgumentException("No grid assigned to this opponent.");
        }
        return grid;
    }
}
