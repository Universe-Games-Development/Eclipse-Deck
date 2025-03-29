using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public enum LoadingPhase {
    PreLoad,
    LoadResources,
    EnterLocation,
    Complete
}

public class LocationTransitionManager {
    public event Action<float> OnLoadingProgressChanged;
    public event Action<bool> OnResourseLoading;
    
    private Dictionary<LocationType, LocationData> _locationDataByType = new();
    private List<LocationData> _orderedPlayableLocations = new();

    public LocationData CurrentLocationData { get; private set; }
    private List<IResourceLoader> _resourceLoaders = new();

    public LocationTransitionManager(
        List<LocationData> locationDatas
        ) {
        InitializeLocationData(locationDatas);
        SceneManager.sceneLoaded += HandleSceneLoaded;
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }


    // Ensures load resourses
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) {
        CurrentLocationData = _locationDataByType.Values
            .FirstOrDefault(loc => loc.sceneName == scene.name);

        bool needUpdate = _resourceLoaders
            .Any(loader => !loader.HasLocationData(CurrentLocationData));

        if (needUpdate) UpdateLocationResources(CurrentLocationData).Forget();
        InvokeTransitionEvent(CurrentLocationData, LoadingPhase.Complete);
    }

    private void InitializeLocationData(List<LocationData> _allLocations) {
        foreach (var location in _allLocations) {
            _locationDataByType[location.locationType] = location;
        }

        _orderedPlayableLocations = _allLocations
            .Where(loc => loc.isPlayableLevel)
            .OrderBy(loc => loc.orderInSequence)
            .ToList();
    }

    private readonly List<WeakReference<Action<LocationData, LoadingPhase>>> _transitionListeners = new();
    public void RegisterListener(Action<LocationData, LoadingPhase> listener) {
        _transitionListeners.Add(new WeakReference<Action<LocationData, LoadingPhase>>(listener));
    }

    private void InvokeTransitionEvent(LocationData data, LoadingPhase phase) {
        foreach (var wr in _transitionListeners.ToArray()) {
            if (wr.TryGetTarget(out var listener)) {
                listener.Invoke(data, phase);
            }
        }
    }

    public void RegisterResourceLoader(IResourceLoader loader) {
        // Додаємо з сортуванням за пріоритетом
        _resourceLoaders.Add(loader);
        _resourceLoaders = _resourceLoaders
            .OrderByDescending(l => l.LoadPriority)
            .ToList();

        if (CurrentLocationData != null)
        loader.LoadResources(CurrentLocationData);
    }

    public void UnregisterResourceLoader(IResourceLoader loader) {
        _resourceLoaders.Remove(loader);
    }

    private async UniTask UpdateLocationResources(LocationData locationData) {
        float totalProgress = 0f;
        int loadersCount = _resourceLoaders.Count;
        OnResourseLoading?.Invoke(true);

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

        OnResourseLoading?.Invoke(false);
    }


    public async UniTask TransitionToLocation(LocationType locationType) {
        try {
            var locationData = _locationDataByType[locationType];
            await UpdateLocationResources(locationData);
            await SceneManager.LoadSceneAsync(locationData.sceneName).ToUniTask();
            InvokeTransitionEvent(CurrentLocationData, LoadingPhase.Complete);
            
        } catch (Exception e) {
            Debug.LogError($"Location transition failed: {e}");
            // Обробка помилок
        }
    }

    public LocationData GetNextLocation() {
        var currentIndex = _orderedPlayableLocations.IndexOf(CurrentLocationData);
        return currentIndex < _orderedPlayableLocations.Count - 1
            ? _orderedPlayableLocations[currentIndex + 1]
            : _orderedPlayableLocations.FirstOrDefault();
    }
}
