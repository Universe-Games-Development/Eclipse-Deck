using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour {
    [Header("UI_References:")]
    [SerializeField] private GameObject uiCanvas;
    [SerializeField] private Button closeButton;
    [Space]
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle soundsToggle;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundsSlider;

    private void Awake() {
        if (closeButton != null) {
            closeButton.onClick.AddListener(() => OnButtonClick(closeButton, Close));
        }

        // Settings always closed at start
        Close();

        // Update UI by saved settings
        if (musicToggle != null) musicToggle.isOn = Settings.MusicEnabled;
        if (soundsToggle != null) soundsToggle.isOn = Settings.SoundsEnabled;
        if (musicSlider != null) musicSlider.value = Settings.MusicVolume;
        if (soundsSlider != null) soundsSlider.value = Settings.SoundsVolume;

        // Listeners set
        musicToggle?.onValueChanged.AddListener(OnMusicToggleChange);
        soundsToggle?.onValueChanged.AddListener(OnSoundsToggleChange);
        musicSlider?.onValueChanged.AddListener(OnMusicVolumeChange);
        soundsSlider?.onValueChanged.AddListener(OnSoundsVolumeChange);
    }

    private void OnMusicToggleChange(bool value) {
        Settings.MusicEnabled = value;
    }

    private void OnSoundsToggleChange(bool value) {
        Settings.SoundsEnabled = value;
    }

    private void OnMusicVolumeChange(float value) {
        Settings.MusicVolume = value;
    }

    private void OnSoundsVolumeChange(float value) {
        Settings.SoundsVolume = value;
    }

    public void Close() {
        if (uiCanvas != null) uiCanvas.SetActive(false);
    }

    public void Show() {
        if (uiCanvas != null) uiCanvas.SetActive(true);
    }

    public void Toggle(bool state) {
        if (uiCanvas != null) uiCanvas.SetActive(state);
    }

    private void OnButtonClick(Button button, UnityEngine.Events.UnityAction action) {
        action?.Invoke();
    }

    private void OnDestroy() {
        closeButton?.onClick.RemoveListener(() => OnButtonClick(closeButton, Close));

        // Remove UI elements listeners
        musicToggle?.onValueChanged.RemoveListener(OnMusicToggleChange);
        soundsToggle?.onValueChanged.RemoveListener(OnSoundsToggleChange);
        musicSlider?.onValueChanged.RemoveListener(OnMusicVolumeChange);
        soundsSlider?.onValueChanged.RemoveListener(OnSoundsVolumeChange);
    }
}
