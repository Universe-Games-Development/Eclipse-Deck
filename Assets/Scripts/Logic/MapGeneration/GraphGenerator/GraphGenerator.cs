using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphGenerator {
    private DungeonGraph graph;
    private MapGenerationData settings;
    public DungeonGraph GenerateGraph(MapGenerationData settings) {
        this.settings = settings;
        graph = new DungeonGraph();
        CreateInitialGraph();
        ModifyNodeCount();  // Add or remove random nodes

        AddFirstNode();
        AddEndNode();
        AddEndNode();
        graph.UpdateNodesData();
        CreateMainPaths();

        

        return graph;
    }

    private void CreateInitialGraph() {

        // Створюємо всі інші рівні
        for (int level = 0; level < settings.levelCount; level++) {
            List<DungeonNode> currentLevelNodes = new List<DungeonNode>();
            for (int i = 0; i < settings.initialNodesPerLevel; i++) {
                // First level node always 1
                DungeonNode newNode = new DungeonNode(graph.GetNextNodeId(), new Vector2(level, i));
                currentLevelNodes.Add(newNode);
            }
            graph.AddLevel(currentLevelNodes);
        }
    }

    private void ModifyNodeCount() {
        RemoveRandomNodes();
        AddRandomNodes();
    }

    private void CreateMainPaths() {
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();

        for (int level = 0; level < levelNodes.Count; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            List<DungeonNode> nextLevel = new();
            if (level < levelNodes.Count - 1) {
                nextLevel = levelNodes[level + 1];
            }

            foreach (var currentNode in currentLevel) {
                List<DungeonNode> potentialNextConnections = GetNeardyNodes(currentNode, currentLevel, nextLevel);
                EnsureConnections(currentNode, potentialNextConnections);

                if (level >= 1) {
                    if (!currentNode.HasConnectionsToPrevLevel()) {
                        List<DungeonNode> prevLevel = levelNodes[level - 1];
                        List<DungeonNode> prevPottentialConnections = GetNeardyNodes(currentNode, currentLevel, prevLevel);
                        MinimalConnection(currentNode, prevPottentialConnections);
                    }

                }
            }
        }
    }

    private void MinimalConnection(DungeonNode currentNode, List<DungeonNode> prevPottentialConnections) {
        DungeonNode connectedNode = ConnectOneRandomNode(currentNode, prevPottentialConnections);
        foreach (var nextNode in connectedNode.nextLevelConnections) {
            if (nextNode.prevLevelConnections.Count > 1) {
                DungeonNode unnecessaryConnection = nextNode;
                connectedNode.UnConnect(unnecessaryConnection);
                break;
            }
        }
        
    }

    private void EnsureConnections(DungeonNode currentNode, List<DungeonNode> potentialConnections) {
        // Перевіряємо, чи є з'єднання хоча б з одним вузлом
        bool hasConnected = false;

        // Проходимо по всіх потенційних з'єднаннях
        foreach (var targetNode in potentialConnections) {
            bool shouldConnect = UnityEngine.Random.value <= settings.randomConnectionChance;

            if (shouldConnect) {
                currentNode.ConnectTo(targetNode);
                hasConnected = true;
            }
        }

        // Якщо нода не має жодного з'єднання, ми повинні створити хоча б одне
        if (!hasConnected && potentialConnections.Count > 0) {
            ConnectOneRandomNode(currentNode, potentialConnections);
        }
    }

    private DungeonNode ConnectOneRandomNode(DungeonNode currentNode, List<DungeonNode> connections) {
        
        DungeonNode targetNode = connections.GetRandomElement();
        currentNode.ConnectTo(targetNode);
        return targetNode;
    }

    private List<DungeonNode> GetNeardyNodes(DungeonNode currentNode, List<DungeonNode> currentLevel, List<DungeonNode> nextLevel) {
        List<DungeonNode> connectTo = new List<DungeonNode>();
        if (nextLevel.IsEmpty()) {
            return connectTo;
        }
        // Знаходимо індекс поточної ноди в поточному рівні
        int currentIndex = currentLevel.IndexOf(currentNode);

        // Знаходимо відносну позицію (від 0 до 1)
        float relativePosition = currentLevel.Count > 1 ? (float)currentIndex / (currentLevel.Count - 1) : 0.5f;

        // Знаходимо відповідний індекс на наступному рівні
        int targetIndex = Mathf.FloorToInt(relativePosition * (nextLevel.Count - 1));

        

        // Додаємо основну ноду
        connectTo.Add(nextLevel[targetIndex]);

        // Додаємо сусідні ноди (якщо вони існують)
        if (targetIndex > 0) {
            connectTo.Add(nextLevel[targetIndex - 1]);
        }

        if (targetIndex < nextLevel.Count - 1) {
            connectTo.Add(nextLevel[targetIndex + 1]);
        }

        return connectTo;
    }

    private void AddFirstNode() {
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();

        List<DungeonNode> enteranceLevel = new List<DungeonNode>();
        DungeonNode enteranceNode = new DungeonNode(graph.GetNextNodeId(), new Vector2(levelNodes.Count, 0));
        enteranceLevel.Add(enteranceNode);

        List<DungeonNode> firstLevel = levelNodes[0];
        graph.AddLevel(0, enteranceLevel);
        foreach (DungeonNode node in firstLevel) {
            enteranceNode.ConnectToNext(node);
        }
    }

    private void AddEndNode() {
        // Отримуємо останній рівень графа
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();
        List<DungeonNode> lastLevel = levelNodes[levelNodes.Count - 1];

        // Створюємо новий рівень з однією нодою "кінець"
        List<DungeonNode> endLevel = new List<DungeonNode>();
        DungeonNode endNode = new DungeonNode(graph.GetNextNodeId(), new Vector2(levelNodes.Count, 0));
        endLevel.Add(endNode);

        // Додаємо новий рівень до графа
        graph.AddLevel(endLevel);

        // З'єднуємо всі ноди останнього рівня з кінцевою нодою
        foreach (DungeonNode node in lastLevel) {
            endNode.ConnectToPrev(node);
        }
    }

    // always leave 1 node 
    private void RemoveRandomNodes() {
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();
        for (int level = 0; level < levelNodes.Count; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            int amountToRemove = currentLevel.Count - settings.minNodesPerLevel;
            if (amountToRemove <= 0) continue;

            int nodesToRemove = UnityEngine.Random.Range(0, amountToRemove);


            if (nodesToRemove > 0) {
                List<int> indices = Enumerable.Range(0, currentLevel.Count - 1).ToList();
                indices.Shuffle();

                for (int i = 0; i < nodesToRemove; i++) {
                    int randomIndex = indices[i];
                    if (randomIndex < currentLevel.Count) {
                        currentLevel[randomIndex].ClearConnections();
                        currentLevel.RemoveAt(randomIndex);
                    }
                }
            }
        }
    }

    private void AddRandomNodes() {
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();

        for (int level = 0; level < levelNodes.Count; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            int amountToAdd = settings.maxNodesPerLevel - currentLevel.Count;
            if (amountToAdd < 0) {
                Debug.Log("LOLs");
            }
            int nodesToAdd = UnityEngine.Random.Range(0, amountToAdd);

            if (nodesToAdd > 0) {
                for (int i = 0; i < nodesToAdd; i++) {
                    DungeonNode newNode = new DungeonNode(graph.GetNextNodeId(), new Vector2(level, currentLevel.Count));
                    newNode.level = level;
                    currentLevel.Add(newNode);
                }
            }
        }
    }
}
