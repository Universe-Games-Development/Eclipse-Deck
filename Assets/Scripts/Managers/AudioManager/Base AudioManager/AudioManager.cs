using System;
using UnityEngine;
using UnityEngine.Audio;

namespace BasicAudioManager {
    [RequireComponent(typeof(MusicManager), typeof(SfxManager))]
    public class AudioManager : SingletonManager<AudioManager> {
        [Header("Core Components")]
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private MixerParameters _mixerParameters = new();

        [Header("Sub-Managers")]
        [SerializeField] private MusicManager _musicManager;
        [SerializeField] private SfxManager _sfxManager;

        private AudioMixerController _mixerController;
        private IAudioSettingsService _settingsService;
        private bool _isInitialized;

        public bool IsMasterEnabled => _settingsService?.IsMasterEnabled ?? true;
        public bool IsMusicEnabled => _settingsService?.IsMusicEnabled ?? true;
        public bool IsSfxEnabled => _settingsService?.IsSfxEnabled ?? true;

        protected override void Awake() {
            base.Awake();
            Initialize();
        }

        private void Initialize() {
            if (_isInitialized) return;

            gameObject.name = "Audio Manager";

            ValidateComponents();
            InitializeSubManagers();
            InitializeServices();
            LoadAndApplySettings();

            _isInitialized = true;
        }

        private void ValidateComponents() {
            if (_mixer == null) {
                Debug.LogError("AudioManager: AudioMixer is not assigned!");
                return;
            }

            _musicManager ??= GetComponent<MusicManager>();
            _sfxManager ??= GetComponent<SfxManager>();
        }

        private void InitializeSubManagers() {
            if (_musicManager == null || _sfxManager == null) {
                Debug.LogError("AudioManager: Sub-managers not found!");
            }
        }

        private void InitializeServices() {
            _mixerController = new AudioMixerController(_mixer, _mixerParameters);
            _settingsService = new AudioSettingsService();
        }

        private void LoadAndApplySettings() {
            _settingsService.Load();
            ApplyAllSettings();
        }

        private void ApplyAllSettings() {
            // Apply volumes
            _mixerController.SetMasterVolume(_settingsService.MasterVolume);
            _mixerController.SetMusicVolume(_settingsService.MusicVolume);
            _mixerController.SetSfxVolume(_settingsService.SfxVolume);
            _mixerController.SetAmbientVolume(_settingsService.AmbientVolume);

            // Apply enabled states
            ToggleMaster(_settingsService.IsMasterEnabled);
            ToggleMusic(_settingsService.IsMusicEnabled);
            ToggleSfx(_settingsService.IsSfxEnabled);
        }

        #region Volume Controls
        public void SetMasterVolume(float volume) {
            _settingsService.MasterVolume = volume;
            _mixerController?.SetMasterVolume(volume);
        }

        public void SetMusicVolume(float volume) {
            _settingsService.MusicVolume = volume;
            _mixerController?.SetMusicVolume(volume);
        }

        public void SetSfxVolume(float volume) {
            _settingsService.SfxVolume = volume;
            _mixerController?.SetSfxVolume(volume);
        }

        public void SetAmbientVolume(float volume) {
            _settingsService.AmbientVolume = volume;
            _mixerController?.SetAmbientVolume(volume);
        }

        public float GetMasterVolume() => _mixerController?.GetMasterVolume() ?? 0f;
        public float GetMusicVolume() => _mixerController?.GetMusicVolume() ?? 0f;
        public float GetSfxVolume() => _mixerController?.GetSfxVolume() ?? 0f;
        public float GetAmbientVolume() => _mixerController?.GetAmbientVolume() ?? 0f;
        #endregion

        #region Toggle Controls
        public void ToggleMaster(bool isEnabled) {
            _settingsService.IsMasterEnabled = isEnabled;
            _mixerController?.SetMasterVolume(isEnabled ? 1f : 0f);
        }

        public void ToggleMusic(bool isEnabled) {
            _settingsService.IsMusicEnabled = isEnabled;
            _mixerController?.SetMasterMusicVolume(isEnabled ? 1f : 0f);
        }

