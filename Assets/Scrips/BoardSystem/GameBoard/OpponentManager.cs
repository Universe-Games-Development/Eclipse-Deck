using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
public class OpponentManager {

    private readonly Dictionary<Opponent, SubGrid> opponentGrids = new();
    public List<Opponent> registeredOpponents = new List<Opponent>();

    [Inject] private GridManager gridManager;

    public int MinPlayers { get; private set; }


    public OpponentManager(GridManager gridManager) {
        this.gridManager = gridManager;
    }

    internal void UpdateSettings(BoardSettings boardSettings) {
        MinPlayers = boardSettings.minPlayers;
    }

    public void RegisterOpponent(Opponent opponent) {
        if (registeredOpponents.Count < MinPlayers) {
            if (!registeredOpponents.Contains(opponent)) {
                registeredOpponents.Add(opponent);
                AssignGridToOpponent(opponent);
                opponent.OnDefeat += UnRegisterOpponent;
                Debug.Log($"Opponent {opponent.Name} registered.");
            }
        } else {
            Debug.LogWarning("Cannot register more opponents. Minimum players reached.");
        }
    }

    public void UnRegisterOpponent(Opponent opponent) {
        if (registeredOpponents.Contains(opponent)) {
            registeredOpponents.Remove(opponent);
            opponent.OnDefeat -= UnRegisterOpponent;
            Debug.Log($"Opponent {opponent.Name} unregistered.");
        }
    }

    public void AssignGridToOpponent(Opponent opponent) {
        if (opponent == null || opponent is not (Player or Enemy)) {
            throw new ArgumentException("Invalid opponent type.");
        }

        if (opponentGrids.ContainsKey(opponent)) {
            throw new InvalidOperationException("This opponent already has assigned fields.");
        }
        SubGrid grid = GetGrid(opponent);

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

    public SubGrid GetGrid(Opponent opponent) {
        return opponent is Player ? gridManager.PlayerGrid : gridManager.EnemyGrid;
    }

    public Opponent GetNextOpponent(Opponent current) {
        if (registeredOpponents.Count == 0) {
            throw new InvalidOperationException("No registered opponents to get the next opponent.");
        }

        int currentIndex = registeredOpponents.IndexOf(current);
        return registeredOpponents[(currentIndex + 1) % registeredOpponents.Count];
    }

    public Opponent GetRandomOpponent() {
        return registeredOpponents[UnityEngine.Random.Range(0, registeredOpponents.Count)];
    }
    public bool IsAllRegistered() {
        return registeredOpponents.Count >= MinPlayers;
    }
}
