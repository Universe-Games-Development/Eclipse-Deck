using UnityEngine;
using UnityEngine.Audio;

public class MixManager : MonoBehaviour {
    [SerializeField] private AudioMixer mixer;

    [Header("Masters")]
    [SerializeField] private string masterVolumeName = "masterVol";
    [SerializeField] private string masterMusicVolumeName = "masterMusicVol";
    [SerializeField] private string masterSFXVolumeName = "masterSFXVol";
    [Header("Childs")]
    [SerializeField] private string musicVolumeName = "musicVol";
    [SerializeField] private string sfxVolumeName = "sfxVol";
    [SerializeField] private string ambientVolumeName = "ambientVol";

    private void Start() {
        LoadSettingsData();
    }

    public void SetMasterVolume(float volume) {
        float logVolume = Mathf.Log10(volume) * 20f;
        mixer.SetFloat(masterVolumeName, logVolume);
    }

    public void SetMusicVolume(float volume) {
        float logVolume;
        if (volume > 0f) {
            logVolume = Mathf.Log10(volume) * 20f;
        } else {
            // якщо гучн≥сть дор≥внюЇ 0, вимкн≥ть звук повн≥стю
            logVolume = -80f;
        }
        mixer.SetFloat(musicVolumeName, logVolume);
    }


    public void SetSFXVolume(float volume) {
        float logVolume;
        if (volume > 0f) {
            logVolume = Mathf.Log10(volume) * 20f;
        } else {
            // якщо гучн≥сть дор≥внюЇ 0, вимкн≥ть звук повн≥стю
            logVolume = -80f;
        }
        mixer.SetFloat(sfxVolumeName, logVolume);
    }

    public void SetAmbientVolume(float volume) {
        float logVolume = Mathf.Log10(volume) * 20f;
        mixer.SetFloat(ambientVolumeName, logVolume);
    }

    public void ToggleMusic(bool isEnabled) {
        mixer.SetFloat(masterMusicVolumeName, isEnabled ? 0f : -80f);
        float value;
        mixer.GetFloat(masterMusicVolumeName, out value);
    }

    private void ToggleSFX(bool isEnabled) {
        mixer.SetFloat(masterSFXVolumeName, isEnabled ? 0f : -80f);
    }

    #region SETUP logic
    public void LoadSettingsData() {
        SetSFXVolume(Settings.SoundsVolume);
        SetMusicVolume(Settings.MusicVolume);

        ToggleMusic(Settings.MusicEnabled);
        ToggleSFX(Settings.SoundsEnabled);
    }

    private void SubscribeSettings() {
        Settings.OnMusicStateChanges += ToggleMusic;
        Settings.OnSoundsStateChanges += ToggleSFX;
        Settings.OnMusicVolumeChanges += SetMusicVolume;
        Settings.OnSoundsVolumeChanges += SetSFXVolume;
    }

    private void UnsubsribeSettings() {
        Settings.OnMusicStateChanges -= ToggleMusic;
        Settings.OnSoundsStateChanges -= ToggleSFX;
        Settings.OnMusicVolumeChanges -= SetMusicVolume;
        Settings.OnSoundsVolumeChanges -= SetSFXVolume;
    }

    private void OnDisable() {
        UnsubsribeSettings();
    }
    #endregion
}