        public void ToggleSfx(bool isEnabled) {
            _settingsService.IsSfxEnabled = isEnabled;
            _mixerController?.SetMasterSfxVolume(isEnabled ? 1f : 0f);

            if (_sfxManager != null) {
                _sfxManager.IsEnabled = isEnabled;
            }
        }
        #endregion

        #region Music API
        public void SetMusicPlaylist(string categoryName, bool autoPlay = true) {
            _musicManager?.SetPlaylist(categoryName, autoPlay);
        }

        public void AddMusicPlaylist(string categoryName, System.Collections.Generic.List<AudioClip> tracks) {
            _musicManager?.AddPlaylist(categoryName, tracks);
        }

        public bool RemoveMusicPlaylist(string categoryName) {
            return _musicManager?.RemovePlaylist(categoryName) ?? false;
        }

        public System.Collections.Generic.List<string> GetAvailablePlaylists() {
            return _musicManager?.GetAvailablePlaylists() ?? new System.Collections.Generic.List<string>();
        }
        #endregion

        #region SFX API
        public void PlaySound(AudioClip clip, float volume = 1.0f) {
            _sfxManager?.PlaySound(clip, volume);
        }

        public TemporaryAudioSource PlaySoundAtPosition(AudioClip clip, Vector3 position,
            float volume = 1.0f, float spatialBlend = 1.0f) {
            return _sfxManager?.PlaySoundAtPosition(clip, position, volume, spatialBlend);
        }
        #endregion

        #region Settings Persistence
        public void SaveSettings() => _settingsService?.Save();
        public void LoadSettings() {
            _settingsService?.Load();
            ApplyAllSettings();
        }
        #endregion

        protected override void OnDestroy() {
            base.OnDestroy();
            SaveSettings();
        }
    }
}

namespace BasicAudioManager {
    [Serializable]
    public class MixerParameters {
        public string masterVolume = "masterVol";
        public string masterMusicVolume = "masterMusicVol";
        public string masterSFXVolume = "masterSFXVol";
        public string musicVolume = "musicVol";
        public string sfxVolume = "sfxVol";
        public string ambientVolume = "ambientVol";
    }

    public class AudioMixerController {
        private readonly AudioMixer _mixer;
        private readonly MixerParameters _parameters;

        public AudioMixerController(AudioMixer mixer, MixerParameters parameters) {
            _mixer = mixer ?? throw new ArgumentNullException(nameof(mixer));
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public void SetMasterVolume(float volume) => SetVolume(_parameters.masterVolume, volume);
        public void SetMusicVolume(float volume) => SetVolume(_parameters.musicVolume, volume);
        public void SetSfxVolume(float volume) => SetVolume(_parameters.sfxVolume, volume);
        public void SetAmbientVolume(float volume) => SetVolume(_parameters.ambientVolume, volume);

        public void SetMasterMusicVolume(float volume) => SetVolume(_parameters.masterMusicVolume, volume);
        public void SetMasterSfxVolume(float volume) => SetVolume(_parameters.masterSFXVolume, volume);

        public float GetMasterVolume() => GetVolume(_parameters.masterVolume);
        public float GetMusicVolume() => GetVolume(_parameters.musicVolume);
        public float GetSfxVolume() => GetVolume(_parameters.sfxVolume);
        public float GetAmbientVolume() => GetVolume(_parameters.ambientVolume);

        private void SetVolume(string parameter, float normalizedVolume) {
            _mixer.SetFloat(parameter, ToLogVolume(normalizedVolume));
        }

        private float GetVolume(string parameter) {
            _mixer.GetFloat(parameter, out float value);
            return FromLogVolume(value);
        }

        private float ToLogVolume(float volume) => volume > 0.001f ? Mathf.Log10(volume) * 20f : -80f;
        private float FromLogVolume(float logVolume) => logVolume > -79f ? Mathf.Pow(10, logVolume / 20f) : 0f;
    }
}
