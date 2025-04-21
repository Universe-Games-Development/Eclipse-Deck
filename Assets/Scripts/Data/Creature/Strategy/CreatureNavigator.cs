using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CreatureNavigator {
    // Zenject needed
    public GridBoard GridBoard { get; private set; }
    private BoardSystem _boardPresenter;
    
    public CreatureNavigator(BoardSystem boardPresenter) {
        _boardPresenter = boardPresenter;
    }

    // Trying to move in the chosen direction
    // Return the path to move
    public FieldsPath GenerateSimplePath(Field CurrentField, int moveAmount, Direction moveDirection) {
        FieldsPath path = new();
        if (!ValidateInputs(CurrentField)) {
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


        // Not used
        List<Field> correctFields = new() {
            CurrentField
        };
        for (int i = 0; i < fieldsToMove.Count; i++) {
            if (fieldsToMove[i].OccupyingCreature != null) {
                path.isInterrupted = true;
                path.interruptedAt = i;
                break;
            }
            if (fieldsToMove[i] != CurrentField) {
                correctFields.Add(fieldsToMove[i]);
            }
        }

        // Результат
        path.fields = fieldsToMove;
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
            if (field.OccupyingCreature != null) {
                creaturesInDirection.Add(field.OccupyingCreature);
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

    private bool ValidateInputs(Field currentField) {
        if (currentField == null) {
            Debug.LogError("Current field is null.");
            return false;
        }
        if (GridBoard == null) {
            Debug.LogError("Board data missing");
            return false;
        }
        return true;
    }

    public Direction GetOppositeDirection(Direction direction) {
        return CompassUtil.GetOppositeDirection(direction);
    }

    public Direction GetDirectionToField(Field currentField, Field fieldToEscape) {
        int currentRow = currentField.Row;
        int currentColumn = currentField.Column;

        int targetRow = fieldToEscape.Row;
        int targetColumn = fieldToEscape.Column;

        int rowDifference = targetRow - currentRow;
        int columnDifference = targetColumn - currentColumn;

        return CompassUtil.GetDirectionFromOffset(rowDifference, columnDifference);
    }

}
