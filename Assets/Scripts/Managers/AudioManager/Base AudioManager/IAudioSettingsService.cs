using UnityEngine;

namespace BasicAudioManager {
    public interface IAudioSettingsService {
        float MasterVolume { get; set; }
        float MusicVolume { get; set; }
        float SfxVolume { get; set; }
        float AmbientVolume { get; set; }

        bool IsMasterEnabled { get; set; }
        bool IsMusicEnabled { get; set; }
        bool IsSfxEnabled { get; set; }

        void Load();
        void Save();
    }

    public class AudioSettingsService : IAudioSettingsService {
        private const string MASTER_VOLUME_KEY = "MasterVolume";
        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string SFX_VOLUME_KEY = "SFXVolume";
        private const string AMBIENT_VOLUME_KEY = "AmbientVolume";
        private const string MASTER_ENABLED_KEY = "MasterVolumeEnabled";
        private const string MUSIC_ENABLED_KEY = "MusicEnabled";
        private const string SFX_ENABLED_KEY = "SFXEnabled";

        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 1f;
        public float SfxVolume { get; set; } = 1f;
        public float AmbientVolume { get; set; } = 1f;

        public bool IsMasterEnabled { get; set; } = true;
        public bool IsMusicEnabled { get; set; } = true;
        public bool IsSfxEnabled { get; set; } = true;

        public void Load() {
            MasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
            SfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
            AmbientVolume = PlayerPrefs.GetFloat(AMBIENT_VOLUME_KEY, 1f);

            IsMasterEnabled = PlayerPrefs.GetInt(MASTER_ENABLED_KEY, 1) == 1;
            IsMusicEnabled = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1) == 1;
            IsSfxEnabled = PlayerPrefs.GetInt(SFX_ENABLED_KEY, 1) == 1;
        }

        public void Save() {
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, MasterVolume);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, MusicVolume);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, SfxVolume);
            PlayerPrefs.SetFloat(AMBIENT_VOLUME_KEY, AmbientVolume);

            PlayerPrefs.SetInt(MASTER_ENABLED_KEY, IsMasterEnabled ? 1 : 0);
            PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, IsMusicEnabled ? 1 : 0);
            PlayerPrefs.SetInt(SFX_ENABLED_KEY, IsSfxEnabled ? 1 : 0);

            PlayerPrefs.Save();
        }
    }
}
