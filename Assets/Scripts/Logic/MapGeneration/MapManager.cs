using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class MapManager : MonoBehaviour {
    [SerializeField] private MapSO firstLevelMap; // ����������� ����� ��� ������� ������

    private MapGraph currentMap;

    // ������������ ResourceManager
    [Inject] private AddressablesResourceManager resourceManager;
    public void Construct(AddressablesResourceManager resourceManager) {
        this.resourceManager = resourceManager;
    }

    private List<RoomSO> roomTemplates;

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        InitializeMap(scene.name == "Level1");
    }

    public void InitializeMap(bool isTutorial = false) {
        if (isTutorial) {
            CreateMapFromPrebuilt(firstLevelMap);
        } else {
            GenerateRandomMap();
        }
    }

    public void CreateMapFromPrebuilt(MapSO prebuiltMap) {
        MapGraph map = new MapGraph();

        // �������� ������ �� ������ � ���������� ���������� �����
        MapGraph.Node previousNode = null;
        foreach (var roomSO in prebuiltMap.Rooms) {
            // ������ ����� ������ �� �����
            MapGraph.Node currentNode = map.AddNode(roomSO);

            // ���� �� �� ����� ������, ��'����� �� � �����������
            if (previousNode != null) {
                previousNode.AddConnection(currentNode); // ��'����� ��������� ������ � ��������
            }

            // ��������� ��������� ������ ��� �������� ��������
            previousNode = currentNode;
        }

        currentMap = map;
    }

    private void GenerateRandomMap() {

        // ������������ ��� ����� ����� ResourceManager
        roomTemplates = resourceManager.GetAllResources<RoomSO>(ResourceType.ROOMS);
        // ����� ��� ��������� ��������� ���� �� ������� RoomSO
    }
}


public class MapGenerator {
    public MapGraph GenerateMap(RoomSO startRoom, RoomSO bossRoom, RoomSO[] randomRooms, int maxDepth, int maxBranches) {
        MapGraph map = new MapGraph();

        // ������ �������� ������
        MapGraph.Node startNode = map.AddNode(startRoom);

        // �������� ������ ������
        GenerateBranches(startNode, randomRooms, maxDepth - 1, maxBranches, 1, out MapGraph.Node lastNode);

        // ������ ������ � �����
        MapGraph.Node bossNode = map.AddNode(bossRoom);
        lastNode.AddConnection(bossNode);

        return map;
    }

    private void GenerateBranches(MapGraph.Node currentNode, RoomSO[] randomRooms, int remainingDepth, int maxBranches, int currentDepth, out MapGraph.Node lastNode) {
        if (remainingDepth <= 0) {
            lastNode = currentNode; // ��������� ������� �����
            return;
        }

        int branches = Random.Range(1, maxBranches + 1);
        List<MapGraph.Node> childNodes = new List<MapGraph.Node>();

        // ����������� ���������� �����
        lastNode = null;

        for (int i = 0; i < branches; i++) {
            RoomSO randomRoom = randomRooms[Random.Range(0, randomRooms.Length)];
            MapGraph.Node newNode = new MapGraph.Node(randomRoom);
            currentNode.AddConnection(newNode);
            childNodes.Add(newNode);

            // ���������� �������� ������ ��� ����� ����
            GenerateBranches(newNode, randomRooms, remainingDepth - 1, maxBranches, currentDepth + 1, out lastNode);
        }

        // ���� ���� ��� ���� lastNode �� �� ��� ���������, �� �������� ���� ��������
        if (lastNode == null) {
            lastNode = childNodes.Count > 0 ? childNodes[childNodes.Count - 1] : currentNode;
        }
    }

}
