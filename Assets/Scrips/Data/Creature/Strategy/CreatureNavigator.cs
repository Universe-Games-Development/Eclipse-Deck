using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CreatureNavigator {
    // Zenject needed
    public GameBoard GameBoard { get; private set; }
    public GridBoard GridBoard { get; private set; }

    [Inject]
    public void Construct(GameBoard gameBoard) {
        GameBoard = gameBoard;
        GridBoard = gameBoard._boardUpdater.GridBoard;
    }

    // Trying to move in the chosen direction
    // Return the path to move
    public Path GenerateSimplePath(Field CurrentField, int moveAmount, Direction moveDirection) {
        Path path = new();
        if (!ValidateInputs(GameBoard, CurrentField)) {
            path.isInterrupted = true;
            path.interruptedAt = 0;
            return path;
        }

        bool isRelativeToEnemy = GridBoard.IsFieldBelogToDirection(CurrentField, Direction.North);
        if (isRelativeToEnemy) moveDirection = CompassUtil.GetOppositeDirection(moveDirection);

        List<Field> fieldsToMove = GridBoard.GetFieldsInDirection(CurrentField, moveAmount, moveDirection);
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

        // Результат
        path.fields = correctFields;
        return path;
    }

    public List<Field> GetFieldsInDirection(Field currentField, int amount, Direction direction) {

        return GridBoard.GetFieldsInDirection(currentField, amount, direction);
    }

    public List<Creature> GetCreaturesInDirection(Field currentField, int amount, Direction direction) {
        List<Field> path = GetFieldsInDirection(currentField, amount, direction);
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

    public List<Field> GetAdjacentFields(Field field) {
        return GridBoard.GetAdjacentFields(field);
    }

    public List<Field> GetFlankFields(Field field, int flankSize) {
        return GridBoard.GetFlankFields(field, flankSize);
    }

    private bool ValidateInputs(GameBoard gameBoard, Field currentField) {
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

    public Direction GetOppositeDirection(Direction direction) {
        return CompassUtil.GetOppositeDirection(direction);
    }

    public Direction GetDirectionToField(Field currentField, Field fieldToEscape) {
        int currentRow = currentField.row;
        int currentColumn = currentField.column;

        int targetRow = fieldToEscape.row;
        int targetColumn = fieldToEscape.column;

        int rowDifference = targetRow - currentRow;
        int columnDifference = targetColumn - currentColumn;

        return CompassUtil.GetDirectionFromOffset(rowDifference, columnDifference);
    }

}
