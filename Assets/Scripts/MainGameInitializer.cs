using UnityEngine;
using Zenject;

public class MainGameInitializer : MonoBehaviour {
    [Inject] TravelManager travelManager;
    [Inject] PlayerManager playerManager;
    [Inject] PlayerPresenter playerPresenter;
    private void Awake() {
        if (!playerManager.GetPlayer(out Player player)) {
            Debug.LogWarning("Failed to get player");
            return;
        }
        playerPresenter.InitializePlayer(player);
        travelManager.BeginRun();
    }
}
