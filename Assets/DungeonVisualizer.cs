// Клас вузла залишаємо майже без змін
using System.Collections.Generic;
using UnityEngine;
// Клас для логування і візуалізації
public class DungeonVisualizer {
    private DungeonGraph graph;
    private UIManager uIManager;

    public DungeonVisualizer(DungeonGraph graph, UIManager uIManager) {
        this.graph = graph;
        this.uIManager = uIManager;
    }

    public void LogGraphStructure(string message) {
        Debug.Log($"{message}:");
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();

        for (int level = 0; level < levelNodes.Count; level++) {
            string levelInfo = $"Рівень {level}: {levelNodes[level].Count} вузлів - ";
            foreach (DungeonNode node in levelNodes[level]) {
                levelInfo += $"[{node.id}] ";
            }
            Debug.Log(levelInfo);
        }
    }

    public void LogGraphConnections(string message) {
        Debug.Log($"{message}:");
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

    public void LogRoomAssignments() {
        Debug.Log("Призначення кімнат:");
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();

        for (int level = 0; level < levelNodes.Count; level++) {
            foreach (DungeonNode node in levelNodes[level]) {
                Debug.Log($"Вузол {node.id} (рівень {node.position.x}, індекс {node.position.y}) -> Кімната: {node.roomType}");
            }
        }
    }

    public void VisualizeGraph() {
        float spacing = 2.0f; // Відстань між вузлами для кращої читабельності

        foreach (var level in graph.GetLevelNodes()) {
            foreach (var node in level) {
                Vector3 nodePosition = new Vector3(node.position.x * spacing, 0, node.position.y * spacing);

                // Відображення вузла
                GameObject nodeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                nodeObject.transform.position = nodePosition;
                nodeObject.transform.localScale = Vector3.one * 0.5f; // Зменшуємо розмір сфери
                uIManager.ShowTextAt($"id:{node.id} {node.roomType.ToString()} \n x [{node.position.x}] y [{node.position.y}]", nodeObject.transform.position + Vector3.up);

                // Відображення зв'язків
                foreach (var connectedNode in node.nextLevelConnections) {
                    Vector3 connectedPosition = new Vector3(connectedNode.position.x * spacing, 0, connectedNode.position.y * spacing);
                    Debug.DrawLine(nodePosition, connectedPosition, Color.white, 50f);
                }
            }
        }
    }

}
