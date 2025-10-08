using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BasicAudioManager {
    public class MusicManager : MonoBehaviour {
        [SerializeField] private PlaylistPlayer _playlistPlayer;
        [SerializeField] private List<MusicPlaylistData> _playlists = new();

        public string CurrentPlaylistName => _playlistPlayer?.CurrentPlaylistName ?? string.Empty;

        private void Awake() {
            InitializePlaylistPlayer();
        }

        private void Start() {
            if (_playlists.Count > 0) {
                SetPlaylist(_playlists[0].categoryName);
            }
        }

        private void InitializePlaylistPlayer() {
            if (_playlistPlayer == null) {
                _playlistPlayer = GetComponent<PlaylistPlayer>();
                if (_playlistPlayer == null) {
                    Debug.LogError("MusicManager: PlaylistPlayer component not found!");
                }
            }
        }

        private void ApplyEnabledState() {
            if (_playlistPlayer == null) return;
        }

        public void SetPlaylist(string categoryName, bool autoPlay = true) {
            var playlist = FindPlaylist(categoryName);
            if (playlist == null || _playlistPlayer == null) return;

            _playlistPlayer.SetPlaylist(playlist, autoPlay);
        }

        public void AddPlaylist(string categoryName, List<AudioClip> tracks) {
            if (string.IsNullOrEmpty(categoryName) || tracks == null || tracks.Count == 0) {
                Debug.LogError("MusicManager: Invalid playlist data");
                return;
            }

            var existing = _playlists.Find(p => p.categoryName == categoryName);
            if (existing != null) {
                existing.musicTracks = new List<AudioClip>(tracks);
            } else {
                _playlists.Add(new MusicPlaylistData {
                    categoryName = categoryName,
                    musicTracks = new List<AudioClip>(tracks)
                });
            }
        }

        public bool RemovePlaylist(string categoryName) {
            var playlist = FindPlaylist(categoryName);
            if (playlist == null) return false;

            if (_playlistPlayer != null && _playlistPlayer.CurrentPlaylistName == categoryName) {
                _playlistPlayer.Stop();
            }

            return _playlists.Remove(playlist);
        }

        public List<string> GetAvailablePlaylists() =>
            _playlists?
                .Where(p => p != null && !string.IsNullOrEmpty(p.categoryName))
                .Select(p => p.categoryName)
                .ToList() ?? new List<string>();

        private MusicPlaylistData FindPlaylist(string categoryName) {
            var playlist = _playlists.Find(p => p?.categoryName == categoryName);

            if (playlist == null || playlist.musicTracks == null || playlist.musicTracks.Count == 0) {
                Debug.LogWarning($"MusicManager: Playlist '{categoryName}' not found or empty");
                return null;
            }

            return playlist;
        }

        public void Stop() => _playlistPlayer?.Stop();
        public void Pause() => _playlistPlayer?.Pause();
        public void Resume() => _playlistPlayer?.Resume();
    }
}
