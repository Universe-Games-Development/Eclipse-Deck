using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class DebugGameLoader : MonoBehaviour
{
    [SerializeField] private GameObject scenePrefab;
    [Inject] AssetLoader assetLoader;
    [Inject] LevelLoadingManager levelManager;

    private void Awake() {
        Location currentLocation = levelManager.GetCurrentLocation();
        bool isActualRes = assetLoader.HasActualData(currentLocation);
        if (!isActualRes) {
            LoadGame(currentLocation).Forget();
        }
    }
    async UniTask LoadGame(Location currentLocation) {
        try {
            Debug.Log("Завантаження ресурсів...");
            await LoadResources(currentLocation);

            Debug.Log("Інстанціюємо сцену...");
            if (scenePrefab != null) {
                Instantiate(scenePrefab);
            } else {
                Debug.LogError("scenePrefab не призначений!");
            }
        } catch (System.Exception ex) {
            Debug.LogError($"Помилка при завантаженні: {ex.Message}");
        }
    }


    async UniTask LoadResources(Location currentLocation) {
        await assetLoader.LoadLocationAssets(currentLocation);
        // Тут можна завантажувати ресурси через Addressables або Resources.LoadAsync
        Debug.Log("Ресурси завантажені!");
    }
}
