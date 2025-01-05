using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MainMenuUI : MonoBehaviour {
    [Header("UI_References :")]
    [SerializeField] private Button uiStartButton;
    [SerializeField] private Button uiSettingsButton;
    [SerializeField] private Button uiExitButton;
    [SerializeField] private Button gitButton;
    [SerializeField] private string teamLink;
    [Space]
    [Header("Settings UI :")]
    [SerializeField] private SettingsUI uISettings;

    private LevelManager levelManager;

    
    private StartGameHandler startGameHandler;
    [Inject]
    public void Contruct(LevelManager levelManager) {
        this.levelManager = levelManager;
    }

    private void Awake() {
        startGameHandler = GetComponent<StartGameHandler>();
    }

    private void Start() {

        // set buttons listeners
        uiStartButton?.onClick.AddListener(() => OnButtonClick(uiStartButton, startGameHandler.StartGame));

        uiExitButton?.onClick.AddListener(() => OnButtonClick(uiExitButton, levelManager.ExitGame));

        if (uISettings != null) {
            uiSettingsButton.onClick.AddListener(() => OnButtonClick(uiSettingsButton, uISettings.Show));
        } else {
            Debug.Log("Ui settings missing");
        }

        gitButton?.onClick.AddListener(() => OnButtonClick(uiExitButton, OpenGitURL));
    }

    private void OnButtonClick(Button button, UnityEngine.Events.UnityAction action) {
        action?.Invoke();
    }

    private void OpenGitURL() {
        Application.OpenURL(teamLink);
    }

    private void OnDestroy() {
        uiStartButton?.onClick.RemoveAllListeners();
        uiSettingsButton?.onClick.RemoveAllListeners();
        uiExitButton?.onClick.RemoveAllListeners();
        gitButton?.onClick.RemoveAllListeners();
    }
}
