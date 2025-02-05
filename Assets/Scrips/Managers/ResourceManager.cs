using System.Collections.Generic;
using UnityEngine;

public class ResourceManager {
    [Header("Card Data")]
    public List<CardSO> cardDataList = new List<CardSO>();

    [Header("Enemy Data")]
    public List<EnemySO> enemyDataList = new List<EnemySO>();

    [Header("Map Info Data")]
    public List<MapInfoSO> mapInfoDataList = new List<MapInfoSO>();

    [Header("Room Data")]
    public List<RoomSO> roomDataList = new List<RoomSO>();

    [SerializeField] private string cardResourcePath = "Cards"; // ���� �� ������� � �������
    [SerializeField] private string enemyResourcePath = "Enemies"; // ���� �� ������� � ��������
    [SerializeField] private string mapInfoResourcePath = "MapInfos"; // ���� �� ������� � ����������� ��� �����
    [SerializeField] private string roomResourcePath = "Rooms"; // ���� �� ������� � ��������
    [SerializeField] private string creaturesResourcePath = "Creatures"; // ���� �� ������� � ��������
    // ������� ��� ���������� ������������ �������
    private Dictionary<ResourceType, bool> resourcesLoaded = new Dictionary<ResourceType, bool>();

    // ������� ��� ��������� ������ �������
    private Dictionary<ResourceType, List<Object>> resourceDictionary = new Dictionary<ResourceType, List<Object>>();

    public ResourceManager() {
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType))) {
            resourcesLoaded[type] = false;
        }
    }

    // ����������� ������� ��� ������� ��������
    private void LoadResources<T>(ResourceType resourceType, string path) where T : Object {
        // ���� ������� ��� ����������, �� ������� �� ������������� �����
        if (resourcesLoaded[resourceType]) {
            return;
        }

        List<T> targetList = new List<T>();

        // ������������ ������� � �����
        T[] resources = Resources.LoadAll<T>(path);

        if (resources != null && resources.Length > 0) {
            targetList.AddRange(resources);
            resourceDictionary[resourceType] = new List<Object>(targetList);
            resourcesLoaded[resourceType] = true; // ��������� ������� �� ����������
            Debug.Log($"Loaded {resources.Length} {typeof(T).Name} resources from '{path}'.");
        } else {
            Debug.LogWarning($"No {typeof(T).Name} resources found at path: {path}");
        }
    }

    // �������� ���������� ������ � ������� ����
    public T GetRandomResource<T>(ResourceType resourceType) where T : Object {
        // ���������� �� ������� ��� ����������, ���� � - ����������� ��
        LoadResources<T>(resourceType, GetResourcePath(resourceType));

        if (!resourceDictionary.ContainsKey(resourceType) || resourceDictionary[resourceType].Count == 0) {
            Debug.LogWarning($"No {typeof(T).Name} resources available.");
            return null;
        }

        List<T> resourceList = resourceDictionary[resourceType].ConvertAll(resource => resource as T);
        int randomIndex = Random.Range(0, resourceList.Count);
        return resourceList[randomIndex];
    }

    // �������� �� ������� ������� ����
    public List<T> GetAllResources<T>(ResourceType resourceType) where T : Object {
        // ���������� �� ������� ��� ����������, ���� � - ����������� ��
        LoadResources<T>(resourceType, GetResourcePath(resourceType));

        if (!resourceDictionary.ContainsKey(resourceType)) {
            Debug.LogWarning($"No {typeof(T).Name} resources available.");
            return null;
        }

        List<T> resourceList = resourceDictionary[resourceType].ConvertAll(resource => resource as T);
        return resourceList;
    }

    // �������� ������ �� ID
    public T GetResourceByID<T>(string resourceID, ResourceType resourceType) where T : Object {
        // ���������� �� ������� ��� ����������, ���� � - ����������� ��
        LoadResources<T>(resourceType, GetResourcePath(resourceType));

        if (!resourceDictionary.ContainsKey(resourceType)) {
            Debug.LogWarning($"No {typeof(T).Name} resources found.");
            return null;
        }

        List<T> resourceList = resourceDictionary[resourceType].ConvertAll(resource => resource as T);
        return resourceList.Find(resource => resource.name == resourceID);
    }

    // ��������� ����� ��� ��������� ����� �� ������� � ��������� �� ����
    private string GetResourcePath(ResourceType resourceType) {
        switch (resourceType) {
            case ResourceType.CARDS:
                return cardResourcePath;
            case ResourceType.ENEMIES:
                return enemyResourcePath;
            case ResourceType.MAP_INFO:
                return mapInfoResourcePath;
            case ResourceType.ROOMS:
                return roomResourcePath;
            case ResourceType.CREATURE:
                return creaturesResourcePath;
            default:
                return string.Empty;
        }
    }
}