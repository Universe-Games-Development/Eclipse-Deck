using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class DungeonVisualizer : MonoBehaviour {
    [Inject] private UIManager uIManager;
    [SerializeField] private Transform dungeonMapParent;
    [SerializeField] private RoomPresenter roomPrefab;
    [SerializeField] private float spacing = 2.0f;
    
    [Header ("Diagnostic")]
    [SerializeField] private bool showNodeDetails = true;
    [SerializeField] private Color normalConnectionColor = Color.white;
    [SerializeField] private Color redundantConnectionColor = Color.red;
    [SerializeField] private Color selfReferenceColor = Color.cyan;
    [SerializeField] private Color invalidNodeColor = Color.magenta;

    private Dictionary<int, RoomPresenter> nodeToRoomMap = new Dictionary<int, RoomPresenter>();
    private List<GameObject> createdObjects = new List<GameObject>();
    private List<TextMeshProUGUI> createdTexts = new List<TextMeshProUGUI>();

    private DungeonGraph currentGraph;

    public void VisualizeGraph(DungeonGraph graph) {
        ClearVisualization();
        currentGraph = graph;

        int totalLevels = graph.GetLevelNodes().Count;

        // Creating rooms first
        CreateRooms(graph, totalLevels);

        // Creating connections
        CreateConnections(graph, totalLevels);
    }

    private void CreateRooms(DungeonGraph graph, int totalLevels) {
        foreach (var level in graph.GetLevelNodes()) {
            foreach (var node in level) {
                RoomPresenter roomObject = Instantiate(roomPrefab, dungeonMapParent);
                roomObject.Initialize(node, spacing);
                roomObject.transform.position = new Vector3(node.position.x * spacing, 0, node.position.y * spacing);
                roomObject.transform.localScale = Vector3.one * 0.5f;

                nodeToRoomMap[node.id] = roomObject;
                createdObjects.Add(roomObject.gameObject);

                if (showNodeDetails) {
                    DisplayNodeDetails(node, roomObject);
                }

                bool hasProperLevelLinks = CheckProperLevelLinks(node, totalLevels);
                if (!hasProperLevelLinks) {
                    roomObject.MarkWrong();
                }
            }
        }
    }

    private void CreateConnections(DungeonGraph graph, int totalLevels) {
        foreach (var level in graph.GetLevelNodes()) {
            foreach (var node in level) {
                if (!nodeToRoomMap.TryGetValue(node.id, out RoomPresenter roomObject)) {
                    continue;
                }

                bool hasSelfReference = CheckSelfReference(node);
                if (hasSelfReference) {
                    roomObject.MarkWithColor(selfReferenceColor);
                }

                foreach (var connectedNode in node.nextLevelConnections) {
                    Color connectionColor = DetermineConnectionColor(node, connectedNode);
                    roomObject.AddConnection(connectedNode, connectionColor, 0.05f);
                }
            }
        }
    }

    private Color DetermineConnectionColor(DungeonNode node, DungeonNode connectedNode) {
        if (node.id == connectedNode.id) {
            return selfReferenceColor;
        }

        if (node.nextLevelConnections.Count == 1) {
            return normalConnectionColor;
        }

        // Перевіряємо, чи з'єднання є надлишковим
        bool isRedundant = connectedNode.prevLevelConnections.Count > 1;
        return isRedundant ? redundantConnectionColor : normalConnectionColor;
    }

    private void DisplayNodeDetails(DungeonNode node, RoomPresenter roomObject) {
        string nodeInfo = $"id:{node.id} {node.room.data.roomName} \n x [{node.position.x}] y [{node.position.y}]";
        TextMeshProUGUI textMesh = uIManager.CreateTextAt(nodeInfo, roomObject.transform.position + Vector3.up);
        if (textMesh != null) {
            createdTexts.Add(textMesh);
        }
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
        foreach (GameObject obj in createdObjects) {
            if (obj != null) {
                GameObject.Destroy(obj);
            }
        }
        createdObjects.Clear();

        foreach (TextMeshProUGUI text in createdTexts) {
            if (text != null) {
                uIManager.RemoveText(text);
            }
        }
        createdTexts.Clear();

        nodeToRoomMap.Clear();
        currentGraph = null;
    }

    public void ToggleNodeDetails() {
        showNodeDetails = !showNodeDetails;
        if (currentGraph != null) {
            VisualizeGraph(currentGraph);
        }
    }

    public void HighlightPath(DungeonNode startNode, DungeonNode endNode) {
        // Реалізація алгоритму пошуку шляху та підсвічування
        // ...
    }

    #region Logging Methods
    public void LogGraphStructure(DungeonGraph graph) {
        Debug.Log("Структура графу:");
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
        Debug.Log("З'єднання графу:");
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
    #endregion
}