using UnityEngine;
using Zenject;

public class TableManager : MonoBehaviour {
    [SerializeField] private GameObject battleCreaturePrefab;
    [SerializeField] private Canvas tableCanvas;

    [SerializeField] private GameObject fieldPrefab;
    [SerializeField] private Transform fieldsOrigin;
    [Header("Grid Settings")]
    [SerializeField] private int numberOfColumns = 7; // Кількість стовпців (кожен гравець має свій ряд)
    [SerializeField] private int numberOfRows = 2; // Кількість рядів для полів
    [SerializeField] private float verticalOffset = 2f; // Вертикальне зміщення між полями
    [SerializeField] private float horizontalOffset = 2f; // Горизонтальне зміщення між полями
    [SerializeField] private Vector3 fieldSize = new Vector3(1.5f, 0.1f, 1.5f); // Розмір поля

    private Field[,] fieldGrid;

    [Inject] private DiContainer diContainer;
    [Inject] private UIManager uiManager;

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
        GameObject newFieldObj = diContainer.InstantiatePrefab(fieldPrefab, spawnPosition, fieldsOrigin.rotation, fieldsOrigin);

        Field createdField = newFieldObj.GetComponent<Field>();


        createdField.Index = col + 1;

        return createdField;
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

    public bool SummonCreature(Opponent player, Card card, Field field) {
        Field gridField = GetFieldFromGrid(field);
        if (gridField == null) {
            Debug.LogError("Поле не знайдено в grid!");
            return false;
        }

        if (gridField.Owner == player) {
            if (gridField.OccupiedCreature != null) {
                Debug.Log($"{name} вже зайняте");
                return false; // Поле вже зайняте
            }

            GameObject battleCreatureObj = diContainer.InstantiatePrefab(battleCreaturePrefab, gridField.spawnPoint);
            BattleCreature battleCreature = battleCreatureObj.GetComponent<BattleCreature>();
            battleCreature.Initialize(card, new SingleAttack(), field);
            uiManager.CreateCreatureUI(battleCreature, card);

            gridField.AssignCreature(battleCreature);
            return true;
        } else {
            Debug.LogWarning("Це поле належить іншому гравцеві! Не можна зіграти карту тут.");
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
