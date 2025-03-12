// ���� ����� �������� ����� ��� ���
using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class DungeonNode {
    public int id;
    public Vector2 position;
    // ��������� ��'���� �� ��������� �� ��������� ���
    public HashSet<DungeonNode> prevLevelConnections = new HashSet<DungeonNode>();
    public HashSet<DungeonNode> nextLevelConnections = new HashSet<DungeonNode>();
    public RoomType roomType;
    public GameObject roomInstance;
    public int level;

    public DungeonNode(int nodeId, Vector2 pos) {
        id = nodeId;
        position = pos;
    }

    // ����� ��� �'������� � ������ ���������� ����
    public void ConnectToNext(DungeonNode other) {
        if (!nextLevelConnections.Contains(other)) {
            nextLevelConnections.Add(other);
        }

        if (!other.prevLevelConnections.Contains(this)) {
            other.prevLevelConnections.Add(this);
        }
    }

    // ����� ��� �'������� � ������ ������������ ����
    public void ConnectToPrev(DungeonNode other) {
        if (!prevLevelConnections.Contains(other)) {
            prevLevelConnections.Add(this);
        }

        if (!other.nextLevelConnections.Contains(this)) {
            other.nextLevelConnections.Add(this);
        }
    }

    // ����� ��� ��'������� �� ����� ���������� ����
    public void UnConnectFromNext(DungeonNode connection) {
        if (nextLevelConnections.Contains(connection)) {
            nextLevelConnections.Remove(connection);
        }

        if (connection.prevLevelConnections.Contains(this)) {
            connection.prevLevelConnections.Remove(this);
        }
    }

    // ����� ��� ��'������� �� ����� ������������ ����
    public void UnConnectFromPrev(DungeonNode connection) {
        if (prevLevelConnections.Contains(connection)) {
            prevLevelConnections.Remove(connection);
        }

        if (connection.nextLevelConnections.Contains(this)) {
            connection.nextLevelConnections.Remove(this);
        }
    }

    // ��������� ����� ��� ��������� ��� ��'���� (��� ��������)
    public List<DungeonNode> GetAllConnections() {
        List<DungeonNode> allConnections = new List<DungeonNode>();
        allConnections.AddRange(prevLevelConnections);
        allConnections.AddRange(nextLevelConnections);
        return allConnections;
    }

    internal void ClearConnections() {
        List<DungeonNode> prevConnections = new List<DungeonNode>(prevLevelConnections);
        foreach (DungeonNode prevNode in prevConnections) {
            prevNode.nextLevelConnections.Remove(this);
            prevLevelConnections.Remove(prevNode);
        }

        // ��������� ��'���� � ��������� �����
        List<DungeonNode> nextConnections = new List<DungeonNode>(nextLevelConnections);
        foreach (DungeonNode nextNode in nextConnections) {
            nextNode.prevLevelConnections.Remove(this);
            nextLevelConnections.Remove(nextNode);
        }
    }
}

// ����, �� ����������� ��������� ����� ���������
public class DungeonGraph {
    private List<List<DungeonNode>> levelNodes = new();
    private int nextNodeId = 0;

    // ����� ������ ��� ���������
    public List<List<DungeonNode>> GetLevelNodes() => levelNodes;
    public List<DungeonNode> GetAllNodes() {
        List<DungeonNode> allNodes = new List<DungeonNode>();
        foreach (var level in levelNodes) {
            allNodes.AddRange(level);
        }
        return allNodes;
    }

    // ������ ������ ��� ���������� � �������� �������
    public void AddNodeToLevel(int level, DungeonNode node) {
        // �������� �� ����� ����, ���� � - ��������
        while (levelNodes.Count <= level) {
            levelNodes.Add(new List<DungeonNode>());
        }
        levelNodes[level].Add(node);
    }

    public bool RemoveNode(DungeonNode node) {
        foreach (var level in levelNodes) {
            if (level.Contains(node)) {
                node.ClearConnections();
                return level.Remove(node);
            }
        }
        return false;
    }

    public DungeonNode GetNodeById(int id) {
        return GetAllNodes().FirstOrDefault(n => n.id == id);
    }

