using Cysharp.Threading.Tasks;
using FMODUnity;
using System;
using System.Threading;
using UnityEngine;

public class GameBoard {
    // Zenject
    public BoardUpdater _boardUpdater { get; private set; }
    private TurnManager _turnManager;

    public GameBoard(BoardUpdater boardUpdater, TurnManager turnManager) {
        _boardUpdater = boardUpdater;
        _turnManager = turnManager;
    }

    public bool SummonCreature(Opponent opponent, Field field, Creature creature) {
        if (!IsValidSummon(opponent, field, creature)) return false;

        bool result = field.PlaceCreature(creature);
        Debug.Log(result
            ? $"Creature successfully placed at {field.GetTextCoordinates()}"
            : $"Failed to place creature at {field.row}, {field.column}.");
        return result;
    }

    private bool IsValidSummon(Opponent opponent, Field field, Creature creature) {
        if (field == null || creature == null) {
            Debug.LogWarning("Invalid summon attempt: Field or creature is null.");
            return false;
        }

        if (opponent != field.Owner) {
            Debug.LogWarning($"{opponent.Name} tried to summon on a field they don't own.");
            return false;
        }
        return true;
    }

    public bool IsValidFieldSelected(Field field) {
        if (!IsInitialized()) {
            Debug.LogWarning("Gameboard not initialized! Can't select field");
            return false;
        }

        if (!_boardUpdater.GridBoard.FieldExists(field)) {
            Debug.LogWarning("Field doesn’t exist! Gameboard can’t select: " + field);
            return false;
        }

        if (_turnManager.ActiveOpponent != field.Owner) {
            Debug.LogWarning("Field does not belong to the current player.");
            return false;
        }

        return true;
    }

    public bool IsInitialized() {
        if (_boardUpdater.GridBoard == null || _boardUpdater.GridBoard.Config == null) {
            Debug.LogWarning("GridManager is not properly initialized: Global grid is null or empty.");
            return false;
        }

        return true;
    }
}
