using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class LevelManager : MonoBehaviour {

    public Action<Location> OnLocationChanged;
    public Action<Location> OnLocationLoad;

    [Header("Scene data")]
    public Location currentLocation;

    // Масив, що містить локації у правильному порядку
    private Location[] locationOrder = {
        Location.MainMenu,
        Location.Sewers,
        Location.Cave,
        Location.FloodedCave,
        Location.Lab,
        Location.Hell
    };

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Scene scene = SceneManager.GetActiveScene();
        CheckLocation(scene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1) {
        CheckLocation(scene);
    }

    private void CheckLocation(Scene scene) {
        string sceneName = scene.name;
        currentLocation = DetermineLocationByName(sceneName);
        OnLocationChanged?.Invoke(currentLocation);
    }

    private Location DetermineLocationByName(string sceneName) {
        if (LocationMappings.SceneNameToLocation.TryGetValue(sceneName, out Location location)) {
            return location;
        }
        Debug.LogError("Unknown scene name: " + sceneName);
        return Location.MainMenu; // Значення за замовчуванням
    }

    public void LoadLocation(Location location) {
        string locationName = location.ToString();
        SceneManager.LoadScene(locationName);
        OnLocationChanged?.Invoke(currentLocation);
    }

    public void LoadNextLocation() {
        Scene scene = SceneManager.GetActiveScene();
        int currentIndex = scene.buildIndex;
        int nextIndex = currentIndex + 1;

        SceneManager.LoadScene(nextIndex);
        OnLocationChanged?.Invoke(currentLocation);
    }

    public Location GetNextLocation() {
        return GetNextLocationFor(currentLocation);
    }

    public Location GetNextLocationFor(Location currentLocation) {
        int currentIndex = Array.IndexOf(locationOrder, currentLocation);

        // Якщо локація знайдена і це не остання локація
        if (currentIndex >= 0 && currentIndex < locationOrder.Length - 1) {
            return locationOrder[currentIndex + 1];
        }

        return locationOrder[0];
    }
}
