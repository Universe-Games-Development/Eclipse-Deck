using System.Collections.Generic;
using UnityEngine;

public class CreatureNavigator {
    // Zenject needed
    public GameBoard GameBoard { get; protected set; }
    // 
    public Grid mainGrid;
    public Grid playerGrid;
    public Grid enemyGrid;
    public GameContext GameContext { get; protected set; }
    public Field CurrentField { get; protected set; }
    public Creature CurrentCreature { get; protected set; }
    public bool IsRelativeToEnemy { get; protected set; } = false;
    public CreatureNavigator(GameContext gameContext) {
        UpdateParams(gameContext);
    }

    public void UpdateParams(GameContext gameContext) {
        GameContext = gameContext;
        GameBoard = gameContext.gameBoard;

        CurrentField = gameContext.initialField;
        CurrentCreature = gameContext.currentCreature;

        mainGrid = gameContext._gridManager.MainGrid;

        IsRelativeToEnemy = mainGrid.IsFieldInEnemyZone(CurrentField);
    }

    // Trying to move in the chosen direction
    // Return the amount of successful moves
    public Path GenerateSimplePath(int moveAmount, Direction moveDirection) {
        Path path = new();
        if (!ValidateInputs(GameBoard, CurrentField)) {
            path.isInterrupted = true;
            path.interruptedAt = 0;
            return path;
        }

        bool isRelativeToEnemy = mainGrid.IsFieldInEnemyZone(CurrentField);

        List<Field> fieldsToMove = mainGrid.GetFieldsInDirection(CurrentField, moveAmount, moveDirection, isRelativeToEnemy);
        if (fieldsToMove == null || fieldsToMove.Count == 0) {
            Debug.LogWarning("No valid fields to move.");
            path.isInterrupted = true;
            path.interruptedAt = 0;
            return path;
        }

        List<Field> correctFields = new() {
            CurrentField
        };
        for (int i = 0; i < fieldsToMove.Count; i++) {
            if (fieldsToMove[i].OccupiedCreature != null) {
                Debug.LogWarning($"Path is blocked by a creature at {fieldsToMove[i].row}, {fieldsToMove[i].column}");
                path.isInterrupted = true;
                path.interruptedAt = i;
                break;
            }
            correctFields.Add(fieldsToMove[i]);
        }

        // ���������
        path.fields = correctFields;
        return path;
    }



    public List<Field> GetFieldsInDirection(int amount, Direction direction) {
        return mainGrid.GetFieldsInDirection(CurrentField, amount, direction, IsRelativeToEnemy);
    }

    public List<Creature> GetCreaturesInDirection(int amount, Direction direction) {
        List<Field> path = GetFieldsInDirection(amount, direction);
        return GetCreaturesOnFields(path);
    }

    public List<Creature> GetCreaturesOnFields(List<Field> fields) {
        List<Creature> creaturesInDirection = new();
        foreach (var field in fields) {
            if (field.OccupiedCreature != null) {
                creaturesInDirection.Add(field.OccupiedCreature);
            }
        }
        return creaturesInDirection;
    }

    public List<Field> GetAdjacentFields() {
        return mainGrid.GetAdjacentFields(CurrentField);
    }

    private static bool ValidateInputs(GameBoard gameBoard, Field currentField) {
        if (currentField == null) {
            Debug.LogError("Current field is null.");
            return false;
        }
        if (gameBoard == null) {
            Debug.LogError("Board data missing");
            return false;
        }
        return true;
    }

    public List<Field> GetFlankFields(int flankSize) {
        return mainGrid.GetFlankFields(CurrentField, flankSize, IsRelativeToEnemy);
    }


    public static Direction GetOppositeDirection(Direction direction) {
        return CompassUtil.GetOppositeDirection(direction);
    }

    public Opponent GetCurrentOwner() {
        return CurrentField.Owner;
    }

    public Direction GetDirectionToField(Field fieldToEscape) {
        int currentRow = CurrentField.row;
        int currentColumn = CurrentField.column;

        int targetRow = fieldToEscape.row;
        int targetColumn = fieldToEscape.column;

        int rowDifference = targetRow - currentRow;
        int columnDifference = targetColumn - currentColumn;

        return CompassUtil.GetDirectionFromOffset(rowDifference, columnDifference);
    }

}
