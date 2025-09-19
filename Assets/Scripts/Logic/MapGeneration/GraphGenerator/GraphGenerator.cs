using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphGenerator {
    private DungeonGraph graph;
    private MapGenerationData settings;

    public DungeonGraph GenerateGraph(MapGenerationData settings) {
        if (settings == null) {
            Debug.LogError("null map generation settings");
        }
        this.settings = settings;
        graph = new DungeonGraph();

        CreateInitialGraph();
        ModifyNodeCountWithDeviation(); // Нова логіка з контролем відхилення

        AddFirstNode();
        AddEndNode();
        AddEndNode();
        graph.UpdateNodesData();
        CreateMainPaths();

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

    private void ModifyNodeCountWithDeviation() {
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();

        for (int level = 0; level < levelNodes.Count; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            int targetNodeCount = CalculateTargetNodeCount(level, levelNodes);

            AdjustLevelToTargetCount(currentLevel, targetNodeCount, level);
        }
    }

    private int CalculateTargetNodeCount(int level, List<List<DungeonNode>> levelNodes) {
        if (level == 0) {
            return GetRandomNodeCountInRange(settings.minNodesPerLevel, settings.maxNodesPerLevel);
        }

        int prevLevelNodeCount = levelNodes[level - 1].Count;

        // Визначаємо діапазон можливих значень
        int minPossible = Math.Max(settings.minNodesPerLevel, prevLevelNodeCount - settings.maxNodeDeviation);
        int maxPossible = Math.Min(settings.maxNodesPerLevel, prevLevelNodeCount + settings.maxNodeDeviation);

        // Застосовуємо обмеження gradual increase/decrease
        if (!settings.allowGradualIncrease) {
            maxPossible = Math.Min(maxPossible, prevLevelNodeCount);
        }
        if (!settings.allowGradualDecrease) {
            minPossible = Math.Max(minPossible, prevLevelNodeCount);
        }

        return GetRandomNodeCountInRange(minPossible, maxPossible);
    }

    private int GetRandomNodeCountInRange(int min, int max) {
        return UnityEngine.Random.Range(min, max + 1);
    }

    private void AdjustLevelToTargetCount(List<DungeonNode> currentLevel, int targetCount, int level) {
        int currentCount = currentLevel.Count;

        if (currentCount > targetCount) {
            // Видаляємо зайві ноди
            RemoveNodesFromLevel(currentLevel, currentCount - targetCount);
        } else if (currentCount < targetCount) {
            // Додаємо недостатні ноди
            AddNodesToLevel(currentLevel, targetCount - currentCount, level);
        }
    }

    private void RemoveNodesFromLevel(List<DungeonNode> currentLevel, int nodesToRemove) {
        if (nodesToRemove <= 0) return;

        List<int> indices = Enumerable.Range(0, currentLevel.Count).ToList();
        indices.Shuffle();

        for (int i = 0; i < nodesToRemove && currentLevel.Count > 0; i++) {
            int randomIndex = indices[i] % currentLevel.Count;
            currentLevel[randomIndex].ClearConnections();
            currentLevel.RemoveAt(randomIndex);

            // Оновлюємо індекси після видалення
            for (int j = i + 1; j < indices.Count; j++) {
                if (indices[j] > randomIndex) {
                    indices[j]--;
                }
            }
        }
    }

    private void AddNodesToLevel(List<DungeonNode> currentLevel, int nodesToAdd, int level) {
        for (int i = 0; i < nodesToAdd; i++) {
            DungeonNode newNode = new DungeonNode(graph.GetNextNodeId(),
                new Vector2(level, currentLevel.Count));
            newNode.level = level;
            currentLevel.Add(newNode);
        }
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
        bool hasConnected = false;

        foreach (var targetNode in potentialConnections) {
            bool shouldConnect = UnityEngine.Random.value <= settings.randomConnectionChance;

            if (shouldConnect) {
                currentNode.ConnectTo(targetNode);
                hasConnected = true;
            }
        }

        if (!hasConnected && potentialConnections.Count > 0) {
            ConnectOneRandomNode(currentNode, potentialConnections);
        }
    }

    private DungeonNode ConnectOneRandomNode(DungeonNode currentNode, List<DungeonNode> connections) {
        if (connections.TryGetRandomElement(out DungeonNode targetNode)) {
            currentNode.ConnectTo(targetNode);
        }
        return targetNode;
    }

    private List<DungeonNode> GetNeardyNodes(DungeonNode currentNode, List<DungeonNode> currentLevel, List<DungeonNode> nextLevel) {
        List<DungeonNode> connectTo = new List<DungeonNode>();
        if (nextLevel.IsEmpty()) {
            return connectTo;
        }

        int currentIndex = currentLevel.IndexOf(currentNode);
        float relativePosition = currentLevel.Count > 1 ? (float)currentIndex / (currentLevel.Count - 1) : 0.5f;
        int targetIndex = Mathf.FloorToInt(relativePosition * (nextLevel.Count - 1));

        connectTo.Add(nextLevel[targetIndex]);

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
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();
        List<DungeonNode> lastLevel = levelNodes[levelNodes.Count - 1];

        List<DungeonNode> endLevel = new List<DungeonNode>();
        DungeonNode endNode = new DungeonNode(graph.GetNextNodeId(), new Vector2(levelNodes.Count, 0));
        endLevel.Add(endNode);

        graph.AddLevel(endLevel);

        foreach (DungeonNode node in lastLevel) {
            endNode.ConnectToPrev(node);
        }
    }
}
