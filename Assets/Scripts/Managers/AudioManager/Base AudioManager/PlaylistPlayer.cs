using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BasicAudioManager {
    public enum PlaybackState {
        Stopped,
        Playing,
        Paused
    }
    public class PlaylistPlayer : MonoBehaviour {
        [SerializeField] private AudioSource _musicSource;
        [Header("Settings")]
        [SerializeField] private bool _autoPlayOnStart = true;
        [SerializeField] private bool _loopPlaylist = true;

        private MusicPlaylist _currentPlaylist;
        private CancellationTokenSource _playbackCTS;
        private AudioClip _currentClip;

        // State
        public PlaybackState State { get; private set; } = PlaybackState.Stopped;

        // Properties
        public bool IsLooping {
            get => _loopPlaylist;
            set => _loopPlaylist = value;
        }

        public float Volume {
            get => _musicSource?.volume ?? 1f;
            set { if (_musicSource != null) _musicSource.volume = Mathf.Clamp01(value); }
        }

        public float CurrentTime => _musicSource?.time ?? 0f;
        public AudioClip CurrentClip => _currentClip;
        public MusicPlaylistData CurrentPlaylistData => _currentPlaylist?.Data;
        public string CurrentPlaylistName => _currentPlaylist?.Data?.categoryName ?? string.Empty;
        public string CurrentTrackName => _currentPlaylist?.GetCurrentTrackName() ?? "No track";
        public int CurrentTrackIndex => _currentPlaylist?.GetCurrentTrackIndex() ?? 0;
        public int TotalTracks => _currentPlaylist?.TrackCount ?? 0;

        // Events
        public event Action<AudioClip> OnTrackStarted;
        public event Action<AudioClip> OnTrackCompleted;
        public event Action<AudioClip> OnTrackStopped;
        public event Action<MusicPlaylistData> OnPlaylistChanged;
        public event Action<PlaybackState> OnStateChanged;

        private void Awake() {
            InitializeAudioSource();
        }

        private void InitializeAudioSource() {
            if (_musicSource == null) {
                _musicSource = gameObject.AddComponent<AudioSource>();
            }
            _musicSource.loop = false;
            _musicSource.playOnAwake = false;
        }

        private void Start() {
            if (_autoPlayOnStart && _currentPlaylist != null) {
                Play().Forget();
            }
        }

        #region Public API

        public void SetPlaylist(MusicPlaylistData playlistData, bool autoPlay = true) {
            if (_currentPlaylist != null && _currentPlaylist.Data == playlistData) {
                Debug.LogWarning("PlaylistPlayer: Trying to assign the same playlist");
                return;
            }

            Stop();
            _currentPlaylist = new MusicPlaylist(playlistData);
            OnPlaylistChanged?.Invoke(playlistData);

            if (autoPlay) {
                Play().Forget();
            }
        }

        public async UniTask Play() {
            if (_currentPlaylist == null || !_currentPlaylist.HasTracks) {
                Debug.LogWarning("PlaylistPlayer: No playlist or tracks available");
                return;
            }

            // Якщо на паузі - просто відновлюємо
            if (State == PlaybackState.Paused) {
                Resume();
                return;
            }

            // Інакше починаємо з початку плейлиста
            Stop();
            await PlayCurrentTrack();
        }

        public void Stop() {
            if (State == PlaybackState.Stopped) return;

            var stoppedClip = _currentClip;

            CancelPlayback();
            StopAudioSource();
            ChangeState(PlaybackState.Stopped);

            if (stoppedClip != null) {
                OnTrackStopped?.Invoke(stoppedClip);
            }
        }

        public void Pause() {
            if (State != PlaybackState.Playing) return;

            _musicSource.Pause();
            ChangeState(PlaybackState.Paused);
        }

        public void Resume() {
            if (State != PlaybackState.Paused) return;

            _musicSource.UnPause();
            ChangeState(PlaybackState.Playing);
        }

        public async UniTask PlayNext() {
            if (_currentPlaylist == null) return;

            var nextTrack = _currentPlaylist.GetNext(IsLooping);
            if (nextTrack != null) {
                await PlayTrack(nextTrack);
            } else {
                Stop();
            }
        }

        public async UniTask PlayPrevious() {
            if (_currentPlaylist == null) return;

            var prevTrack = _currentPlaylist.GetPrevious(IsLooping);
            if (prevTrack != null) {
                await PlayTrack(prevTrack);
            }
        }

        public async UniTask PlayTrackAt(int index) {
            if (_currentPlaylist == null) return;

            var track = _currentPlaylist.GetAt(index);
            if (track != null) {
                await PlayTrack(track);
            }
        }

        public void RestartCurrentTrack() {
            if (_currentPlaylist == null || _currentClip == null) return;

            _musicSource.time = 0f;
            if (State == PlaybackState.Stopped) {
                PlayTrack(_currentClip).Forget();
            }
        }

        public void ToggleShuffle() {
            _currentPlaylist?.Shuffle();
        }

        public void SetTime(float time) {
            if (_musicSource != null && _currentClip != null) {
                _musicSource.time = Mathf.Clamp(time, 0f, _currentClip.length);
            }
        }

        #endregion

        #region Private Methods

        private async UniTask PlayCurrentTrack() {
            if (_currentPlaylist == null) return;

            var track = _currentPlaylist.GetCurrent();
            if (track == null) {
                track = _currentPlaylist.GetNext(IsLooping);
            }

            if (track != null) {
                await PlayTrack(track);
            }
        }

        private async UniTask PlayTrack(AudioClip clip) {
            if (clip == null) return;

            Stop();

            _currentClip = clip;
            _playbackCTS = new CancellationTokenSource();

            try {
                _musicSource.clip = clip;
                _musicSource.time = 0f;
                _musicSource.Play();

                ChangeState(PlaybackState.Playing);
                OnTrackStarted?.Invoke(clip);

                await WaitForTrackToFinish(_playbackCTS.Token);

                // Якщо трек завершився природньо (не був зупинений)
                if (State == PlaybackState.Playing) {
                    OnTrackCompleted?.Invoke(clip);
                    await PlayNext();
                }
            } catch (OperationCanceledException) {
                // Playback was cancelled - normal case
            } finally {
                if (State != PlaybackState.Stopped) {
                    _currentClip = null;
                }
            }
        }

        private async UniTask WaitForTrackToFinish(CancellationToken ct) {
            // Чекаємо поки трек грає
            await UniTask.WaitWhile(
                () => _musicSource != null &&
                      _musicSource.isPlaying &&
                      State == PlaybackState.Playing,
                cancellationToken: ct
            );
        }

        private void ChangeState(PlaybackState newState) {
            if (State == newState) return;

            State = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void CancelPlayback() {
            _playbackCTS?.Cancel();
            _playbackCTS?.Dispose();
            _playbackCTS = null;
        }

        private void StopAudioSource() {
            if (_musicSource != null && _musicSource.isPlaying) {
                _musicSource.Stop();
            }
        }

        #endregion

        private void OnDestroy() {
            Stop();
        }
    }
}


[System.Serializable]
public class MusicPlaylist {
    public MusicPlaylistData Data { get; private set; }
    private List<AudioClip> _currentOrder;
    private int _currentIndex = -1;

    public AudioClip CurrentTrack => _currentIndex >= 0 && _currentIndex < _currentOrder.Count ? _currentOrder[_currentIndex] : null;
    public int TrackCount => _currentOrder?.Count ?? 0;
    public bool HasTracks => TrackCount > 0;

    public MusicPlaylist(MusicPlaylistData data) {
        Data = data;
        ResetToOriginalOrder();
    }

    public AudioClip GetNext(bool loop = true) {
        if (!HasTracks) return null;

        if (_currentIndex < _currentOrder.Count - 1) {
            _currentIndex++;
        } else if (loop) {
            _currentIndex = 0;
        } else {
            return null;
        }

        return _currentOrder[_currentIndex];
    }

    public AudioClip GetPrevious(bool loop = true) {
        if (!HasTracks) return null;

        if (_currentIndex > 0) {
            _currentIndex--;
        } else if (loop) {
            _currentIndex = _currentOrder.Count - 1;
        } else {
            return null;
        }

        return _currentOrder[_currentIndex];
    }

    public AudioClip GetCurrent() {
        return CurrentTrack;
    }

    public AudioClip GetAt(int index) {
        if (index >= 0 && index < _currentOrder.Count) {
            _currentIndex = index;
            return _currentOrder[index];
        }
        return null;
    }

    public void Shuffle() {
        if (!HasTracks) return;
        System.Random random = new System.Random();

        var currentTrack = CurrentTrack;
        _currentOrder = _currentOrder.OrderBy(x => random.Next()).ToList();

        // Çáåð³ãàºìî ïîòî÷íèé òðåê íà òîìó æ ì³ñö³
        if (currentTrack != null) {
            var newIndex = _currentOrder.IndexOf(currentTrack);
            if (newIndex >= 0) {
                // Ì³íÿºìî ì³ñöÿìè ç ïîòî÷íèì ³íäåêñîì
                (_currentOrder[newIndex], _currentOrder[_currentIndex]) =
                    (_currentOrder[_currentIndex], _currentOrder[newIndex]);
            }
        }
    }

    public void ResetToOriginalOrder() {
        _currentOrder = Data?.musicTracks?.ToList() ?? new List<AudioClip>();
        _currentIndex = -1;
    }

    public int GetCurrentTrackIndex() => _currentIndex;
    public string GetCurrentTrackName() => CurrentTrack?.name ?? "No track";
}