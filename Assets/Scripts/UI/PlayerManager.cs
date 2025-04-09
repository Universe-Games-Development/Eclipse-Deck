using UnityEngine;
using Zenject;

public class PlayerManager : MonoBehaviour {
    [SerializeField] private OpponentData playerData;
    private Player currentPlayer;
    private DiContainer _container;

    [Inject]
    public void Construct(DiContainer container) {
        _container = container;
    }

    private Player CreatePlayer() {
        Player player = _container.Instantiate<Player>();
        player.SetData(playerData);
        return player;
    }

    public bool GetPlayer(out Player player) {
        if (currentPlayer == null) {

            currentPlayer = CreatePlayer();
        }
        player = currentPlayer;
        return true;
    }

    public void SavePlayerData() {
        if (currentPlayer == null) {
            Debug.LogWarning("Failed to save player data player is null");
        }
        // PlayerPrefs.SetInt("PlayerHealth", player.Health); example of saving player health
    }
}
