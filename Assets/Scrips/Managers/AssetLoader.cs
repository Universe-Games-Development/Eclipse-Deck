using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public class AssetLoader : MonoBehaviour {
    public Action OnAssetsLoaded;
    [Inject] public CardManager CardManager;

    // Location scenes addressable labels
    public AssetLabelReference mainMenuLabel;
    public AssetLabelReference sewersLabel;
    public AssetLabelReference caveLabel;
    public AssetLabelReference floodedCaveLabel;
    public AssetLabelReference labLabel;
    public AssetLabelReference hellLabel;

    private Dictionary<Location, AssetLabelReference> locationToScene;
    private void Awake() {
        locationToScene = new Dictionary<Location, AssetLabelReference> {
            { Location.MainMenu, mainMenuLabel},
        { Location.Sewers, sewersLabel},
        { Location.Cave, caveLabel },
        { Location.FloodedCave, floodedCaveLabel},
        { Location.Lab, labLabel },
        { Location.Hell, hellLabel},
        };
    }

    public async UniTask LoadLocationAssets(Location enumLabel) {
        if (!locationToScene.TryGetValue(enumLabel, out AssetLabelReference assetLabel)) {
            Debug.LogWarning($"Wrong label for scene asset loading: {enumLabel}");
            return;
        }

        try {
            await CardManager.LoadCardsForLocation(assetLabel, enumLabel);
            if (CardManager.HasLocationCardData(enumLabel)) {
                Debug.Log("Cards loaded for " + enumLabel);
            }
            OnAssetsLoaded?.Invoke();
        } catch (Exception e) {
            Debug.LogError($"Failed to load assets for {enumLabel}: {e.Message}");
        }
    }


    public bool HasActualData(Location enumLabel) {
        if (enumLabel == Location.MainMenu || enumLabel == Location.Loading || enumLabel == Location.GameLoading) return true;
        return CardManager.HasLocationCardData(enumLabel);
    }
}