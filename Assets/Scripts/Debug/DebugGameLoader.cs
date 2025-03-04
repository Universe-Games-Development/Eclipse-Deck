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
            Debug.Log("������������ �������...");
            await LoadResources(currentLocation);

            Debug.Log("������������ �����...");
            if (scenePrefab != null) {
                Instantiate(scenePrefab);
            } else {
                Debug.LogError("scenePrefab �� �����������!");
            }
        } catch (System.Exception ex) {
            Debug.LogError($"������� ��� �����������: {ex.Message}");
        }
    }


    async UniTask LoadResources(Location currentLocation) {
        await assetLoader.LoadLocationAssets(currentLocation);
        // ��� ����� ������������� ������� ����� Addressables ��� Resources.LoadAsync
        Debug.Log("������� ����������!");
    }
}
