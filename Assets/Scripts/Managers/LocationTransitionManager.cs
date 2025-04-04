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

    private readonly Dictionary<LoadingPhase, List<WeakReference<Func<LocationData, UniTask>>>> _transitionListenersByPhase = new();
    public LocationTransitionManager(LocationsData locationsData) {
        InitializeLocationData(locationsData);

        foreach (LoadingPhase phase in Enum.GetValues(typeof(LoadingPhase))) {
            _transitionListenersByPhase[phase] = new List<WeakReference<Func<LocationData, UniTask>>>();
        }

        SceneManager.sceneLoaded += HandleSceneLoaded;
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void InitializeLocationData(LocationsData locationDatas) {
        List<LocationData> allLocations = locationDatas.locationDatas;
        _orderedPlayableLocations = allLocations
            .Where(loc => loc.isPlayableLevel)
            .OrderBy(loc => loc.orderInSequence)
            .Distinct()
            .ToList();
        foreach (var locationData in _orderedPlayableLocations) {
            _locationDataByType[locationData.locationType] = locationData;
        }
    }

    public LocationData GetNextLocation() {
        LocationData currentLocationData = GetSceneLocation();
        if (currentLocationData == null) return _orderedPlayableLocations.FirstOrDefault();

        var currentIndex = _orderedPlayableLocations.IndexOf(currentLocationData);
        return currentIndex < _orderedPlayableLocations.Count - 1
            ? _orderedPlayableLocations[currentIndex + 1]
            : _orderedPlayableLocations.FirstOrDefault();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Ensures that the LoadResources phase is called when the scene is loaded
        InvokeTransitionEvent(GetSceneLocation(), LoadingPhase.LoadResources).Forget();
    }

    public async UniTask TransitionToLocation(LocationType locationType) {
        try {
            var locationData = _locationDataByType[locationType];
            await InvokeTransitionEvent(locationData, LoadingPhase.PreLoad);
            await InvokeTransitionEvent(locationData, LoadingPhase.LoadResources);

            await locationData.sceneReference.LoadSceneAsync(LoadSceneMode.Single, true);

            await InvokeTransitionEvent(locationData, LoadingPhase.EnterLocation);
            await InvokeTransitionEvent(locationData, LoadingPhase.Complete);
        } catch (Exception e) {
            Debug.LogError($"Location transition failed: {e}");
        }
    }

    // Метод для реєстрації підписника на конкретну фазу
    public void RegisterListener(LoadingPhase phase, Func<LocationData, UniTask> listener) {
        if (!_transitionListenersByPhase.ContainsKey(phase))
            _transitionListenersByPhase[phase] = new List<WeakReference<Func<LocationData, UniTask>>>();

        _transitionListenersByPhase[phase].Add(new WeakReference<Func<LocationData, UniTask>>(listener));

        LocationData currentLocationData = GetSceneLocation();
        // if we already have a current location, invoke the listener immediately
        if (currentLocationData != null) {
            switch (phase) {
                case LoadingPhase.LoadResources:
                case LoadingPhase.Complete:
                    listener(currentLocationData).Forget();
                    break;
                default:
                    break;
            }
        }
    }

    public void RegisterListenerForAllPhases(Func<LocationData, UniTask> listener) {
        foreach (LoadingPhase phase in Enum.GetValues(typeof(LoadingPhase))) {
            RegisterListener(phase, listener);
        }
    }

    public void UnregisterListener(Func<LocationData, UniTask> listener) {
        foreach (var phaseListeners in _transitionListenersByPhase.Values) {
            for (int i = phaseListeners.Count - 1; i >= 0; i--) {
                if (phaseListeners[i].TryGetTarget(out var target) && target == listener) {
                    phaseListeners.RemoveAt(i);
                }
            }
        }
    }

    private async UniTask InvokeTransitionEvent(LocationData data, LoadingPhase phase) {
        if (!_transitionListenersByPhase.TryGetValue(phase, out var listeners))
            return;

        foreach (var weakRef in listeners.ToArray()) {
            if (weakRef.TryGetTarget(out var listener)) {
                await listener(data);
            } else {
                // Deleting dead references
                _transitionListenersByPhase[phase].Remove(weakRef);
            }
        }
    }

    public LocationData GetSceneLocation(Scene scene) {
        LocationData locationData = _locationDataByType.Values
            .FirstOrDefault(loc => loc.sceneReference.SceneName == scene.name);
        if (locationData == null) {
            Debug.LogError($"LocationData not found for scene: {scene.name}");
            return null;
        }
        return locationData;
    }

    public LocationData GetSceneLocation() {
        Scene scene = SceneManager.GetActiveScene();
        return GetSceneLocation(scene);
    }
}