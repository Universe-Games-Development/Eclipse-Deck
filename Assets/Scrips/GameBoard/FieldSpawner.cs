using System;
using UnityEngine;
using Zenject;

public class FieldSpawner : MonoBehaviour {
    [SerializeField] private GameObject fieldPrefab;
    [Header("Grid Settings")]
    [SerializeField] private Transform fieldsOrigin;
    [SerializeField] private float verticalOffset = 2f;
    [SerializeField] private float horizontalOffset = 2f;
    [SerializeField] private int numberOfColumns = 7;
    [SerializeField] private Vector3 fieldSize = new Vector3(1.5f, 0.1f, 1.5f);
    [SerializeField] private FieldType[] fieldTypeFormat; // Формат рядів

    private Field[,] fieldGrid;

    [Inject] private DiContainer diContainer;

    public void GenerateGrid() {

        if (!ValidateFieldFormat()) {
            Debug.LogError("Incorrect field format. 2 Attack fields needed");
            return;
        }

        int numberOfRows = fieldTypeFormat.Length;

        fieldGrid = new Field[numberOfRows, numberOfColumns];

        bool isPlayerField = true;

        for (int row = 0; row < numberOfRows; row++) {
            FieldType rowType = fieldTypeFormat[row];

            for (int col = 0; col < numberOfColumns; col++) {
                fieldGrid[row, col] = CreateField(row, col, rowType, isPlayerField);
            }

            if (rowType == FieldType.Attack && isPlayerField) {
                isPlayerField = false; 
            }
        }
    }


    private Field CreateField(int row, int col, FieldType type, bool isPlayerField) {
        Vector3 offset = new Vector3(col * horizontalOffset, 0, row * verticalOffset);
        Vector3 spawnPosition = fieldsOrigin.position + fieldsOrigin.rotation * offset;
        GameObject newFieldObj = diContainer.InstantiatePrefab(fieldPrefab, spawnPosition, fieldsOrigin.rotation, fieldsOrigin);

        Field createdField = newFieldObj.GetComponent<Field>();
        createdField.Index = col + 1;
        createdField.Type = type;
        createdField.IsPlayerField = isPlayerField;
        createdField.gameObject.name = (isPlayerField ? "Player " : "Enemy") + " Field " + $"{row} / {col}";

        return createdField;
    }

    private bool ValidateFieldFormat() {
        int attackRowCount = 0;

        foreach (var type in fieldTypeFormat) {
            if (type == FieldType.Attack) {
                attackRowCount++;
            }
        }

        return attackRowCount == 2;
    }

    public Field[,] GetFieldGrid() {
        return fieldGrid;
    }

    public void GetFieldDictionary() {

    }

    private void OnDrawGizmos() {
        if (fieldsOrigin == null || fieldTypeFormat.Length == 0) return;

        Gizmos.color = Color.yellow;
        for (int row = 0; row < fieldTypeFormat.Length; row++) {
            for (int col = 0; col < numberOfColumns; col++) {
                Vector3 offset = new Vector3(col * horizontalOffset, 0, row * verticalOffset);
                Vector3 spawnPosition = fieldsOrigin.position + fieldsOrigin.rotation * offset;
                Gizmos.DrawWireCube(spawnPosition, fieldSize);
            }
        }
    }
}
