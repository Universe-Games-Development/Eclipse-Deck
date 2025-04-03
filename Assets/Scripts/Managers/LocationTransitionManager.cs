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
    
    private Dictionary<LocationType, LocationData> _locationDataByType = new();
    private List<LocationData> _orderedPlayableLocations = new();
    private ResourceLoadingManager _resourceLoadingManager;
    public LocationData CurrentLocationData { get; private set; }
    private LoadingPhase _currentPhase = LoadingPhase.Complete;

    public LocationTransitionManager(
       List<LocationData> locationDatas,
       ResourceLoadingManager resourceLoadingManager) {
        _resourceLoadingManager = resourceLoadingManager;
        InitializeLocationData(locationDatas);
        SceneManager.sceneLoaded += HandleSceneLoaded;
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }


    // Ensures load resourses
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) {
        LocationData locationData = _locationDataByType.Values
            .FirstOrDefault(loc => loc.sceneName == scene.name);

        if (!_resourceLoadingManager.IsLocationLoaded(locationData)) {
            _resourceLoadingManager.LoadResourcesForLocation(locationData).Forget();
        }

        CurrentLocationData = locationData;
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
        if (CurrentLocationData != null) {
            listener.Invoke(CurrentLocationData, _currentPhase);
        }
    }

    private void InvokeTransitionEvent(LocationData data, LoadingPhase phase) {
        foreach (var wr in _transitionListeners.ToArray()) {
            if (wr.TryGetTarget(out var listener)) {
                listener.Invoke(data, phase);
            }
        }
    }

    public async UniTask TransitionToLocation(LocationType locationType) {
        try {
            var locationData = _locationDataByType[locationType];
            await _resourceLoadingManager.LoadResourcesForLocation(locationData);
            await SceneManager.LoadSceneAsync(locationData.sceneName).ToUniTask();
            InvokeTransitionEvent(CurrentLocationData, _currentPhase);
            
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
