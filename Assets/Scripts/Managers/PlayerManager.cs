using UnityEngine;
using Zenject;

public class PlayerManager : MonoBehaviour {
    private Opponent _player;

    [SerializeField] private PlayerData playerData;

    [Inject] private DiContainer _container;

    /// <summary>
    /// Gets the current player or creates one if none exists
    /// </summary>
    /// <param name="player">The current player instance</param>
    /// <returns>True if player is available</returns>
    public bool GetPlayer(out Opponent player) {
        if (_player == null) {
            //_player = opponentFactory.CreateOpponent(playerData);
            if (_player == null) {
                player = null;
                return false;
            }
        }

        player = _player;
        return true;
    }


    /// <summary>
    /// Saves the current player data to persistent storage
    /// </summary>
    public void SavePlayerData() {
        if (_player == null) {
            Debug.LogWarning("Failed to save player data - player is null");
            return;
        }

        // Save player data to PlayerPrefs
        // Add other properties as needed

        //PlayerPrefs.SetInt("PlayerHealth", _player.Health);
        //PlayerPrefs.SetInt("PlayerScore", _player.Score);
        //PlayerPrefs.SetString("PlayerName", _player.Name);


        PlayerPrefs.Save();
        Debug.Log("Player data saved successfully");
    }

    /// <summary>
    /// Loads player data from persistent storage
    /// </summary>
    public void LoadPlayerData() {
        if (_player == null) {
            if (!GetPlayer(out _)) {
                Debug.LogError("Failed to load player data - couldn't create player");
                return;
            }
        }

        if (PlayerPrefs.HasKey("PlayerHealth")) {
            //_player.Health = PlayerPrefs.GetInt("PlayerHealth");
            //_player.Score = PlayerPrefs.GetInt("PlayerScore", 0);
            //_player.Name = PlayerPrefs.GetString("PlayerName", "Player");
            // Load other properties as needed

            Debug.Log("Player data loaded successfully");
        } else {
            Debug.Log("No saved player data found, using defaults");
        }
    }
}