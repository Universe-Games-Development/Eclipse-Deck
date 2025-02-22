using UnityEngine;

public class GameBoard {
    // Zenject
    public GameBoardManager boardManager { get; private set; }
    private TurnManager _turnManager;

    public GameBoard(GameBoardManager boardManager, TurnManager turnManager) {
        this.boardManager = boardManager;
        _turnManager = turnManager;
    }

    public bool IsValidFieldSelected(Field field) {
        if (!IsInitialized()) {
            Debug.LogWarning("Gameboard not initialized! Can't select field");
            return false;
        }

        if (!boardManager.GridBoard.FieldExists(field)) {
            return false;
        }

        if (_turnManager.ActiveOpponent != field.Owner) {
            Debug.LogWarning("Field does not belong to the current player.");
            return false;
        }

        return true;
    }

    public bool IsInitialized() {
        if (boardManager.GridBoard == null || boardManager.GridBoard.Config == null) {
            Debug.LogWarning("GridManager is not properly initialized: Global grid is null or empty.");
            return false;
        }

        return true;
    }
}
