using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using ModestTree;

public class ResourceLoadingManager {
    public event Action OnResourcesLoaded;
    public event Action<float> OnLoadingProgressChanged;

    private List<IResourceLoader> _resourceLoaders = new();

    public void RegisterResourceLoader(IResourceLoader loader) {
        _resourceLoaders.Add(loader);
        _resourceLoaders = _resourceLoaders
            .OrderByDescending(l => l.LoadPriority)
            .ToList();
    }

    public void UnregisterResourceLoader(IResourceLoader loader) {
        _resourceLoaders.Remove(loader);
    }

    public async UniTask LoadResourcesForLocation(LocationData locationData) {
        float totalProgress = 0f;
        int loadersCount = _resourceLoaders.Count;

        foreach (var loader in _resourceLoaders) {
            if (loader.HasLocationData(locationData)) continue;

            try {
                float loaderProgress = 0;
                var progress = new Progress<float>(value => {
                    loaderProgress = value;
                    OnLoadingProgressChanged?.Invoke((totalProgress + loaderProgress) / loadersCount);
                });

                await loader.LoadResources(locationData, progress);
                totalProgress += 1f;
            } catch (Exception e) {
                Debug.LogError($"Resource loader {loader.GetType().Name} failed: {e.Message}");
            }
        }

        OnResourcesLoaded?.Invoke();
    }

    public bool IsLocationLoaded(LocationData locationData) {
        bool isLocationLoaded = true;
        foreach (var loader in _resourceLoaders) {
            if (!loader.HasLocationData(locationData)) {
                isLocationLoaded = false;
                break;
            }
        }
        return isLocationLoaded;
    }

    public bool HasLoaders() {
        return !_resourceLoaders.IsEmpty();
    }
}