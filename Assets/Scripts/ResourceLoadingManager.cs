using Cysharp.Threading.Tasks;
using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ResourceLoadingManager {
    public event Action OnResourcesLoaded;
    public event Action<float> OnLoadingProgressChanged;
    private List<IResourceLoader> _resourceLoaders = new();

    // Зберігаємо стан останнього/поточного завантаження
    private LocationData _currentLoadingLocation;
    private UniTaskCompletionSource<bool> _currentLoadingTask;

    public void RegisterResourceLoader(IResourceLoader loader) {
        _resourceLoaders.Add(loader);
        _resourceLoaders = _resourceLoaders
            .OrderByDescending(l => l.LoadPriority)
            .ToList();

        // Якщо є активне завантаження, додамо і цей лоадер до нього
        if (_currentLoadingLocation != null) {
            TryLoadResourcesForLoader(loader, _currentLoadingLocation).Forget();
        }
    }

    public void UnregisterResourceLoader(IResourceLoader loader) {
        _resourceLoaders.Remove(loader);
    }

    private async UniTask TryLoadResourcesForLoader(IResourceLoader loader, LocationData locationData) {
        AssetLabelReference assetLabel = locationData.assetLabel;
        if (loader.HasResources(assetLabel)) return;

        try {
            float loaderProgress = 0;
            var progress = new Progress<float>(value => {
                loaderProgress = value;
                // Тут можна реалізувати більш складну логіку прогресу
                OnLoadingProgressChanged?.Invoke(loaderProgress);
            });
            await loader.LoadResources(assetLabel, progress);
        } catch (Exception e) {
            Debug.LogError($"Resource loader {loader.GetType().Name} failed: {e.Message}");
        }
    }

    public async UniTask<bool> LoadResourcesForLocation(LocationData locationData) {
        // Якщо вже йде завантаження для цієї локації, повертаємо поточне завдання
        if (_currentLoadingLocation == locationData && _currentLoadingTask != null) {
            return await _currentLoadingTask.Task;
        }

        // Початок нового завантаження
        _currentLoadingLocation = locationData;
        _currentLoadingTask = new UniTaskCompletionSource<bool>();

        try {
            int loadersCount = _resourceLoaders.Count;

            // Запускаємо завантаження для всіх наявних лоадерів
            var loadingTasks = _resourceLoaders
                .Select(loader => TryLoadResourcesForLoader(loader, locationData))
                .ToArray();

            // Чекаємо на завершення всіх завантажень
            await UniTask.WhenAll(loadingTasks);

            OnResourcesLoaded?.Invoke();
            _currentLoadingTask.TrySetResult(true);
            return true;
        } catch (Exception ex) {
            Debug.LogError($"Resource loading failed: {ex}");
            _currentLoadingTask.TrySetException(ex);
            throw;
        } finally {
            if (_currentLoadingLocation == locationData) {
                _currentLoadingLocation = null;
            }
        }
    }

    public bool IsLocationLoaded(LocationData locationData) {
        if (_resourceLoaders.Count == 0) return true;

        foreach (var loader in _resourceLoaders) {
            if (!loader.HasResources(locationData.assetLabel)) {
                return false;
            }
        }
        return true;
    }

    public bool HasLoaders() {
        return !_resourceLoaders.IsEmpty();
    }
}