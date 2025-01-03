using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager> {

    [Header("Scene data")]
    public string currentSceneName;
    [SerializeField] private string mainMenuName;
    [SerializeField] private string gameSceneName;

    private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
        currentSceneName = scene.name;
    }

    #region LEVEL LOAD
    public bool isMainMenu() {
        return SceneManager.GetActiveScene().name.Equals(mainMenuName);
    }

    public void OpenMainMenu() {
        // To DO : Add check scene valid name
        SceneManager.LoadScene(mainMenuName);
    }

    public void StartGame() {
        // Scene Change
        SceneManager.LoadScene(gameSceneName);
    }
    #endregion

    public void Restart() {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void SetPause(bool pause) {
        if (pause) {
            Time.timeScale = 0f; // Stop game time
        } else {
            Time.timeScale = 1f; // Reset game time
        }
    }

    public void ExitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
