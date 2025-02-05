using UnityEngine;
using Zenject;
using static UnityEditor.FilePathAttribute;

public class SceneLoader : MonoBehaviour
{
    [Inject] private AssetLoader assetLoader;

    [Inject] private LevelManager levelManager;

    private void Construct(LevelManager levelManager) {
        levelManager.OnLocationLoad += PrepareLocation;
    }
    
    public async void PrepareLocation(Location location) {
        Location loadingLocation = location == Location.MainMenu ? Location.GameLoading : Location.Loading;

        if (!assetLoader.HasActualData(location)) {
            levelManager.LoadLocation(loadingLocation);
            await assetLoader.LoadLocationAssets(location);
        }

        levelManager.LoadLocation(location);
    }
}
