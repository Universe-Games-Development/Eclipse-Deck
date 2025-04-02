using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class DungeonRunner : MonoBehaviour {
    [Inject] TravelManager travelManager;
    [Inject] ResourceLoadingManager resourceLoadingManager;
    [Inject] LocationTransitionManager locationTransitionManager;

    public void Start() {
        BeginRun().Forget();
    }

    private async UniTask BeginRun() {
        LocationData currentLocationData = locationTransitionManager.CurrentLocationData;
        if (!resourceLoadingManager.HasLoaders() || !resourceLoadingManager.IsLocationLoaded(currentLocationData)) {
            await resourceLoadingManager.LoadResourcesForLocation(currentLocationData);
        }
        travelManager.BeginRun();
    }
}

