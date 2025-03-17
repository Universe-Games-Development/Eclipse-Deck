using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

public class LocationAssetLoader : IAssetLoader {
    public event Action OnAssetsLoaded;

    private CardManager _cardManager;
    private LocationManager locationManager;

    [Inject]
    public void Construct(CardManager cardManager, LocationManager locationManager) {
        _cardManager = cardManager;
        this.locationManager = locationManager;
        locationManager.OnLocationPreLoad += LoadLocationAssets;
    }

    public async UniTask LoadLocationAssets(LocationData locationData) {
        if (locationData == null || locationData.assetLabel == null) {
            Debug.LogWarning($"Invalid location data or asset label for asset loading");
            return;
        }

        try {
            await _cardManager.LoadCardsForLocation(locationData.assetLabel, locationData.locationType);

            if (_cardManager.HasLocationCardData(locationData.locationType)) {
                Debug.Log($"Cards loaded for {locationData.locationType}");
            }

            OnAssetsLoaded?.Invoke();
        } catch (Exception e) {
            Debug.LogError($"Failed to load assets for {locationData.locationType}: {e.Message}");
        }
    }

    public bool HasAssetsLoaded(LocationType locationType) {
        // Спеціальні випадки, які не потребують завантаження карт
        if (locationType == LocationType.MainMenu ||
            locationType == LocationType.Loading ||
            locationType == LocationType.GameLoading) {
            return true;
        }

        return _cardManager.HasLocationCardData(locationType);
    }
}

public interface IAssetLoader {
    UniTask LoadLocationAssets(LocationData locationData);
    bool HasAssetsLoaded(LocationType locationType);
}
