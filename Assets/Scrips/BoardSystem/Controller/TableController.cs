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
                // ���������� �������� ������� ������� origin
                Vector3 localPosition = new Vector3(x * cellSize.width + xOffset, 0f, y * cellSize.height + yOffset);

                // ������������ �������� ������� � ������, ���������� ������������� origin
                Vector3 spawnPosition = origin.TransformPoint(localPosition);

                // ��������� ��'��� � ��������� origin
                GameObject fieldObject = Instantiate(fieldPrefab, spawnPosition, origin.rotation, origin); // ������� ���� ���

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

        // �������� �� Y (�����)
        if (Mathf.Abs(worldPosition.y - origin.position.y) > yCheckRange) {
            // ������� �� ������ �������� �� Y
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