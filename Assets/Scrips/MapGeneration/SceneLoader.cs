using UnityEngine;
using Zenject;

public class SceneLoader : MonoBehaviour
{
    [Inject] private AssetLoader assetLoader;

    [Inject]  private LevelLoadingManager levelManager;


    
    public async void ChangeLocation(Location location) {
        Location loadingLocation = location == Location.MainMenu ? Location.GameLoading : Location.Loading;

        if (!assetLoader.HasActualData(location)) {
            levelManager.LoadLocation(loadingLocation);
            await assetLoader.LoadLocationAssets(location);
        }

        levelManager.LoadLocation(location);
    }
}
