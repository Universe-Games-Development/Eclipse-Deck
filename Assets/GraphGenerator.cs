using ModestTree;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// Клас для генерації графу
public class GraphGenerator {
    private DungeonGraph graph;
    private MapGenerationData settings;
    private System.Random random;

    public GraphGenerator(MapGenerationData settings, System.Random random) {
        this.settings = settings;
        this.random = random;
    }

    public DungeonGraph GenerateGraph() {
        graph = new DungeonGraph();
        CreateInitialGraph();
        ModifyNodeCount();  // Add or remove random nodes
        CreateMainPaths();

        AddFirstNode();
        AddEndNode();
        graph.UpdateNodeData();

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

        // Прохід від останнього рівня до першого
        for (int level = 0; level < levelNodes.Count - 1; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            List<DungeonNode> nextLevel = levelNodes[level + 1];

            foreach (var currentNode in currentLevel) {
                List<DungeonNode> potentialConnections = GetNeardyNodes(currentNode, currentLevel, nextLevel);
                EnsureConnections(currentNode, potentialConnections);
            }
        }

        // Після основної логіки, перевіряємо чи є ноди без з'єднань
        FixDisconnectedNodes(levelNodes);
    }

    private void EnsureConnections(DungeonNode currentNode, List<DungeonNode> potentialConnections) {
        // Перевіряємо, чи є з'єднання хоча б з одним вузлом
        bool hasAtLeastOneConnection = false;

        // Проходимо по всіх потенційних з'єднаннях
        foreach (var targetNode in potentialConnections) {
            // Перевіряємо чи потрібно створити з'єднання на основі randomConnectionChance
            bool shouldConnect = random.NextDouble() <= settings.randomConnectionChance;

            if (shouldConnect) {
                currentNode.ConnectToNext(targetNode);
                hasAtLeastOneConnection = true;
            }
        }

        // Якщо нода не має жодного з'єднання, ми повинні створити хоча б одне
        if (!hasAtLeastOneConnection && potentialConnections.Count > 0) {
            // Обираємо випадкову ноду з потенційних з'єднань
            int randomIndex = random.Next(potentialConnections.Count);
            DungeonNode targetNode = potentialConnections[randomIndex];

            // Створюємо обов'язкове з'єднання
            currentNode.ConnectToNext(targetNode);
        }
    }

    private void FixDisconnectedNodes(List<List<DungeonNode>> levelNodes) {
        for (int level = 1; level < levelNodes.Count - 1; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            List<DungeonNode> prevLevel = levelNodes[level - 1];

            foreach (var currentNode in currentLevel) {
                // Перевіряємо, чи нода має хоча б одне з'єднання з наступним рівнем
                if (!currentNode.HasConnectionsToPrevLevel()) {
                    // Якщо немає з'єднань, знаходимо потенційні з'єднання з попереднього рівня
                    List<DungeonNode> potentialConnections = GetNeardyNodes(currentNode, currentLevel, prevLevel);

                    // Якщо є потенційні з'єднання, створюємо обов'язкове з'єднання
                    if (potentialConnections.Count > 0) {
                        int randomIndex = random.Next(potentialConnections.Count);
                        DungeonNode targetNode = potentialConnections[randomIndex];
                        currentNode.ConnectToNext(targetNode);
                    }
                }
            }
        }
    }


    private List<DungeonNode> GetNeardyNodes(DungeonNode currentNode, List<DungeonNode> currentLevel, List<DungeonNode> nextLevel) {
        // Знаходимо індекс поточної ноди в поточному рівні
        int currentIndex = currentLevel.IndexOf(currentNode);

        // Знаходимо відносну позицію (від 0 до 1)
        float relativePosition = currentLevel.Count > 1 ? (float)currentIndex / (currentLevel.Count - 1) : 0.5f;

        // Знаходимо відповідний індекс на наступному рівні
        int targetIndex = Mathf.FloorToInt(relativePosition * (nextLevel.Count - 1));

        List<DungeonNode> connectTo = new List<DungeonNode>();

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
        List<DungeonNode> firstLevel = levelNodes[0];

        List<DungeonNode> enteranceLevel = new List<DungeonNode>();
        DungeonNode endNode = new DungeonNode(graph.GetNextNodeId(), new Vector2(levelNodes.Count, 0));
        enteranceLevel.Insert(0, endNode);

        // Додаємо новий рівень до графа
        graph.AddLevel(0, enteranceLevel);

        foreach (DungeonNode node in firstLevel) {
            node.ConnectToPrev(endNode);
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
            node.ConnectToNext(endNode);
        }
    }

    // always leave 1 node 
    private void RemoveRandomNodes() {
        List<List<DungeonNode>> levelNodes = graph.GetLevelNodes();
        for (int level = 0; level < levelNodes.Count; level++) {
            List<DungeonNode> currentLevel = levelNodes[level];
            int nodesRemoveAmount = currentLevel.Count - settings.minNodesPerLevel;
            if (nodesRemoveAmount <= 0) continue;

            int nodesToRemove = random.Next(0, nodesRemoveAmount); 


            if (nodesToRemove > 0) {
                List<int> indices = Enumerable.Range(0, currentLevel.Count - 1).ToList();
                Shuffle(indices);

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
            int nodesToAdd = random.Next(0, amountToAdd);

            if (nodesToAdd > 0) {
                for (int i = 0; i < nodesToAdd; i++) {
                    DungeonNode newNode = new DungeonNode(graph.GetNextNodeId(), new Vector2(level, currentLevel.Count));
                    newNode.level = level;
                    currentLevel.Add(newNode);
                }
            }
        }
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
