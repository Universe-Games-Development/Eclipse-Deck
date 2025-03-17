using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using Zenject;
using System.Linq;

public class LocationManager : MonoBehaviour {

    public event Action<LocationData> OnLocationChanged;
    public Func<LocationData, UniTask> OnLocationPreLoad;
    public event Action<LocationData> OnLocationLoaded;

    [Header("Settings")]
    [SerializeField] private List<LocationData> allLocations = new List<LocationData>();
    [SerializeField] private LocationType defaultLocation = LocationType.MainMenu;

    [Header("WorkflowMode data")]
    private Dictionary<LocationType, LocationData> _locationDataByType = new Dictionary<LocationType, LocationData>();
    private Dictionary<string, LocationData> _locationDataBySceneName = new Dictionary<string, LocationData>();
    private List<LocationData> _orderedPlayableLocations = new List<LocationData>();
    private LocationData _currentLocationData;

    [Inject] private DiContainer _container;

    private void Awake() {
        InitializeLocationData();
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateCurrentLocation();
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeLocationData() {
        // Заповнюємо словники для швидкого пошуку
        foreach (var location in allLocations) {
            _locationDataByType[location.locationType] = location;
            _locationDataBySceneName[location.sceneName] = location;
        }

        // Створюємо впорядкований список локацій для проходження
        _orderedPlayableLocations = allLocations
            .Where(loc => loc.isPlayableLevel)
            .OrderBy(loc => loc.orderInSequence)
            .ToList();
    }

    public LocationData GetLocationData(LocationType locationType) {
        if (_locationDataByType.TryGetValue(locationType, out LocationData data)) {
            return data;
        }
        Debug.LogWarning($"Location data not found for {locationType}");
        return null;
    }

    public LocationData GetLocationDataBySceneName(string sceneName) {
        if (_locationDataBySceneName.TryGetValue(sceneName, out LocationData data)) {
            return data;
        }
        Debug.LogWarning($"Location data not found for scene {sceneName}");
        return null;
    }

    public LocationData GetCurrentLocationData() {
        if (_currentLocationData == null) {
            UpdateCurrentLocation();
        }
        return _currentLocationData;
    }

    public void UpdateCurrentLocation() {
        string currentSceneName = SceneManager.GetActiveScene().name;
        LocationData data = GetLocationDataBySceneName(currentSceneName);

        if (data != null) {
            _currentLocationData = data;
        } else {
            // Якщо не знайдено, використовуємо дефолтну локацію
            _currentLocationData = GetLocationData(defaultLocation);
            //Debug.LogWarning($"Unknown scene: {currentSceneName}, using default location");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        UpdateCurrentLocation();
        OnLocationLoaded?.Invoke(_currentLocationData);
    }

    public LocationData GetNextLocation() {
        return GetNextLocationFor(_currentLocationData.locationType);
    }

    public LocationData GetNextLocationFor(LocationType locationType) {
        // Знаходимо поточну локацію в упорядкованому списку
        var currentLocation = GetLocationData(locationType);
        if (currentLocation == null) return null;

        int currentIndex = _orderedPlayableLocations.IndexOf(currentLocation);

        // Якщо локація знайдена і це не остання локація
        if (currentIndex >= 0 && currentIndex < _orderedPlayableLocations.Count - 1) {
            return _orderedPlayableLocations[currentIndex + 1];
        }

        // Якщо це остання локація або не знайдена в списку, повертаємось до першої
        return _orderedPlayableLocations.FirstOrDefault();
    }

    public async UniTask LoadLocation(LocationType locationType) {
        var locationData = GetLocationData(locationType);
        if (locationData != null) {
            await LoadLocationByData(locationData);
        } else {
            Debug.LogError($"Cannot load location {locationType}: data not found");
        }
    }

    public async UniTask LoadNextLocation() {
        var nextLocation = GetNextLocation();
        if (nextLocation != null) {
            await LoadLocationByData(nextLocation);
        } else {
            Debug.LogError("No next location available");
        }
    }

    private async UniTask LoadLocationByData(LocationData locationData) {
        await (OnLocationPreLoad?.Invoke(locationData) ?? UniTask.CompletedTask);

        SceneManager.LoadScene(locationData.sceneName);

        OnLocationChanged?.Invoke(locationData);
    }
}