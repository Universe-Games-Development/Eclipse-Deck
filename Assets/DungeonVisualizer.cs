using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class DungeonVisualizer {
    private UIManager uIManager;
    private List<GameObject> createdObjects = new List<GameObject>(); // Список для збереження створених об'єктів
    private List<TextMeshProUGUI> createdTexts = new List<TextMeshProUGUI>(); // Список для збереження текстових елементів

    public DungeonVisualizer(UIManager uIManager) {
        this.uIManager = uIManager;
    }

    public void LogGraphStructure(DungeonGraph graph) {
        Debug.Log("Після створення графу");
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();
        for (int level = 0; level < levelNodes.Count; level++) {
            string levelInfo = $"Рівень {level}: {levelNodes[level].Count} вузлів - ";
            foreach (DungeonNode node in levelNodes[level]) {
                levelInfo += $"[{node.id}] ";
            }
            Debug.Log(levelInfo);
        }
    }

    public void LogGraphConnections(DungeonGraph graph) {
        Debug.Log("З'єднання графу");
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();
        for (int level = 0; level < levelNodes.Count; level++) {
            foreach (DungeonNode node in levelNodes[level]) {
                string connectionInfo = $"Вузол {node.id} (рівень {node.position.x}, індекс {node.position.y}) -> Зв'язки: ";
                foreach (DungeonNode connected in node.nextLevelConnections) {
                    connectionInfo += $"{connected.id} (рівень {connected.position.x}), ";
                }
                Debug.Log(connectionInfo);
            }
        }
    }

    public void LogRoomAssignments(DungeonGraph graph) {
        Debug.Log("Призначення кімнат:");
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();
        for (int level = 0; level < levelNodes.Count; level++) {
            foreach (DungeonNode node in levelNodes[level]) {
                Debug.Log($"Вузол {node.id} (рівень {node.position.x}, індекс {node.position.y}) -> Кімната: {node.room.data.type}");
            }
        }
    }

    public void VisualizeGraph(DungeonGraph graph) {
        // Очищуємо попередню візуалізацію
        ClearVisualization();

        float spacing = 2.0f; // Відстань між вузлами для кращої читабельності
        foreach (var level in graph.GetLevelNodes()) {
            foreach (var node in level) {
                Vector3 nodePosition = new Vector3(node.position.x * spacing, 0, node.position.y * spacing);

                // Відображення вузла
                GameObject nodeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                nodeObject.transform.position = nodePosition;
                nodeObject.transform.localScale = Vector3.one * 0.5f; // Зменшуємо розмір сфери

                // Додаємо створений об'єкт до списку
                createdObjects.Add(nodeObject);

                // Відображення тексту та збереження посилання на нього
                TextMeshProUGUI textMesh = uIManager.CreateTextAt($"id:{node.id} {node.room.data.roomName.ToString()} \n x [{node.position.x}] y [{node.position.y}]", nodeObject.transform.position + Vector3.up);
                if (textMesh != null) {
                    createdTexts.Add(textMesh);
                }

                // Відображення зв'язків
                foreach (var connectedNode in node.nextLevelConnections) {
                    Vector3 connectedPosition = new Vector3(connectedNode.position.x * spacing, 0, connectedNode.position.y * spacing);

                    // Створюємо лінію як об'єкт для можливості видалення
                    GameObject lineObject = CreateLine(nodePosition, connectedPosition, Color.white, 0.05f);
                    if (lineObject != null) {
                        createdObjects.Add(lineObject);
                    }
                }
            }
        }
    }

    // Допоміжний метод для створення лінії між вузлами як об'єкта
    private GameObject CreateLine(Vector3 start, Vector3 end, Color color, float width) {
        GameObject lineObject = new GameObject("ConnectionLine");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Встановлюємо матеріал для лінії
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = color;

        return lineObject;
    }

    public void ClearVisualization() {
        // Видаляємо всі створені об'єкти
        foreach (GameObject obj in createdObjects) {
            if (obj != null) {
                GameObject.Destroy(obj);
            }
        }
        createdObjects.Clear();

        // Видаляємо всі текстові елементи через UIManager
        foreach (TextMeshProUGUI text in createdTexts) {
            if (text != null) {
                uIManager.RemoveText(text);
            }
        }
        createdTexts.Clear();
    }
}