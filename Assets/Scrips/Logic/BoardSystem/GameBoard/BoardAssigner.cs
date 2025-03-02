using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

public class BoardAssigner {
    private const Direction ENEMY_DIRECTION = Direction.North;
    private const Direction PLAYER_DIRECTION = Direction.South;
    private readonly Dictionary<Type, Direction> _opponentDirections = new()
         {
             { typeof(Player), PLAYER_DIRECTION },
             { typeof(Enemy), ENEMY_DIRECTION }
         };

    private readonly GameboardBuilder boardManager;

    private readonly OpponentRegistrator _opponentRegister;

    [Inject]
    public BoardAssigner(OpponentRegistrator opponentRegister, GameboardBuilder boardManager) {
        _opponentRegister = opponentRegister;
        this.boardManager = boardManager;

        SubscribeGridUpdates(); // temporary soon be bounded to game start event
    }

    private async UniTask HandleGridUpdate(BoardUpdateData data) {
        HandleBoardUpdate(data.GetUpdateByGlobalDirection(ENEMY_DIRECTION), _opponentRegister.GetEnemy());
        HandleBoardUpdate(data.GetUpdateByGlobalDirection(PLAYER_DIRECTION), _opponentRegister.GetPlayer());
        await UniTask.Yield();
    }

    private void HandleBoardUpdate(GridUpdateData boardUpdateData, Opponent opponent) {
        if (opponent == null) return;

        // Use null-conditional operator and LINQ for conciseness and safety
        boardUpdateData?.addedFields?.ForEach(field => field.AssignOwner(opponent));
        boardUpdateData?.removedFields?.ForEach(field => field.UnassignOwner());
    }

    public List<Creature> GetOpponentCreatures(Opponent endTurnOpponent) {
        List<Creature> creatures = new();
        foreach (var grid in GetOpponentGrids(endTurnOpponent)) {
            foreach (var row in grid.Fields) {
                foreach (var field in row) {
                    if (field != null && field.OccupiedCreature != null) {
                        creatures.Add(field.OccupiedCreature);
                    }
                }
            }
        }

        bool isPlayer = endTurnOpponent is Player;
        creatures.Sort((creatureA, creatureB) => {
            // Start comparing by row
            int rowA = creatureA.CurrentField.GetRow();
            int rowB = creatureB.CurrentField.GetRow();

            int rowComparison = isPlayer ? rowA.CompareTo(rowB) : rowB.CompareTo(rowA);
            if (rowComparison != 0) {
                return rowComparison;
            }
            // If rows are same compare by column
            int columnA = creatureA.CurrentField.GetColumn();
            int columnB = creatureB.CurrentField.GetColumn();
            return columnA.CompareTo(columnB);
        });
        return creatures;
    }

    public List<CompasGrid> GetOpponentGrids(Opponent opponent) {
        if (boardManager.GridBoard == null) {
            Debug.LogError("GridBoard not initialized during assignment");
            return new List<CompasGrid>();
        }

        if (_opponentDirections.TryGetValue(opponent.GetType(), out var direction)) {
            return boardManager.GridBoard.GetGridsByGlobalDirection(direction);
        } else {
            throw new ArgumentException($"Received an unexpected opponent type: {opponent.GetType()}");
        }
    }

    // When battle ends
    public void UnassignGrids() {
        // Use LINQ for more concise code
        boardManager.GridBoard?.GetAllGrids()?.ForEach(grid => grid.Fields?.ForEach(row => row?.ForEach(field => field.UnassignOwner())));
    }

    // When activated
    public void ReAssignGrids() {
        // Use LINQ and flatten the grid structure for more efficient iteration
        foreach (Opponent opponent in _opponentRegister.GetActiveOpponents()) {
            GetOpponentGrids(opponent).ForEach(grid => grid.Fields.SelectMany(row => row).ToList().ForEach(field => field.AssignOwner(opponent)));
        }
    }

    // When battle starts
    private void SubscribeGridUpdates() {
        boardManager.OnGridInitialized += HandleGridUpdate;
        boardManager.OnBoardChanged += HandleGridUpdate;
    }

    // When battle ends
    public void UnsubscribeGridUpdates() {
        boardManager.OnGridInitialized -= HandleGridUpdate;
        boardManager.OnBoardChanged -= HandleGridUpdate;
    }
}