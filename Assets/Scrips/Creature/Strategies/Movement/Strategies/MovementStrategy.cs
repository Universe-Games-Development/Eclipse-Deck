using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementStrategy {

    protected GameBoard gameBoard;
    protected BoardOverseer boardOverseer;
    protected Creature creatureToMove;
    protected CreatureMovementDataSO data;
    protected Field currentField;
    public MovementStrategy(CreatureMovementDataSO data) {
        this.data = data;
    }

    protected abstract UniTask<int> Movement(GameContext gameContext, bool reversedDirection);

    public async UniTask<int> Move(GameContext gameContext) {
        gameBoard = gameContext.gameBoard;
        boardOverseer = gameContext.overseer;
        creatureToMove = gameContext.currentCreature;
        currentField = gameContext.initialField;

        ValidateInputs(gameBoard, boardOverseer, creatureToMove, gameContext.initialField);
        bool reversedDirection = gameContext.initialField.Owner is Enemy;

        return await Movement(gameContext, reversedDirection);
    }

    // Trying to move in the chosen direction
    // Return the amount of successful moves
    protected async UniTask<int> TryMove(GameBoard gameBoard, Creature creatureToMove, List<Field> path) {
        int movement = 0;

        if (path.Count == 0) {
            Debug.LogWarning("No valid path found.");
            return movement;
        }

        foreach (var field in path) {
            if (field.OccupiedCreature != null) {
                Debug.LogWarning("Path is blocked by an ally at " + field.row + ", " + field.column);
                return movement;  // Path is blocked, return the number of moves made
            }
            try {
                bool moveResult = await gameBoard.PlaceCreature(field, creatureToMove);
                if (moveResult) {
                    Debug.Log("Creature moved to field " + field.row + " / " + field.column);
                    currentField.RemoveCreature();
                    currentField = field;
                    movement++;
                } else {
                    Debug.LogWarning("Failed to move creature to field " + field.row + " / " + field.column);
                }
            } catch (Exception ex) {
                Debug.LogError($"Error while trying to move creature: {ex.Message}");
            }
        }

        return movement;
    }

    private bool ValidateInputs(GameBoard gameBoard, BoardOverseer boardOverseer, Creature creatureToMove, Field currentField) {
        if (creatureToMove == null || currentField == null) {
            Debug.LogError("Current creature or field is null.");
            return false;
        }
        if (gameBoard == null || boardOverseer == null) {
            Debug.Log("Board data missing");
            return false;
        }
        return true;
    }

}