    public void AddLevel(List<DungeonNode> level) {
        levelNodes.Add(level);
    }

    public int GetNextNodeId() {
        return nextNodeId++;
    }

    public void UpdateNodeData() {
        int roomId = 0;
        for (int level = 0; level < levelNodes.Count; level++) {
            List<DungeonNode> levelNodes = this.levelNodes[level];
            for (int i = 0; i < levelNodes.Count; i++) {
                levelNodes[i].id = roomId++;
                levelNodes[i].level = level;
                levelNodes[i].position = new Vector2(level, i);
            }
        }
    }

    public void Clear() {
        levelNodes.Clear();
        nextNodeId = 0;
    }

    public int GetLevelCount() => levelNodes.Count;
    public int GetNodesAtLevel(int level) => level < levelNodes.Count ? levelNodes[level].Count : 0;
}


// ���� ��� ��������� �����
public class GraphGenerator {
    private DungeonGraph graph;
    private DungeonGeneratorSettings settings;
    private System.Random random;

    public GraphGenerator(DungeonGeneratorSettings settings, System.Random random) {
        this.settings = settings;
        this.random = random;
    }

    public DungeonGraph GenerateGraph() {
        graph = new DungeonGraph();
        CreateInitialGraph();
        ModifyNodeCount();  // ������� ����� ��� ��������� � ��������� �����

        CreateMainPaths();
        AddRandomConnections();
        RemoveRandomConnections();
        return graph;
    }

    private void CreateInitialGraph() {
        for (int level = 0; level < settings.levelCount; level++) {
            List<DungeonNode> currentLevelNodes = new List<DungeonNode>();
            for (int i = 0; i < settings.initialNodesPerLevel; i++) {
                DungeonNode newNode = new DungeonNode(graph.GetNextNodeId(), new Vector2(level, i));
                currentLevelNodes.Add(newNode);
            }
            graph.AddLevel(currentLevelNodes);
        }
    }

    private void ModifyNodeCount() {
        RemoveRandomNodes();
        AddRandomNodes();
        graph.UpdateNodeData();
    }

    private void CreateMainPaths() {
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();

        for (int level = 0; level < levelNodes.Count - 1; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            List<DungeonNode> nextLevel = levelNodes[level + 1];

            for (int i = 0; i < currentLevel.Count; i++) {
                DungeonNode currentNode = currentLevel[i];
                float normalizedPosition = currentLevel.Count > 1 ? (float)i / (currentLevel.Count - 1) : 0.5f;

                List<DungeonNode> connectTo = new List<DungeonNode>();

                if (normalizedPosition < 0.33f) {
                    connectTo.Add(nextLevel[0]);
                    //if (nextLevel.Count > 1) connectTo.Add(nextLevel[1]);
                } else if (normalizedPosition < 0.66f) {
                    int middleIndex = nextLevel.Count / 2;
                    connectTo.Add(nextLevel[middleIndex]);
                    if (nextLevel.Count > 1 && middleIndex - 1 >= 0) connectTo.Add(nextLevel[middleIndex - 1]);
                    if (nextLevel.Count > 2 && middleIndex + 1 < nextLevel.Count) connectTo.Add(nextLevel[middleIndex + 1]);
                } else {
                    connectTo.Add(nextLevel[nextLevel.Count - 1]);
                    if (nextLevel.Count > 1) connectTo.Add(nextLevel[nextLevel.Count - 2]);
                }

                // Added random connections probability
                for (int j = 0; j < connectTo.Count; j++) {
                    // if it first then guarantee connection
                    bool isFirstConnection = j == 0;
                    bool shouldConnect = isFirstConnection || random.NextDouble() > settings.randomConnectionChance;

                    if (shouldConnect) currentNode.ConnectToNext(connectTo[j]);
                }
            }
        }
    }

    private void RemoveRandomNodes() {
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();
        for (int level = 0; level < levelNodes.Count; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            int nodesToRemove = random.Next(0, currentLevel.Count - settings.minNodesPerLevel + 1);

            
            if (nodesToRemove > 0) {
                List<int> indices = Enumerable.Range(0, currentLevel.Count).ToList();
                Shuffle(indices);

                for (int i = 0; i < nodesToRemove; i++) {
                    int indexToRemove = indices[i] - i;
                    currentLevel[indexToRemove].ClearConnections();
                    currentLevel.RemoveAt(indexToRemove);
                }
            }
        }
    }

