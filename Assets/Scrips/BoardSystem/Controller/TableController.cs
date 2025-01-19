using Cysharp.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public struct CellSize {
    public float width;
    public float height;
}

public class TableController : MonoBehaviour {
    [SerializeField] private int spawnDelay = 15;
    public CellSize cellSize = new CellSize { width = 1f, height = 1f };
    public Transform origin;
    public GameObject fieldPrefab;

    private int gridWidth;
    private int gridHeight;

    public async UniTask SpawnFields(Grid grid) {
        gridWidth = grid.Fields.Count;
        gridHeight = grid.Fields[0].Count;

        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                Vector3 targetPosition = GetWorldPosition(x, y, gridWidth, gridHeight);

                GameObject fieldObject = Instantiate(fieldPrefab, targetPosition, Quaternion.identity);
                Field fieldData = grid.Fields[x][y];
                fieldObject.GetComponent<FieldController>().Initialize(fieldData);
            }
            await UniTask.Delay(spawnDelay);
        }
    }


    public Vector3 GetWorldPosition(int x, int y, int gridWidth, int gridHeight) {
        // Розраховуємо зміщення для центрування всіх полів
        float offsetX = -((gridWidth * cellSize.width) / 2f) + (cellSize.width / 2f);
        float offsetY = -((gridHeight * cellSize.height) / 2f) + (cellSize.height / 2f);

        // Позиція у локальній системі координат
        Vector3 localPosition = new Vector3(x * cellSize.width + offsetX, 0, y * cellSize.height + offsetY);

        // Трансформуємо у світову систему координат
        return origin.TransformPoint(localPosition);
    }

    public Vector2Int GetGridIndex(Vector3 worldPosition) {
        if (gridHeight == 0 || gridWidth == 0) {
            Debug.Log("Width and height not instantiated yet for the board!");
            return new Vector2Int(-1, -1); // Або інше значення, яке вказує на недійсний індекс
        }

        float offsetX = -((gridWidth * cellSize.width) / 2f) + (cellSize.width / 2f);
        float offsetY = -((gridHeight * cellSize.height) / 2f) + (cellSize.height / 2f);

        // Перетворюємо світову позицію у локальну
        Vector3 localPosition = origin.InverseTransformPoint(worldPosition);

        // Перевірка на глибину
        if (localPosition.y < 0) {
            return new Vector2Int(-1, -1); // Або інше значення, яке вказує на недійсний індекс
        }

        int x = Mathf.FloorToInt((localPosition.x - offsetX) / cellSize.width);
        int y = Mathf.FloorToInt((localPosition.z - offsetY) / cellSize.height);

        return new Vector2Int(x, y);
    }
}
