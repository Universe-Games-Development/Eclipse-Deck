using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CreatureNavigator {
    public GameContext GameContext { get; protected set; }
    public GameBoard GameBoard { get; protected set; }
    public BoardOverseer Overseer { get; protected set; }
    public Field CurrentField { get; protected set; }
    public Creature CurrentCreature { get; protected set; }
    public bool IsReversed { get; protected set; } = false;

    public CreatureNavigator(GameContext gameContext) {
        UpdateParams(gameContext);
    }

    public void UpdateParams(GameContext gameContext) {
        GameContext = gameContext;
        GameBoard = gameContext.gameBoard;
        Overseer = gameContext.overseer;
        CurrentField = gameContext.initialField;
        CurrentCreature = gameContext.currentCreature;
        IsReversed = Overseer.IsFieldInEnemyZone(CurrentField);
    }

    // Trying to move in the chosen direction
    // Return the amount of successful moves
    public async UniTask<int> TryMove(int moveAmount, Direction moveDirection) {
        int moves = 0;

        if (!ValidateInputs(GameBoard, Overseer, CurrentCreature, CurrentField)) {
            return moves;
        }

        IsReversed = Overseer.IsFieldInEnemyZone(CurrentField);
        List<Field> path = Overseer.GetMainGridPath(CurrentField, moveAmount, moveDirection, IsReversed);

        if (path.Count == 0) {
            Debug.LogWarning("No valid path found.");
            return moves;
        }

        foreach (var field in path) {
            if (field.OccupiedCreature != null) {
                Debug.LogWarning("Path is blocked by an ally at " + field.row + ", " + field.column);
                return moves; // Path is blocked
            }

            moves += await TryMoveToField(field);
        }

        return moves;
    }

    public async UniTask<int> TryMoveToField(Field field) {
        try {
            bool moveResult = await GameBoard.PlaceCreature(field, CurrentCreature);
            if (moveResult) {
                Debug.Log($"Moved to {field.row} / {field.column}");
                CurrentField.RemoveCreature();
                CurrentField = field;
                return 1;
            } else {
                Debug.LogWarning($"Failed to move to {field.row} / {field.column}. Field may be occupied or invalid.");
                return 0;
            }
        } catch (Exception ex) {
            Debug.LogError($"Error during movement to {field.row} / {field.column}: {ex.Message}");
            return 0;
        }
    }

    public List<Field> GetFieldsInDirection(int amount, Direction direction) {
        return Overseer.GetMainGridPath(CurrentField, amount, direction, IsReversed);
    }

    public List<Creature> GetCreaturesInDirection(int amount, Direction direction) {
        List<Field> path = GetFieldsInDirection(amount, direction);
        return GetCreaturesOnFields(path);
    }

    public List<Creature> GetCreaturesOnFields(List<Field> fields) {
        List<Creature> creaturesInDirection = new List<Creature>();
        foreach (var field in fields) {
            if (field.OccupiedCreature != null) {
                creaturesInDirection.Add(field.OccupiedCreature);
            }
        }
        return creaturesInDirection;
    }

    public List<Field> GetAdjacentFields() {
        return Overseer.GetAdjacentFields(CurrentField);
    }

    private static bool ValidateInputs(GameBoard gameBoard, BoardOverseer boardOverseer, Creature creatureToMove, Field currentField) {
        if (creatureToMove == null || currentField == null) {
            Debug.LogError("Current creature or field is null.");
            return false;
        }
        if (gameBoard == null || boardOverseer == null) {
            Debug.LogError("Board data missing");
            return false;
        }
        return true;
    }

    public static Direction GetOppositeDirection(Direction direction) {
        return CompasUtil.GetOppositeDirection(direction);
    }
}
