using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class FieldManager : MonoBehaviour {
    private Field[,] fieldGrid;

    [Header("Board Configuration")]
    [SerializeField] private BoardConfig boardConfig;
    [SerializeField] private GameObject fieldPrefab;
    private Mesh fieldMesh;
    [Inject] private DiContainer diContainer;

    [Header("Grid Center")]
    [SerializeField] private Transform boardCenter;

    public void GenerateGrid() {
        if (!ValidateGridGeneration()) return;

        if (boardCenter == null) {
            Debug.LogError("boardCenter is not assigned.");
            return;
        }

        if (fieldPrefab != null && fieldMesh == null) {
            fieldMesh = fieldPrefab.GetComponentInChildren<MeshFilter>().sharedMesh;
        } else {
            Debug.LogError("No mesh found on fieldPrefab.");
            return;
        }

        Vector3 fieldSize = fieldMesh.bounds.size;
        float horizontalOffset = fieldSize.x;
        float verticalOffset = fieldSize.z;

        int numberOfRows = boardConfig.FieldTypeFormat.Length;
        int numberOfColumns = boardConfig.NumberOfColumns;

        fieldGrid = new Field[numberOfRows, numberOfColumns];

        Vector3 gridCenter = boardCenter.position;

        float totalWidth = horizontalOffset * numberOfColumns;
        float totalHeight = verticalOffset * numberOfRows;

        Vector3 gridStartPosition = gridCenter - new Vector3(totalWidth / 2, 0, totalHeight / 2);

        bool isPlayerField = true;

        for (int row = 0; row < numberOfRows; row++) {
            FieldType rowType = boardConfig.FieldTypeFormat[row];

            for (int col = 0; col < numberOfColumns; col++) {
                fieldGrid[row, col] = CreateField(row, col, rowType, isPlayerField, horizontalOffset, verticalOffset, gridStartPosition);
            }

            if (rowType == FieldType.Attack && isPlayerField) {
                isPlayerField = false;
            }
        }
    }

    private Field CreateField(int row, int col, FieldType type, bool isPlayerField, float horizontalOffset, float verticalOffset, Vector3 gridStartPosition) {
        // Обчислюємо позицію поля, розташовану з урахуванням центральної точки
        Vector3 offset = new Vector3(col * horizontalOffset, 0, row * verticalOffset);
        Vector3 spawnPosition = gridStartPosition + offset; // Використовуємо центр сітки

        GameObject newFieldObj = diContainer.InstantiatePrefab(fieldPrefab, spawnPosition, boardCenter.rotation, boardCenter);

        Field createdField = newFieldObj.GetComponent<Field>();
        createdField.Index = col + 1;
        createdField.Type = type;
        createdField.IsPlayerField = isPlayerField;
        createdField.gameObject.name = (isPlayerField ? "Player " : "Enemy") + " Field " + $"{row} / {col}";

        return createdField;
    }

    public Field[,] GetFieldGrid() {
        return fieldGrid;
    }

    public void AssignFieldsToOpponent(Opponent opponent) {
        if (fieldGrid == null) {
            Debug.LogWarning("Field grid has not been generated. Cannot assign fields.");
            return;
        }

        foreach (var field in GetFieldsForOpponent(opponent)) {
            field.AssignOwner(opponent);
        }

        Debug.Log($"{(opponent is Player ? "Player" : "Enemy")} fields have been assigned to {opponent.Name ?? "Unnamed Opponent"}.");
    }

    private IEnumerable<Field> GetFieldsForOpponent(Opponent opponent) {
        bool isPlayer = opponent is Player;
        foreach (var field in fieldGrid) {
            if (field.IsPlayerField == isPlayer) {
                yield return field;
            }
        }
    }

    private bool ValidateGridGeneration() {
        if (boardConfig == null) {
            Debug.LogError("BoardConfig is not assigned.");
            return false;
        }

        if (fieldPrefab == null) {
            Debug.LogError("FieldPrefab is not assigned.");
            return false;
        }

        if (boardConfig == null) {
            Debug.LogError("BoardConfig is not initialized.");
            return false;
        }

        if (!ValidateBoardFormat()) {
            Debug.LogError("Incorrect field format. 2 Attack fields needed.");
            return false;
        }
        return true;
    }

    public Field ValidateSummon(Opponent player, Field field) {
        if (field == null) {
            Debug.LogError("Field is not found in grid!");
            return null;
        }

        if (field.Owner != player) {
            Debug.LogWarning("This field belongs to another player! You can't play a card here.");
            return null;
        }

        if (field.OccupiedCreature != null) {
            Debug.Log($"{field.name} is already occupied");
            return null;
        }

        return field;
    }

    private bool ValidateBoardFormat() {
        if (boardConfig.FieldTypeFormat == null) {
            Debug.Log("Board format not set.");
            return false;
        }

        int attackRowCount = 0;

        foreach (var type in boardConfig.FieldTypeFormat) {
            if (type == FieldType.Attack) {
                attackRowCount++;
            }
        }

        return attackRowCount == 2;
    }

    public Field GetNeighboringField(Field field, Direction direction) {
        int rowIndex = field.Index / boardConfig.NumberOfColumns;
        int colIndex = field.Index % boardConfig.NumberOfColumns;

        switch (direction) {
            case Direction.Up:
                return GetField(rowIndex - 1, colIndex);
            case Direction.Down:
                return GetField(rowIndex + 1, colIndex);
            case Direction.Left:
                return GetField(rowIndex, colIndex - 1);
            case Direction.Right:
                return GetField(rowIndex, colIndex + 1);
            default:
                return null;
        }
    }

    private Field GetField(int rowIndex, int colIndex) {
        if (rowIndex >= 0 && rowIndex < fieldGrid.GetLength(0) && colIndex >= 0 && colIndex < fieldGrid.GetLength(1)) {
            return fieldGrid[rowIndex, colIndex];
        }
        return null;
    }

    private void OnDrawGizmos() {
        if (boardConfig == null || boardConfig.FieldsOrigin == null || boardConfig.FieldTypeFormat == null) return;

        // Check for null boardCenter
        if (boardCenter == null) {
            Debug.LogError("boardCenter is not assigned.");
            return;
        }

        Gizmos.color = Color.yellow;

        int numberOfRows = boardConfig.FieldTypeFormat.Length;
        int numberOfColumns = boardConfig.NumberOfColumns;

        float totalWidth = numberOfColumns * boardConfig.HorizontalOffset;
        float totalHeight = numberOfRows * boardConfig.VerticalOffset;

        Vector3 gridCenter = boardCenter.position;
        Vector3 gridStartPosition = gridCenter - new Vector3(totalWidth / 2, 0, totalHeight / 2);

        for (int row = 0; row < numberOfRows; row++) {
            for (int col = 0; col < numberOfColumns; col++) {
                Vector3 offset = new Vector3(col * boardConfig.HorizontalOffset, 0, row * boardConfig.VerticalOffset);
                Vector3 fieldPosition = gridStartPosition + offset;
                Gizmos.DrawWireCube(fieldPosition, new Vector3(boardConfig.HorizontalOffset, -0.6f, boardConfig.VerticalOffset));
            }
        }
    }

}

public enum Direction {
    Up,
    Down,
    Left,
    Right
}
