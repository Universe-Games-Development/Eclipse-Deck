using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class BoardAssigner {
    private const Direction ENEMY_DIRECTION = Direction.North;
    private const Direction PLAYER_DIRECTION = Direction.South;
    private readonly Dictionary<Type, Direction> _opponentDirections = new()
         {
             { typeof(Player), PLAYER_DIRECTION },
             { typeof(Enemy), ENEMY_DIRECTION }
         };

    private readonly BoardUpdater _boardUpdater;

    private readonly OpponentRegistrator _opponentRegister;

    [Inject]
    public BoardAssigner(OpponentRegistrator opponentRegister, BoardUpdater boardUpdater) {
        _opponentRegister = opponentRegister;
        _boardUpdater = boardUpdater;

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

    public List<Creature> GetOpponentCreatures(Opponent activeOpponent) {
        return GetOpponentGrids(activeOpponent)
            .SelectMany(grid => grid.Fields)
            .Where(row => row != null)
            .SelectMany(row => row)
            .Where(field => field?.OccupiedCreature != null)
            .Select(field => field.OccupiedCreature)
            .ToList();
    }

    public List<CompasGrid> GetOpponentGrids(Opponent opponent) {
        if (_boardUpdater.GridBoard == null) {
            Debug.LogError("GridBoard not initialized during assignment");
            return new List<CompasGrid>();
        }

        if (_opponentDirections.TryGetValue(opponent.GetType(), out var direction)) {
            return _boardUpdater.GridBoard.GetGridsByGlobalDirection(direction);
        } else {
            throw new ArgumentException($"Received an unexpected opponent type: {opponent.GetType()}");
        }
    }

    // When battle ends
    public void UnassignGrids() {
        // Use LINQ for more concise code
        _boardUpdater.GridBoard?.GetAllGrids()?.ForEach(grid => grid.Fields?.ForEach(row => row?.ForEach(field => field.UnassignOwner())));
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
        _boardUpdater.OnGridInitialized += HandleGridUpdate;
        _boardUpdater.OnBoardChanged += HandleGridUpdate;
    }

    // When battle ends
    public void UnsubscribeGridUpdates() {
        _boardUpdater.OnGridInitialized -= HandleGridUpdate;
        _boardUpdater.OnBoardChanged -= HandleGridUpdate;
    }
}