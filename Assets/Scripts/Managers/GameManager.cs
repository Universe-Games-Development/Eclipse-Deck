using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject] LocationTransitionManager locationManager;
    [SerializeField]

    public void StartNewGame() {
        locationManager.LocationTransition(0);
    }

    public void SetPause(bool pause) {
        if (pause) {
            Time.timeScale = 0f; // Stop game time
        } else {
            Time.timeScale = 1f; // Reset game time
        }
    }

    public void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
