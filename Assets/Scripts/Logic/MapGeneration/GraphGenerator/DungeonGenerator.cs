using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public interface IDungeonGenerator {
    bool GenerateDungeon(LocationData locationData, out DungeonGraph dungeonGraph);
}

public class DungeonGenerator : IDungeonGenerator {

    private GraphCenterer centerer;
    private GraphGenerator graphGenerator;
    [Inject] RoomPopulator roomPopulator;

    public DungeonGenerator() {
        centerer = new GraphCenterer();
        graphGenerator = new GraphGenerator();
    }

    public bool GenerateDungeon(LocationData locationData, out DungeonGraph dungeonGraph) {
        dungeonGraph = null;
        if (locationData == null) {
            Debug.LogError("Empty common room list to generate rooms");
            return false;
        }

        var startTime = Time.realtimeSinceStartup;

        dungeonGraph = graphGenerator.GenerateGraph(locationData.mapGenerationData);
        CheckGraphValidation(dungeonGraph);

        roomPopulator.UpdateData(locationData);
        roomPopulator.PopulateGraph(dungeonGraph);

        centerer.CenterGraph(dungeonGraph);

        var endTime = Time.realtimeSinceStartup;
        //Debug.Log($"Dungeon map generation took {endTime - startTime} seconds");
        return true;
    }


    private void CheckGraphValidation(DungeonGraph graph) {
        List<List<DungeonNode>> list = graph.GetLevelNodes();
        for (int levelCounter = 1; levelCounter < list.Count; levelCounter++) {
            List<DungeonNode> levelNodes = list[levelCounter];
            for (int j = 0; j < levelNodes.Count; j++) {
                DungeonNode node = levelNodes[j];
                if (!node.HasConnectionsToPrevLevel() && (levelCounter != 0)) {
                    Debug.LogWarning($"Absent Prev Connection at Level : {node.level}, id : {node.id}");
                } else if (!node.HasConnectionsToNextLevel() && (levelCounter < list.Count - 1)) {
                    Debug.LogWarning($"Absent Next Connection at Level : {node.level}, id : {node.id}");
                }
            }
        }
    }
}

public class DungeonNode {
    public int id;
    public Vector2 position;
    // Розділяємо зв'язки на попередній та наступний рівні
    public HashSet<DungeonNode> prevLevelConnections = new HashSet<DungeonNode>();
    public HashSet<DungeonNode> nextLevelConnections = new HashSet<DungeonNode>();
    public GameObject roomInstance;
    public int level;
    public Room Room { get; private set; }

    public DungeonNode(int nodeId, Vector2 pos) {
        id = nodeId;
        position = pos;
    }

    public void ConnectTo(DungeonNode other) {
        if (other.level > level) {
            ConnectToNext(other);
        } else if (other.level < level) {
            ConnectToPrev(other);
        } else {
            Debug.LogWarning($"Wrong connection Try to same level: {id} and {other.id}");
        }
    }
    // Метод для з'єднання з вузлом наступного рівня
    public void ConnectToNext(DungeonNode other) {
        if (!nextLevelConnections.Contains(other)) {
            nextLevelConnections.Add(other);
        }

        if (!other.prevLevelConnections.Contains(this)) {
            other.prevLevelConnections.Add(this);
        }
    }

    // Метод для з'єднання з вузлом попереднього рівня
    public void ConnectToPrev(DungeonNode other) {
        if (!prevLevelConnections.Contains(other)) {
            prevLevelConnections.Add(other);
        }

        if (!other.nextLevelConnections.Contains(this)) {
            other.nextLevelConnections.Add(this);
        }
    }

    public void UnConnect(DungeonNode unconnectNode) {
        if (unconnectNode.level > level) {
            UnConnectFromNext(unconnectNode);
        } else if (unconnectNode.level < level) {
            UnConnectFromPrev(unconnectNode);
        } else {
            Debug.LogWarning($"Wrong connection Try to same level: {id} and {unconnectNode.id}");
        }
    }

    public void UnConnectFromNext(DungeonNode connection) {
        if (nextLevelConnections.Contains(connection)) {
            nextLevelConnections.Remove(connection);
        }

        if (connection.prevLevelConnections.Contains(this)) {
            connection.prevLevelConnections.Remove(this);
        }
    }

    // Метод для від'єднання від вузла попереднього рівня
    public void UnConnectFromPrev(DungeonNode connection) {
        if (prevLevelConnections.Contains(connection)) {
            prevLevelConnections.Remove(connection);
        }

        if (connection.nextLevelConnections.Contains(this)) {
            connection.nextLevelConnections.Remove(this);
        }
    }

