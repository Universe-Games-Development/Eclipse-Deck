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

    [SerializeField] private string cardResourcePath = "Cards"; // Шлях до ресурсів з картами
    [SerializeField] private string enemyResourcePath = "Enemies"; // Шлях до ресурсів з ворогами
    [SerializeField] private string mapInfoResourcePath = "MapInfos"; // Шлях до ресурсів з інформацією про карту
    [SerializeField] private string roomResourcePath = "Rooms"; // Шлях до ресурсів з кімнатами
    [SerializeField] private string creaturesResourcePath = "Creatures"; // Шлях до ресурсів з кімнатами
    // Словник для збереження завантажених ресурсів
    private Dictionary<ResourceType, bool> resourcesLoaded = new Dictionary<ResourceType, bool>();

    // Словник для зберігання списків ресурсів
    private Dictionary<ResourceType, List<Object>> resourceDictionary = new Dictionary<ResourceType, List<Object>>();

    public ResourceManager() {
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType))) {
            resourcesLoaded[type] = false;
        }
    }

    // Завантажуємо ресурси при першому зверненні
    private void LoadResources<T>(ResourceType resourceType, string path) where T : Object {
        // Якщо ресурси вже завантажені, не потрібно їх завантажувати знову
        if (resourcesLoaded[resourceType]) {
            return;
        }

        List<T> targetList = new List<T>();

        // Завантаження ресурсів з папки
        T[] resources = Resources.LoadAll<T>(path);

        if (resources != null && resources.Length > 0) {
            targetList.AddRange(resources);
            resourceDictionary[resourceType] = new List<Object>(targetList);
            resourcesLoaded[resourceType] = true; // Позначаємо ресурси як завантажені
            Debug.Log($"Loaded {resources.Length} {typeof(T).Name} resources from '{path}'.");
        } else {
            Debug.LogWarning($"No {typeof(T).Name} resources found at path: {path}");
        }
    }

    // Отримуємо випадковий ресурс з певного типу
    public T GetRandomResource<T>(ResourceType resourceType) where T : Object {
        // Перевіряємо чи ресурси вже завантажені, якщо ні - завантажуємо їх
        LoadResources<T>(resourceType, GetResourcePath(resourceType));

        if (!resourceDictionary.ContainsKey(resourceType) || resourceDictionary[resourceType].Count == 0) {
            Debug.LogWarning($"No {typeof(T).Name} resources available.");
            return null;
        }

        List<T> resourceList = resourceDictionary[resourceType].ConvertAll(resource => resource as T);
        int randomIndex = Random.Range(0, resourceList.Count);
        return resourceList[randomIndex];
    }

    // Отримуємо всі ресурси певного типу
    public List<T> GetAllResources<T>(ResourceType resourceType) where T : Object {
        // Перевіряємо чи ресурси вже завантажені, якщо ні - завантажуємо їх
        LoadResources<T>(resourceType, GetResourcePath(resourceType));

        if (!resourceDictionary.ContainsKey(resourceType)) {
            Debug.LogWarning($"No {typeof(T).Name} resources available.");
            return null;
        }

        List<T> resourceList = resourceDictionary[resourceType].ConvertAll(resource => resource as T);
        return resourceList;
    }

    // Отримуємо ресурс за ID
    public T GetResourceByID<T>(string resourceID, ResourceType resourceType) where T : Object {
        // Перевіряємо чи ресурси вже завантажені, якщо ні - завантажуємо їх
        LoadResources<T>(resourceType, GetResourcePath(resourceType));

        if (!resourceDictionary.ContainsKey(resourceType)) {
            Debug.LogWarning($"No {typeof(T).Name} resources found.");
            return null;
        }

        List<T> resourceList = resourceDictionary[resourceType].ConvertAll(resource => resource as T);
        return resourceList.Find(resource => resource.name == resourceID);
    }

    // Допоміжний метод для отримання шляху до ресурсу в залежності від типу
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