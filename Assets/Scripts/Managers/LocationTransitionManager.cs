using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LoadingPhase {
    PreLoad,
    LoadResources,
    EnterLocation,
    Complete
}

public class LocationTransitionManager {
    private ResourceLoadingManager _loadingManager;

    private readonly Dictionary<LocationType, LocationData> _locationDataByType = new();
    private readonly List<LocationData> _orderedPlayableLocations = new();
    private readonly Dictionary<LoadingPhase, List<WeakReference<Func<LocationData, UniTask>>>> _transitionListenersByPhase = new();

    public LocationTransitionManager(LocationsData locationsData, ResourceLoadingManager loadingManager) {
        _loadingManager = loadingManager;
        InitializeLocationData(locationsData);
        InitializeTransitionListeners();

        SceneManager.sceneLoaded += HandleSceneLoaded;
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    #region Initialization

    private void InitializeLocationData(LocationsData locationsData) {
        var allLocations = locationsData.locationDatas;

        _orderedPlayableLocations.AddRange(
            allLocations
                .Where(loc => loc.isPlayableLevel)
                .OrderBy(loc => loc.orderInSequence)
                .Distinct()
        );

        foreach (var location in _orderedPlayableLocations) {
            _locationDataByType[location.locationType] = location;
        }
    }

    private void InitializeTransitionListeners() {
        foreach (LoadingPhase phase in Enum.GetValues(typeof(LoadingPhase))) {
            _transitionListenersByPhase[phase] = new List<WeakReference<Func<LocationData, UniTask>>>();
        }
    }

    #endregion

    #region Public API

    public LocationData GetNextLocation() {
        var current = GetSceneLocation();
        if (current == null) return _orderedPlayableLocations.FirstOrDefault();

        int currentIndex = _orderedPlayableLocations.IndexOf(current);
        return currentIndex >= 0 && currentIndex < _orderedPlayableLocations.Count - 1
            ? _orderedPlayableLocations[currentIndex + 1]
            : _orderedPlayableLocations.FirstOrDefault();
    }

    public void LocationTransition(int locationIndex) {
        if (locationIndex >= 0 && locationIndex < _orderedPlayableLocations.Count) {
            TransitionToLocation(_orderedPlayableLocations[locationIndex]).Forget();
        } else {
            Debug.LogWarning($"Invalid location index: {locationIndex}");
        }
    }

    public LocationData GetSceneLocation() => GetSceneLocation(SceneManager.GetActiveScene());

    public LocationData GetSceneLocation(Scene scene) {
        var location = _locationDataByType.Values.FirstOrDefault(loc => loc.sceneReference.SceneName == scene.name);
        if (location == null) {
            Debug.LogWarning($"LocationData not found for scene: {scene.name}");
        }
        return location;
    }

    public void RegisterListener(LoadingPhase phase, Func<LocationData, UniTask> listener) {
        _transitionListenersByPhase[phase].Add(new WeakReference<Func<LocationData, UniTask>>(listener));

        // Якщо вже є активна сцена — викликаємо одразу
        var current = GetSceneLocation();
        if (current != null && (phase == LoadingPhase.LoadResources || phase == LoadingPhase.Complete)) {
            listener(current).Forget();
        }
    }

    public void RegisterListenerForAllPhases(Func<LocationData, UniTask> listener) {
        foreach (LoadingPhase phase in Enum.GetValues(typeof(LoadingPhase))) {
            RegisterListener(phase, listener);
        }
    }

    public void UnregisterListener(Func<LocationData, UniTask> listener) {
        foreach (var listeners in _transitionListenersByPhase.Values) {
            for (int i = listeners.Count - 1; i >= 0; i--) {
                if (listeners[i].TryGetTarget(out var target) && target == listener) {
                    listeners.RemoveAt(i);
                }
            }
        }
    }

    #endregion

    #region Transition Logic

    private async UniTask TransitionToLocation(LocationData locationData) {
        try {
            await InvokeTransitionEvent(locationData, LoadingPhase.PreLoad);
            await _loadingManager.LoadResourcesForLocation(locationData);

            if (GetSceneLocation() != locationData) {
                await locationData.sceneReference.LoadSceneAsync(LoadSceneMode.Single, true);
            }


            await InvokeTransitionEvent(locationData, LoadingPhase.EnterLocation);
            await InvokeTransitionEvent(locationData, LoadingPhase.Complete);
        } catch (Exception ex) {
            Debug.LogError($"Location transition failed: {ex}");
        }
    }

    private async UniTask InvokeTransitionEvent(LocationData data, LoadingPhase phase) {
        if (!_transitionListenersByPhase.TryGetValue(phase, out var listeners)) return;

        foreach (var weakRef in listeners.ToList()) {
            if (weakRef.TryGetTarget(out var listener)) {
                await listener(data);
            } else {
                listeners.Remove(weakRef); // Remove garbage collected references
            }
        }
    }

    #endregion

    #region Scene Event Handling

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) {
        var location = GetSceneLocation(scene);
        if (location != null) {
            TransitionToLocation(location).Forget();
        }
    }

    #endregion
}