    // Допоміжний метод для отримання всіх зв'язків (для сумісності)
    public List<DungeonNode> GetAllConnections() {
        List<DungeonNode> allConnections = new List<DungeonNode>();
        allConnections.AddRange(prevLevelConnections);
        allConnections.AddRange(nextLevelConnections);
        return allConnections;
    }

    public void ClearConnections() {
        List<DungeonNode> prevConnections = new List<DungeonNode>(prevLevelConnections);
        foreach (DungeonNode prevNode in prevConnections) {
            prevNode.nextLevelConnections.Remove(this);
            prevLevelConnections.Remove(prevNode);
        }

        // Видаляємо зв'язки з наступним рівнем
        List<DungeonNode> nextConnections = new List<DungeonNode>(nextLevelConnections);
        foreach (DungeonNode nextNode in nextConnections) {
            nextNode.prevLevelConnections.Remove(this);
            nextLevelConnections.Remove(nextNode);
        }
    }

    public bool HasConnectionsToPrevLevel() {
        return !prevLevelConnections.IsEmpty();
    }

    public bool HasConnectionsToNextLevel() {
        return !nextLevelConnections.IsEmpty();
    }

    public bool IsConnectedTo(DungeonNode targetNode) {
        return nextLevelConnections.Contains(targetNode) || prevLevelConnections.Contains(targetNode);
    }

    public bool IsLinked() {
        return HasConnectionsToPrevLevel() && HasConnectionsToNextLevel();
    }

    public void BindRoom(Room room) {
        Room = room;
    }

    public void ClearRoom() {
        Room = null;
    }
}

public class DungeonGraph {
    private List<List<DungeonNode>> levelNodes = new();
    private int nextNodeId = 0;

    public void AddNodeToLevel(int level, DungeonNode node) {
        // Перевірка чи рівень існує, якщо ні - створити
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

    public void AddLevel(List<DungeonNode> level) {
        levelNodes.Add(level);
    }

    public void AddLevel(int levelIndex, List<DungeonNode> level) {
        levelNodes.Insert(levelIndex, level);
    }

    public void UpdateNodesData() {
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

    public List<List<DungeonNode>> GetLevelNodes() => levelNodes;

    public List<DungeonNode> GetAllNodes() {
        List<DungeonNode> allNodes = new List<DungeonNode>();
        foreach (var level in levelNodes) {
            allNodes.AddRange(level);
        }
        return allNodes;
    }

    public DungeonNode GetNodeById(int id) {
        return GetAllNodes().FirstOrDefault(n => n.id == id);
    }

    public int GetLevelCount() => levelNodes.Count;

    public int GetNodesAtLevel(int level) => level < levelNodes.Count ? levelNodes[level].Count : 0;

    public int GetNextNodeId() {
        return nextNodeId++;
    }

    public void Clear() {
        levelNodes.Clear();
        nextNodeId = 0;
    }

    public DungeonNode GetEntranceNode() {
        return levelNodes[0][0];
    }

    public DungeonNode GetEndRoom() {
        return levelNodes[levelNodes.Count - 1][0];
    }
}

public class GraphCenterer {

    public void CenterGraph(DungeonGraph graph) {
        int maxNodesLevel = FindLevelWithMaxNodes(graph);

        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();

        // Знаходимо висоту (кількість нод) рівня з максимальною кількістю нод
        int maxLevelHeight = levelNodes[maxNodesLevel].Count;

        // Для кожного рівня
        for (int level = 0; level < levelNodes.Count; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            int currentLevelHeight = currentLevel.Count;

            // Обчислюємо зміщення для центрування
            float offset = ((float)(maxLevelHeight - currentLevelHeight)) / 2;

            // Оновлюємо позиції вузлів
            for (int i = 0; i < currentLevel.Count; i++) {
                // Зберігаємо x-координату (рівень) незмінною, змінюємо лише y-координату (позицію в рівні)
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

public class RoomPopulator {
    private IRoomFactory _roomFactory;
    private IRoomActivityFactory _roomActivityFactory;

    public RoomPopulator(IRoomFactory roomFactory, IRoomActivityFactory roomActivityFactory) {
        _roomFactory = roomFactory;
        _roomActivityFactory = roomActivityFactory;
    }

    public void UpdateData(LocationData currentLocationData) {
        _roomFactory.UpdateRoomData(currentLocationData.locationRoomsData);
        _roomActivityFactory.UpdateActivityData(currentLocationData.activitiesData);
    }

    public void PopulateGraph(DungeonGraph graph) {
        for (int level = 0; level < graph.GetLevelCount(); level++) {
            foreach (DungeonNode node in graph.GetLevelNodes()[level]) {
                var room = _roomFactory.GetRoom(graph, node);
                var activity = _roomActivityFactory.CreateActivity(graph, node, room);

                room.SetActivity(activity);
                node.BindRoom(room);
            }
        }
    }

}
