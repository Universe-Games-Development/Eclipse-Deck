using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementStrategy : ScriptableObject {
    public Sprite moveStrategyIcon;
    [SerializeField] protected int supportMoveAmount = 0;
    [SerializeField] protected int atteckMoveAmount = 0;
    [SerializeField] protected Direction supportMoveDirection = Direction.North;
    [SerializeField] protected Direction attackMoveDirection = Direction.South;

    protected abstract UniTask<int> SupportFieldMovement(GameBoard gameBoard, BoardOverseer boardOverseer, Creature creatureToMove, Field currentField, bool reversedDirection);
    protected abstract UniTask<int> AttackFieldMovement(GameBoard gameBoard, BoardOverseer boardOverseer, Creature creatureToMove, Field currentField, bool reversedDirection);

    protected async UniTask<bool> Move(GameContext gameContext) {
        GameBoard gameBoard = gameContext.gameBoard;
        BoardOverseer boardOverseer = gameContext.overseer;
        Creature creatureToMove = gameContext.currentCreature;
        Field currentField = gameContext.currentField;

        ValidateInputs(gameBoard, boardOverseer, creatureToMove, currentField);

        bool reversedDirection = currentField.Owner is Enemy;

        int moves = 0;
        if (currentField.Type == FieldType.Attack) {
            moves += await AttackFieldMovement(gameBoard, boardOverseer, creatureToMove, currentField, reversedDirection);
        } else if (currentField.Type == FieldType.Support) { // Виправлено перевірку на підтримуюче поле
            moves += await SupportFieldMovement(gameBoard, boardOverseer, creatureToMove, currentField, reversedDirection);
        } else {
            Debug.LogError("Wrong field type to move creature.");
        }

        Debug.Log("Creture moved by " + moves + " fields");
        return moves > 0;
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
