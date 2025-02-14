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

    public bool IsValidFieldSelected(Field field) {
        if (!IsInitialized()) {
            Debug.LogWarning("Gameboard not initialized! Can't select field");
            return false;
        }

        if (!_boardUpdater.GridBoard.FieldExists(field)) {
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
