using UnityEngine;

public class TableManager : MonoBehaviour {
    [SerializeField] private GameObject fieldPrefab;
    [SerializeField] private Transform fieldsOrigin;
    [Header("Grid Settings")]
    [SerializeField] private int numberOfColumns = 7; // ʳ������ �������� (����� ������� �� ��� ���)
    [SerializeField] private int numberOfRows = 2; // ʳ������ ���� ��� ����
    [SerializeField] private float verticalOffset = 2f; // ����������� ������� �� ������
    [SerializeField] private float horizontalOffset = 2f; // ������������� ������� �� ������
    [SerializeField] private Vector3 fieldSize = new Vector3(1.5f, 0.1f, 1.5f); // ����� ����

    private Field[,] fieldGrid;

    private void Awake() {
        if (fieldGrid == null) {
            GenerateGrid();
        }
    }

    private void GenerateGrid() {
        fieldGrid = new Field[numberOfRows, numberOfColumns];

        for (int row = 0; row < numberOfRows; row++) {
            for (int col = 0; col < numberOfColumns; col++) {
                fieldGrid[row, col] = CreateField(row, col);
            }
        }
    }

    private Field CreateField(int row, int col) {
        Vector3 offset = new Vector3(col * horizontalOffset, 0, row * verticalOffset);
        Vector3 spawnPosition = fieldsOrigin.position + fieldsOrigin.rotation * offset;
        GameObject newFieldObj = Instantiate(fieldPrefab, spawnPosition, Quaternion.identity);
        newFieldObj.transform.rotation = fieldsOrigin.rotation; // ���������� ��������
        return newFieldObj.GetComponent<Field>();
    }

    public void AssignFieldsToPlayer(Opponent player, int rowIndex) {
        if (fieldGrid == null) {
            GenerateGrid();
        }
        if (rowIndex < 0 || rowIndex >= numberOfRows) {
            Debug.LogError("Invalid row index!");
            return;
        }

        for (int col = 0; col < numberOfColumns; col++) {
            Field field = fieldGrid[rowIndex, col];
            field.AssignOwner(player);
            field.SetFieldOwnerIndicator(player);
        }

        Debug.Log($"Row {rowIndex} has been assigned to {player.Name}.");
    }

    public bool SummonCreature(Opponent player, Card selectedCard, Field field) {
        Field gridField = GetFieldFromGrid(field);
        if (gridField == null) {
            Debug.LogError("���� �� �������� � grid!");
            return false;
        }

        if (gridField.Owner == player) {
            gridField.SummonCreature(selectedCard);
            Debug.Log($"{player.Name} �������� �������� �� ���.");
            return true;
        } else {
            Debug.LogError("�� ���� �������� ������ �������! �� ����� ������ ����� ���.");
            return false;
        }
    }

    private Field GetFieldFromGrid(Field field) {
        for (int row = 0; row < fieldGrid.GetLength(0); row++) {
            for (int col = 0; col < fieldGrid.GetLength(1); col++) {
                if (fieldGrid[row, col] == field) {
                    return fieldGrid[row, col];
                }
            }
        }
        return null;
    }

    private void OnDrawGizmos() {
        if (fieldsOrigin == null) return;

        Gizmos.color = Color.yellow;
        for (int row = 0; row < numberOfRows; row++) {
            for (int col = 0; col < numberOfColumns; col++) {
                Vector3 offset = new Vector3(col * horizontalOffset, 0, row * verticalOffset);
                Vector3 spawnPosition = fieldsOrigin.position + fieldsOrigin.rotation * offset;
                Gizmos.DrawWireCube(spawnPosition, fieldSize);
            }
        }
    }
}