using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class OpponentManager {
    private readonly Dictionary<Opponent, OpponentGrid> opponentGrids = new();
    public List<Opponent> registeredOpponents = new();
    public OpponentGrid PlayerGrid { get; private set; }
    public OpponentGrid EnemyGrid { get; private set; }

    public int MinPlayers = 2;

    private GridManager gridManager;

    [Inject]
    public void Construct(GridManager gridManager) {
        this.gridManager = gridManager;
        gridManager.OnGridInitialized += HandleGridUpdate;
        gridManager.OnGridChanged += HandleGridUpdate;
    }

    #region Grid Opponent Logic
    public void AssignGrid(Opponent opponent) {
        if (opponentGrids.ContainsKey(opponent)) return;

        OpponentGrid grid = opponent is Player ? PlayerGrid : opponent is Enemy ? EnemyGrid : null;

        if (grid == null) {
            Debug.LogError("Wrong opponent to assign");
        }
        grid.AssignGridToOwner(opponent);

        opponentGrids.Add(opponent, grid);
        Debug.Log($"Grid assigned to opponent {opponent.Name}.");
    }


    public OpponentGrid GetGrid(Opponent opponent) {
        opponentGrids.TryGetValue(opponent, out OpponentGrid grid);
        return grid;
    }

    private void HandleGridUpdate(GridUpdateData updateData) {
        Grid mainGrid = gridManager.MainGrid;
        GridSettings gridSettings = mainGrid.GetConfig();

        int attackRowIndex = gridSettings.RowTypes.FindIndex(row => row == FieldType.Attack);
        if (attackRowIndex == -1) {
            Debug.LogError("No 'Attack' row found in GridSettings.RowTypes.");
            return;
        }

        int totalRows = gridSettings.RowTypes.Count;

        PlayerGrid = PlayerGrid == null
            ? new OpponentGrid(mainGrid, 0, attackRowIndex)
            : PlayerGrid.BoundToMainGrid(mainGrid, 0, attackRowIndex);

        EnemyGrid = EnemyGrid == null
            ? new OpponentGrid(mainGrid, attackRowIndex + 1, totalRows - 1)
            : EnemyGrid.BoundToMainGrid(mainGrid, attackRowIndex + 1, totalRows - 1);

        foreach (Opponent opponent in registeredOpponents) {
            bool isPlayer = opponent is Player;

            List<Field> addedFields = updateData.addedFields
                .Where(field => isPlayer ?
                    field.row <= attackRowIndex :
                    field.row > attackRowIndex)
                .ToList();

            List<Field> removedFields = updateData.removedFields
                .Where(field => isPlayer ?
                    field.row <= attackRowIndex :
                    field.row > attackRowIndex)
                .ToList();

            OpponentGrid opponentGrid = GetGrid(opponent);
            if (opponentGrid != null) {
                opponentGrid.AddFields(addedFields);
                opponentGrid.RemoveFields(removedFields);
            }
        }
    }
    #endregion

    #region Board Opponent Logic
    public bool IsAllRegistered() => registeredOpponents.Count >= MinPlayers;

    public void RegisterOpponent(Opponent opponent) {
        if (registeredOpponents.Contains(opponent)) return;

        if (registeredOpponents.Count >= MinPlayers) {
            Debug.LogWarning("Cannot register more opponents. Minimum players reached.");
            return;
        }

        registeredOpponents.Add(opponent);
        AssignGrid(opponent);
        opponent.OnDefeat += UnregisterOpponent;
        Debug.Log($"Opponent {opponent.Name} registered.");
    }

    public void UnregisterOpponent(Opponent opponent) {
        // Try to find and unregister
        if (!registeredOpponents.Remove(opponent)) return;

        // Try to find and unassign
        OpponentGrid opponentGrid = GetGrid(opponent);
        if (opponentGrid != null) {
            opponentGrid.UnassignGridOwner(opponent);
        }
        Debug.Log($"Opponent {opponent.Name} unregistered.");
    }

    public Opponent GetNextOpponent(Opponent current) {
        if (!registeredOpponents.Any()) {
            throw new InvalidOperationException("No registered opponents available.");
        }

        int currentIndex = registeredOpponents.IndexOf(current);
        return registeredOpponents[(currentIndex + 1) % registeredOpponents.Count];
    }

    public Opponent GetRandomOpponent() {
        if (!registeredOpponents.Any()) {
            throw new InvalidOperationException("No registered opponents available.");
        }

        return registeredOpponents[UnityEngine.Random.Range(0, registeredOpponents.Count)];
    }
    #endregion
}
