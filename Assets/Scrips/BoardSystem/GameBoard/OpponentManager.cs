using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OpponentManager {
    private BoardSettings boardSettings;
    private readonly Dictionary<Opponent, SubGrid> opponentGrids = new();
    public List<Opponent> registeredOpponents = new();

    private Grid mainGrid;
    public SubGrid PlayerGrid { get; private set; }
    public SubGrid EnemyGrid { get; private set; }
    public int MinPlayers { get; private set; }
    public Action<Field> OnFieldAssigned;
    public Action<Field> OnFieldUnassigned;

    public void UpdateSettings(BoardSettings boardSettings) {
        this.boardSettings = boardSettings;
        MinPlayers = boardSettings.minPlayers;
    }

    public void RegisterOpponent(Opponent opponent) {
        if (registeredOpponents.Contains(opponent)) return;

        if (registeredOpponents.Count >= MinPlayers) {
            Debug.LogWarning("Cannot register more opponents. Minimum players reached.");
            return;
        }

        registeredOpponents.Add(opponent);
        AssignGridToOpponent(opponent);
        opponent.OnDefeat += UnRegisterOpponent;
        Debug.Log($"Opponent {opponent.Name} registered.");
    }


    public void UnRegisterOpponent(Opponent opponent) {
        if (registeredOpponents.Contains(opponent)) {
            registeredOpponents.Remove(opponent);
            opponent.OnDefeat -= UnRegisterOpponent;
            Debug.Log($"Opponent {opponent.Name} unregistered.");
        }
    }

    private void UpdateFieldOwners(SubGrid grid, Opponent owner) {
        grid.Fields.ForEach(row => row.ForEach(field => field.AssignOwner(owner)));
    }

    public void AssignGridToOpponent(Opponent opponent) {
        var grid = GetGrid(opponent);
        UpdateFieldOwners(grid, opponent);

        if (!opponentGrids.ContainsKey(opponent)) {
            opponentGrids[opponent] = grid;
            opponent.OnDefeat += o => UnassignGrid(o);
        }

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
        if (opponent is Player) {
            return PlayerGrid ?? throw new InvalidOperationException("PlayerGrid is not assigned.");
        }

        if (opponent is Enemy) {
            return EnemyGrid ?? throw new InvalidOperationException("EnemyGrid is not assigned.");
        }

        throw new ArgumentException("Invalid opponent type.");
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

    private void AssignOwner(Field field) {
        Player player = registeredOpponents.OfType<Player>().FirstOrDefault();
        Enemy enemy = registeredOpponents.OfType<Enemy>().FirstOrDefault();

        if (player == null || enemy == null) {
            Debug.LogError("Cannot assign field ownership. Missing Player or Enemy.");
            return;
        }

        int divider = boardSettings.RowTypes.FindIndex(row => row == FieldType.Attack);
        field.AssignOwner(field.row <= divider ? player : enemy);
        OnFieldAssigned?.Invoke(field);
    }


    private void UnAssignOwner(Field field) {
        field.AssignOwner(null);
        OnFieldUnassigned?.Invoke(field);
    }

    private void UpdateGrids(Grid grid) {
        int rows = grid.Fields.Count;
        int divider = boardSettings.RowTypes.FindIndex(row => row == FieldType.Attack);

        // Оновлення прив'язки SubGrid для гравця та ворога
        PlayerGrid.BoundToMainGrid(grid, 0, divider);
        EnemyGrid.BoundToMainGrid(grid, divider + 1, rows - 1);

        // Оновлення власників для гравця
        Player player = registeredOpponents.OfType<Player>().FirstOrDefault();
        if (player != null) {
            foreach (var row in PlayerGrid.Fields) {
                foreach (var field in row) {
                    field.AssignOwner(player);
                }
            }
        }

        // Оновлення власників для ворога
        Enemy enemy = registeredOpponents.OfType<Enemy>().FirstOrDefault();
        if (enemy != null) {
            foreach (var row in EnemyGrid.Fields) {
                foreach (var field in row) {
                    field.AssignOwner(enemy);
                }
            }
        }
    }


    private void SetNewMainGrid(Grid grid) {
        mainGrid = grid;

        int rows = grid.Fields.Count;
        int divider = boardSettings.RowTypes.FindIndex(row => row == FieldType.Attack);
        PlayerGrid = new SubGrid(grid, 0, divider);
        EnemyGrid = new SubGrid(grid, divider + 1, rows - 1);
        SubscribeEvents();
    }

    public void SetGrid(Grid mainGrid, BoardSettings config) {
        boardSettings = config;
        if (this.mainGrid == null) {
            SetNewMainGrid(mainGrid);
            return;
        }

        if (this.mainGrid != mainGrid) {
            this.mainGrid = mainGrid;
            UnsubscribeAllEvents();
            SetNewMainGrid(mainGrid);
            return;
        }

        UpdateGrids(mainGrid);
    }

    private void SubscribeEvents() {
        mainGrid.OnAddField += AssignOwner;
        mainGrid.OnRemoveField += UnAssignOwner;
        mainGrid.OnGridChanged += UpdateGrids;
    }

    private void UnsubscribeAllEvents() {
        mainGrid.OnAddField -= AssignOwner;
        mainGrid.OnRemoveField -= UnAssignOwner;
        mainGrid.OnGridChanged -= UpdateGrids;
    }
}
