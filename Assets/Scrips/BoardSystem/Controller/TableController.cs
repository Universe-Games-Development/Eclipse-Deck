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

    [Header("Mouse Check Range")]
    [Range (0, 10)]
    public float yCheckRange = 1f;
    public async UniTask SpawnFields(Grid grid) {
        gridWidth = grid.Fields.Count;
        gridHeight = grid.Fields[0].Count;
        float xOffset = cellSize.width / 2;
        float yOffset = cellSize.height / 2;

        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                // Обчислюємо локальну позицію відносно origin
                Vector3 localPosition = new Vector3(x * cellSize.width + xOffset, 0f, y * cellSize.height + yOffset);

                // Перетворюємо локальну позицію в світову, враховуючи трансформації origin
                Vector3 spawnPosition = origin.TransformPoint(localPosition);

                // Створюємо об'єкт з поворотом origin
                GameObject fieldObject = Instantiate(fieldPrefab, spawnPosition, origin.rotation, origin); // Ключова зміна тут

                Field fieldData = grid.Fields[x][y];
                fieldObject.GetComponent<FieldController>().Initialize(fieldData);

                await UniTask.Delay(spawnDelay);
            }
        }
    }

    public Vector2Int? GetGridIndex(Vector3 worldPosition) {
        if (gridWidth == 0 || gridHeight == 0) {
            Debug.LogError("Grid dimensions are not initialized!");
            return null;
        }

        // Перевірка по Y (висоті)
        if (Mathf.Abs(worldPosition.y - origin.position.y) > yCheckRange) {
            // Позиція за межами діапазону по Y
            return null;
        }

        Vector3 localPosition = origin.InverseTransformPoint(worldPosition);

        int x = Mathf.FloorToInt((localPosition.x) / cellSize.width);
        int y = Mathf.FloorToInt((localPosition.z) / cellSize.height);

        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) {
            return null;
        }

        return new Vector2Int(x, y);
    }

}