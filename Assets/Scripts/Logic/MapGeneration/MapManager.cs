using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class MapManager : MonoBehaviour {
    [SerializeField] private MapSO firstLevelMap; // Построенная карта для первого уровня

    private MapGraph currentMap;

    // Впровадження ResourceManager
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

        // Спочатку додаємо всі кімнати з попередньо побудованої карти
        MapGraph.Node previousNode = null;
        foreach (var roomSO in prebuiltMap.Rooms) {
            // Додаємо кожну кімнату до графу
            MapGraph.Node currentNode = map.AddNode(roomSO);

            // Якщо це не перша кімната, зв'язуємо її з попередньою
            if (previousNode != null) {
                previousNode.AddConnection(currentNode); // Зв'язуємо попередню кімнату з поточною
            }

            // Оновлюємо попередню кімнату для наступної ітерації
            previousNode = currentNode;
        }

        currentMap = map;
    }

    private void GenerateRandomMap() {

        // Завантаження всіх кімнат через ResourceManager
        roomTemplates = resourceManager.GetAllResources<RoomSO>(ResourceType.ROOMS);
        // Логіка для випадкової генерації карт із шаблонів RoomSO
    }
}


public class MapGenerator {
    public MapGraph GenerateMap(RoomSO startRoom, RoomSO bossRoom, RoomSO[] randomRooms, int maxDepth, int maxBranches) {
        MapGraph map = new MapGraph();

        // Додаємо стартову кімнату
        MapGraph.Node startNode = map.AddNode(startRoom);

        // Генеруємо проміжні кімнати
        GenerateBranches(startNode, randomRooms, maxDepth - 1, maxBranches, 1, out MapGraph.Node lastNode);

        // Додаємо кімнату з босом
        MapGraph.Node bossNode = map.AddNode(bossRoom);
        lastNode.AddConnection(bossNode);

        return map;
    }

    private void GenerateBranches(MapGraph.Node currentNode, RoomSO[] randomRooms, int remainingDepth, int maxBranches, int currentDepth, out MapGraph.Node lastNode) {
        if (remainingDepth <= 0) {
            lastNode = currentNode; // Повертаємо останній вузол
            return;
        }

        int branches = Random.Range(1, maxBranches + 1);
        List<MapGraph.Node> childNodes = new List<MapGraph.Node>();

        // Ініціалізація останнього вузла
        lastNode = null;

        for (int i = 0; i < branches; i++) {
            RoomSO randomRoom = randomRooms[Random.Range(0, randomRooms.Length)];
            MapGraph.Node newNode = new MapGraph.Node(randomRoom);
            currentNode.AddConnection(newNode);
            childNodes.Add(newNode);

            // Рекурсивно генеруємо кімнати для кожної гілки
            GenerateBranches(newNode, randomRooms, remainingDepth - 1, maxBranches, currentDepth + 1, out lastNode);
        }

        // Якщо після всіх гілок lastNode ще не був присвоєний, ми присвоїмо його значення
        if (lastNode == null) {
            lastNode = childNodes.Count > 0 ? childNodes[childNodes.Count - 1] : currentNode;
        }
    }

}
