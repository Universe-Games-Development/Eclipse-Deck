using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

public class OpponentManager {
    private const Direction ENEMY_DIRECTION = Direction.North;
    private const Direction PLAYER_DIRECTION = Direction.South;

    private readonly Dictionary<Opponent, List<CompasGrid>> asiignedGrids = new();
    public List<Opponent> registeredOpponents = new();

    public int MinPlayers = 2;

    private GridManager gridManager;

    [Inject]
    public void Construct(GridManager gridManager) {
        this.gridManager = gridManager;
        gridManager.OnGridInitialized += HandleGridUpdate;
        gridManager.OnGridChanged += HandleGridUpdate;
    }

    // If opponents registered assign all fields to all opponents
    // 
    private void HandleGridUpdate(BoardUpdateData data) {
        UpdateBoard(data.GetUpdateByGlobalDirection(ENEMY_DIRECTION), typeof(Enemy));
        UpdateBoard(data.GetUpdateByGlobalDirection(PLAYER_DIRECTION), typeof(Player));
    }

    private void UpdateBoard(GridUpdateData boardUpdateData, Type opponentType) {
        var opponent = registeredOpponents.Find(opponent => opponent.GetType() == opponentType);

        if (opponent != null) {
            foreach (var field in boardUpdateData.addedFields) {
                field.AssignOwner(opponent);
            }

            foreach (var field in boardUpdateData.removedFields) {
                field.UnassignOwner();
            }
        }
    }


    #region Grid Opponent Logic
    public void AssignGrid(Opponent opponent) {
        if (asiignedGrids.ContainsKey(opponent)) return;

        List<CompasGrid> opponentGrids = GetOpponentGrids(opponent);
        foreach (CompasGrid grid in opponentGrids) {
            foreach(var row in grid._fields) {
                foreach (Field field in row) {
                    field.AssignOwner(opponent);
                }
            }
        }

        asiignedGrids.Add(opponent, opponentGrids);
        Debug.Log($"Grid assigned to opponent {opponent.Name}.");
    }

    private List<CompasGrid> GetOpponentGrids(Opponent opponent) {
        if (gridManager.GridBoard == null) {
            Debug.LogError("GridBoard not initialized during assignment");
        }

        if (opponent is Player) {
            return gridManager.GridBoard.GetGridsByGlobalDirection(Direction.South);
        } else if (opponent is Enemy) {
            return gridManager.GridBoard.GetGridsByGlobalDirection(Direction.North);
        } else {
            throw new ArgumentException("Received wron opponent to get compas grid");
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
        if (asiignedGrids.TryGetValue(opponent, out List<CompasGrid> opponentGrids)) {
            foreach (CompasGrid grid in opponentGrids) {
                foreach (var row in grid._fields) {
                    foreach (Field field in row) {
                        field.UnassignOwner();
                    }
                }
            }
            Debug.Log($"Opponent {opponent.Name} unregistered.");
        }
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

    internal List<List<Field>> GetOpponentBoard(Opponent currentPlayer) {
        throw new NotImplementedException();
    }
    #endregion
}
