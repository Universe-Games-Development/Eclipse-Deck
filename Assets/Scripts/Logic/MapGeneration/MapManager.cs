using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class MapManager : MonoBehaviour {
    [SerializeField] private MapSO firstLevelMap; // Построенная карта для первого уровня

    // Впровадження ResourceManager
    [Inject] private AddressablesResourceManager resourceManager;
    public void Construct(AddressablesResourceManager resourceManager) {
        this.resourceManager = resourceManager;
    }

    private List<RoomData> roomTemplates;

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        InitializeMap(scene.name == "Level1");
    }

    public void InitializeMap(bool isTutorial = false) {
        
    }
}