    private void AddRandomNodes() {
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();

        for (int level = 0; level < levelNodes.Count; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            int nodesToAdd = random.Next(0, settings.maxNodesPerLevel - currentLevel.Count + 1);

            if (nodesToAdd > 0) {
                for (int i = 0; i < nodesToAdd; i++) {
                    DungeonNode newNode = new DungeonNode(graph.GetNextNodeId(), new Vector2(level, currentLevel.Count));
                    newNode.level = level;
                    currentLevel.Add(newNode);
                }
            }
        }
    }

    private void RemoveRandomConnections() {
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();

        for (int level = 0; level < levelNodes.Count - 1; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];

            foreach (DungeonNode currentNode in currentLevel) {
                // ������� ������ ��'����, ��� �������� ������� ��� ��������
                List<DungeonNode> nextLevelConnections = new List<DungeonNode>(currentNode.nextLevelConnections);

                // ���� � ����� ������ ��'����, ������ �������� ����, ��������� ���� � ����
                if (nextLevelConnections.Count > 1) {
                    foreach (var connection in nextLevelConnections) {
                        // ����������, �� � ����� ���������� ���� ���������� ���� � ���� ��'���� � �������� �����
                        bool canRemove = connection.prevLevelConnections.Count > 1;

                        // ��������� ��'���� � ��������� destroyConnectionChance
                        if (canRemove && settings.destroyConnectionChance > random.NextDouble()) {
                            // ����������, �� �� �� ������� ��'���� ��� ��������� �����
                            if (currentNode.nextLevelConnections.Count > 1) {
                                currentNode.UnConnectFromNext(connection);
                            }
                        }
                    }
                }
            }
        }
    }

    private void AddRandomConnections() {
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();

        for (int level = 0; level < levelNodes.Count - 1; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            List<DungeonNode> nextLevel = levelNodes[level + 1];

            foreach (DungeonNode currentNode in currentLevel) {
                // ��������� "�����" ����� ���������� ���� ��� ��������� �����
                List<DungeonNode> nearbyNodes = GetNearbyNodes(currentNode, nextLevel);

                foreach (DungeonNode nearbyNode in nearbyNodes) {
                    if (!currentNode.nextLevelConnections.Contains(nearbyNode)) {
                        if (random.NextDouble() < settings.randomConnectionChance) {
                            currentNode.ConnectToNext(nearbyNode);
                        }
                    }
                }
            }
        }
    }

    private List<DungeonNode> GetNearbyNodes(DungeonNode node, List<DungeonNode> nextLevelNodes) {
        List<DungeonNode> nearbyNodes = new List<DungeonNode>();

        // ��������� ������� ������� ��������� ����� � ���� ���
        List<DungeonNode> currentLevelNodes = graph.GetLevelNodes()[node.level];
        float nodeRelativePosition = currentLevelNodes.Count > 1
            ? (float)node.position.y / (currentLevelNodes.Count - 1)
            : 0.5f;

        // ʳ������ ����� � ���������� ���
        int nextLevelCount = nextLevelNodes.Count;

        if (nextLevelCount == 0) return nearbyNodes;

        // ��������� ���������� ������ � ���������� ���, ���� ������� ������� ������� ��������� �����
        int closestIndex = Mathf.RoundToInt(nodeRelativePosition * (nextLevelCount - 1));
        closestIndex = Mathf.Clamp(closestIndex, 0, nextLevelCount - 1);

        // ������ ���������� ����� � ������
        nearbyNodes.Add(nextLevelNodes[closestIndex]);

        // ������ ����� ����� (���� ���� �������)
        if (closestIndex > 0) {
            nearbyNodes.Add(nextLevelNodes[closestIndex - 1]);
        }

        if (closestIndex < nextLevelCount - 1) {
            nearbyNodes.Add(nextLevelNodes[closestIndex + 1]);
        }

        return nearbyNodes;
    }

    private void Shuffle<T>(List<T> list) {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--) {
            int j = random.Next(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}

// ���� ��� ���������� ����� ��������
public class RoomPopulator {
    private List<RoomData> roomList;
    private System.Random random;

    public RoomPopulator(List<RoomData> roomList, System.Random random) {
        this.roomList = roomList;
        this.random = random;
    }

    public void PopulateGraphWithRooms(DungeonGraph graph) {
        RoomGenerator roomGenerator = new RoomGenerator(roomList, random);

        foreach (var level in graph.GetLevelNodes()) {
            foreach (var node in level) {
                RoomData template = roomGenerator.GetRandomRoom();
                node.roomType = template.type;

                // ����� ��������� ���������� ������
                // node.roomInstance = Instantiate(template.roomPrefab, position, rotation);
            }
        }
    }
}

public class GraphCenterer {

    public void CenterGraph(DungeonGraph graph) {
        int maxNodesLevel = FindLevelWithMaxNodes(graph);

        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();

        // ��������� ������ (������� ���) ���� � ������������ ������� ���
        int maxLevelHeight = levelNodes[maxNodesLevel].Count;

        // ��� ������� ����
        for (int level = 0; level < levelNodes.Count; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            int currentLevelHeight = currentLevel.Count;

            // ���������� ������� ��� �����������
            float offset = ((float)(maxLevelHeight - currentLevelHeight)) / 2;

            // ��������� ������� �����
            for (int i = 0; i < currentLevel.Count; i++) {
                // �������� x-���������� (�����) ��������, ������� ���� y-���������� (������� � ���)
                currentLevel[i].position = new Vector2(level, i + offset);
            }   
        }
    }

    private int FindLevelWithMaxNodes(DungeonGraph graph) {
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();
        int maxNodes = 0;
        int maxNodesLevel = 0;

        for (int level = 0; level < levelNodes.Count; level++) {
            int nodesCount = levelNodes[level].Count;
            if (nodesCount > maxNodes) {
                maxNodes = nodesCount;
                maxNodesLevel = level;
            }
        }

        return maxNodesLevel;
    }
}

// ������� ���� ��� �����������
[Serializable]
public class DungeonGeneratorSettings {
    public string seed = "simple_seed";

    [Header("Level Settings")]
    public int levelCount = 4;
    [Header ("Nodes Level Settings")]
    public int initialNodesPerLevel = 4;
    public int minNodesPerLevel = 3;
    public int maxNodesPerLevel = 5;
    [Header("Connections Settings")]
    [Range(0f, 1f)] public float randomConnectionChance = 0.3f;
    [Range(0f, 1f)] public float destroyConnectionChance = 0.5f;
}
// �������� ����-���������
public class DungeonGenerator : MonoBehaviour {
    [SerializeField] private DungeonGeneratorSettings settings;
    [SerializeField] private List<RoomData> roomList = new();

    private DungeonGraph graph;
    private GraphGenerator graphGenerator;
    private RoomPopulator roomPopulator;
    private DungeonVisualizer visualizer;
    private System.Random random;
    [Inject] UIManager uIManager;

    private void Start() {
        random = new System.Random(settings.seed.GetHashCode());
        graphGenerator = new GraphGenerator(settings, random);

        roomPopulator = new RoomPopulator(roomList, random);
        GenerateDungeon();
    }

    public void GenerateDungeon() {
        if (roomList.Count == 0) {
            Debug.LogError("Empty room list to generate rooms");
            return;
        }

        Debug.Log("1. ��������� ��������� �����");
        graph = graphGenerator.GenerateGraph();

        // ������� ����������� ����� ���� ���� ���������
        GraphCenterer centerer = new GraphCenterer();
        centerer.CenterGraph(graph);

        visualizer = new DungeonVisualizer(graph, uIManager);

        visualizer.LogGraphStructure("ϳ��� ��������� �����");
        visualizer.LogGraphConnections("�'������� �����");

        Debug.Log("2. ���������� ����� ��������");
        roomPopulator.PopulateGraphWithRooms(graph);
        visualizer.LogRoomAssignments();
        visualizer.VisualizeGraph();
    }
}