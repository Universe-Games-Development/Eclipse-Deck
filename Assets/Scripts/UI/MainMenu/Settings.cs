using UnityEngine;
using UnityEngine.Events;

public static class Settings {
    public static UnityAction<bool> OnMusicStateChanges;

    private static bool _musicEnabled;
    public static bool MusicEnabled {
        get {
            return _musicEnabled;
        }
        set {
            // Notify all subscribers about changes
            OnMusicStateChanges?.Invoke(value);
            _musicEnabled = value;
            PlayerPrefs.SetInt("MusicEnabled", value ? 1 : 0);
        }
    }

    public static UnityAction<bool> OnSoundsStateChanges;
    private static bool _soundsEnabled;
    public static bool SoundsEnabled {
        get {
            return _soundsEnabled;
        }
        set {
            // Notify all subscribers about changes
            OnSoundsStateChanges?.Invoke(value);
            _soundsEnabled = value;
            PlayerPrefs.SetInt("SoundsEnabled", value ? 1 : 0);
        }
    }

    public static UnityAction<float> OnMusicVolumeChanges;
    private static float _musicVolume;
    public static float MusicVolume {
        get {
            return _musicVolume;
        }
        set {
            // Notify all subscribers about changes
            OnMusicVolumeChanges?.Invoke(value);
            _musicVolume = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
        }
    }

    public static UnityAction<float> OnSoundsVolumeChanges;
    private static float _soundsVolume;
    public static float SoundsVolume {
        get {
            return _soundsVolume;
        }
        set {
            // Notify all subscribers about changes
            OnSoundsVolumeChanges?.Invoke(value);
            _soundsVolume = value;
            PlayerPrefs.SetFloat("SoundsVolume", value);
        }
    }

    static Settings() {
        _musicEnabled = PlayerPrefs.GetInt("MusicEnabled") == 1 ? true : false;
        _soundsEnabled = PlayerPrefs.GetInt("SoundsEnabled") == 1 ? true : false;
        _musicVolume = PlayerPrefs.GetFloat("MusicVolume");
        _soundsVolume = PlayerPrefs.GetFloat("SoundsVolume");
    }
}
