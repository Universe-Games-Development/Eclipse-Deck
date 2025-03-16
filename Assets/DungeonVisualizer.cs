using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class DungeonVisualizer : MonoBehaviour {
    private List<GameObject> createdObjects = new List<GameObject>(); // Список для збереження створених об'єктів
    private List<TextMeshProUGUI> createdTexts = new List<TextMeshProUGUI>(); // Список для збереження текстових елементів
    [Inject] UIManager uIManager;
    [SerializeField] Transform dungeonMapParent;
    [SerializeField] RoomPresenter roomPrefab;
    [SerializeField] float spacing = 2.0f;

    public void VisualizeGraph(DungeonGraph graph) {
        // Очищуємо попередню візуалізацію
        ClearVisualization();

        int totalLevels = graph.GetLevelNodes().Count;

        foreach (var level in graph.GetLevelNodes()) {
            foreach (var node in level) {
                RoomPresenter roomObject = Instantiate(roomPrefab);
                roomObject.SetSelfNode(node, spacing);
                //if (dungeonMapParent != null)
                //    roomObject.transform.SetParent(dungeonMapParent);
                roomObject.transform.position = new Vector3(node.position.x * spacing, 0, node.position.y * spacing); ;
                roomObject.transform.localScale = Vector3.one * 0.5f;
                createdObjects.Add(roomObject.gameObject);

                // Відображення тексту та збереження посилання на нього
                TextMeshProUGUI textMesh = uIManager.CreateTextAt($"id:{node.id} {node.room.data.roomName.ToString()} \n x [{node.position.x}] y [{node.position.y}]", roomObject.transform.position + Vector3.up);
                if (textMesh != null) {
                    createdTexts.Add(textMesh);
                }

                Color color = Color.white;

                bool hasSelfReference = CheckSelfReference(node);
                if (hasSelfReference) {
                    color = Color.cyan;
                }

                // Відображення зв'язків
                foreach (var connectedNode in node.nextLevelConnections) {
                    if (node.nextLevelConnections.Count == 1) {
                        color = Color.white;
                    } else {
                        bool isMinimal = true;
                        if (connectedNode.prevLevelConnections.Count > 1) {
                            isMinimal = false;
                        }
                        color = isMinimal ? Color.white : Color.red;
                    }

                    
                    // Створюємо лінію як об'єкт для можливості видалення
                    roomObject.AddConnection(connectedNode, color, 0.05f);
                }

                bool hasProperLevelLinks = CheckProperLevelLinks(node, totalLevels);
                if (!hasProperLevelLinks) {
                    roomObject.MarkWrong();
                }
            }
        }
    }

    private bool HasMinimalNecessaryConnections(DungeonNode node) {
        int prevConnections = node.prevLevelConnections.Count;
        int nextConnections = node.nextLevelConnections.Count;

        if (prevConnections <= 1 && nextConnections <= 1) {
            return true;
        }

        if (nextConnections > 1) {
            foreach (var nextNode in node.nextLevelConnections) {
                if (nextNode.prevLevelConnections.Count > 1) {
                    return false; 
                }
            }
        }

        //if (prevConnections > 1) {
        //    foreach (var prevNode in node.prevLevelConnections) {
        //        if (prevNode.nextLevelConnections.Count == 1) {
        //            return true; 
        //        }
        //    }
        //}
        
        return true; 
    }


    private bool CheckProperLevelLinks(DungeonNode node, int totalLevels) {
        if (node.level == 0) {
            return node.HasConnectionsToNextLevel();
        }

        if (node.level == totalLevels - 1) {
            return node.HasConnectionsToPrevLevel();
        }

        return node.IsLinked();
    }

    private bool CheckSelfReference(DungeonNode node) {
        foreach (var nextNode in node.nextLevelConnections) {
            if (nextNode.id == node.id) {
                return true;
            }
        }

        foreach (var prevNode in node.prevLevelConnections) {
            if (prevNode.id == node.id) {
                return true;
            }
        }

        return false;
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
}