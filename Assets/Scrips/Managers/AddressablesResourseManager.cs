using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks; // Потрібно, якщо використовуєш UniTask

public class AddressablesResourceManager {
    private Dictionary<ResourceType, AsyncOperationHandle<IList<Object>>> resourceHandles = new Dictionary<ResourceType, AsyncOperationHandle<IList<Object>>>();
    private Dictionary<ResourceType, List<Object>> resourceDictionary = new Dictionary<ResourceType, List<Object>>();

    public async UniTask LoadResourcesAsync(ResourceType resourceType, string label) {
        if (resourceHandles.ContainsKey(resourceType)) return; // Уже завантажено

        var handle = Addressables.LoadAssetsAsync<Object>(label, null); // Завантажуємо за лейблом
        await handle.Task; // Очікуємо завершення завантаження

        if (handle.Status == AsyncOperationStatus.Succeeded) {
            resourceDictionary[resourceType] = new List<Object>(handle.Result);
            resourceHandles[resourceType] = handle;
            Debug.Log($"Loaded {handle.Result.Count} resources for {resourceType}.");
        } else {
            Debug.LogError($"Failed to load resources for {resourceType}.");
        }
    }

    public T GetRandomResource<T>(ResourceType resourceType) where T : Object {
        if (!resourceDictionary.ContainsKey(resourceType) || resourceDictionary[resourceType].Count == 0) {
            Debug.LogWarning($"No {typeof(T).Name} resources available.");
            return null;
        }

        List<T> resourceList = resourceDictionary[resourceType].ConvertAll(resource => resource as T);
        return resourceList[Random.Range(0, resourceList.Count)];
    }

    public List<T> GetAllResources<T>(ResourceType resourceType) where T : Object {
        if (!resourceDictionary.ContainsKey(resourceType)) return null;
        return resourceDictionary[resourceType].ConvertAll(resource => resource as T);
    }

    public async UniTask<T> GetResourceByID<T>(string resourceID, ResourceType resourceType) where T : Object {
        if (!resourceDictionary.ContainsKey(resourceType)) {
            await LoadResourcesAsync(resourceType, GetResourceLabel(resourceType));
        }

        List<T> resourceList = resourceDictionary[resourceType].ConvertAll(resource => resource as T);
        return resourceList.Find(resource => resource.name == resourceID);
    }

    private string GetResourceLabel(ResourceType resourceType) {
        return resourceType switch {
            ResourceType.CARDS => "Cards",
            ResourceType.ENEMIES => "Enemies",
            ResourceType.MAP_INFO => "MapInfos",
            ResourceType.ROOMS => "Rooms",
            ResourceType.CREATURE => "Creatures",
            _ => string.Empty
        };
    }

    public void ReleaseResources(ResourceType resourceType) {
        if (resourceHandles.ContainsKey(resourceType)) {
            Addressables.Release(resourceHandles[resourceType]);
            resourceHandles.Remove(resourceType);
            resourceDictionary.Remove(resourceType);
        }
    }
}