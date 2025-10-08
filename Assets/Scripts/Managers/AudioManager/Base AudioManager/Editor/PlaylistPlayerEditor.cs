#if UNITY_EDITOR
using BasicAudioManager;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlaylistPlayer))]
public class PlaylistPlayerEditor : Editor {
    private PlaylistPlayer _player;
    private MusicPlaylistData _newPlaylistData;

    private bool _showPlaylistInfo = true;
    private bool _showPlayerControls = true;
    private bool _showPlaybackOptions = true;
    private bool _showRuntimeInfo = true;

    private Vector2 _trackListScroll;
    private GUIStyle _headerStyle;
    private GUIStyle _infoBoxStyle;
    private bool _stylesInitialized;

    private void OnEnable() {
        _player = (PlaylistPlayer)target;
        EditorApplication.update += Repaint;
    }

    private void OnDisable() {
        EditorApplication.update -= Repaint;
    }

    private void InitializeStyles() {
        if (_stylesInitialized) return;

        _headerStyle = new GUIStyle(EditorStyles.boldLabel) {
            fontSize = 12,
            margin = new RectOffset(0, 0, 10, 5)
        };

        _infoBoxStyle = new GUIStyle(EditorStyles.helpBox) {
            padding = new RectOffset(10, 10, 10, 10)
        };

        _stylesInitialized = true;
    }

    public override void OnInspectorGUI() {
        InitializeStyles();
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        DrawSeparator();

        DrawPlaylistManagement();
        DrawPlaylistInfo();
        DrawPlayerControls();
        DrawPlaybackOptions();
        DrawRuntimeInfo();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed) {
            EditorUtility.SetDirty(target);
        }
    }

    private void DrawPlaylistManagement() {
        EditorGUILayout.Space(5);
        _showPlaylistInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_showPlaylistInfo, "Playlist Management");

        if (_showPlaylistInfo) {
            EditorGUILayout.BeginVertical(_infoBoxStyle);

            EditorGUI.BeginChangeCheck();
            _newPlaylistData = (MusicPlaylistData)EditorGUILayout.ObjectField(
                "Assign New Playlist",
                _newPlaylistData,
                typeof(MusicPlaylistData),
                false
            );

            if (EditorGUI.EndChangeCheck() && _newPlaylistData != null) {
                EditorGUILayout.HelpBox(
                    $"Playlist: {_newPlaylistData.categoryName}\nTracks: {_newPlaylistData.musicTracks?.Count ?? 0}",
                    MessageType.Info
                );
            }

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = _newPlaylistData != null;
            if (GUILayout.Button("Apply & Play", GUILayout.Height(25))) {
                _player.SetPlaylist(_newPlaylistData, true);
                _newPlaylistData = null;
            }

            if (GUILayout.Button("Apply (No Play)", GUILayout.Height(25))) {
                _player.SetPlaylist(_newPlaylistData, false);
                _newPlaylistData = null;
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawPlaylistInfo() {
        if (_player.CurrentPlaylistData == null) return;

        EditorGUILayout.Space(5);
        _showPlaylistInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_showPlaylistInfo, "Current Playlist Info");

        if (_showPlaylistInfo) {
            EditorGUILayout.BeginVertical(_infoBoxStyle);

            var data = _player.CurrentPlaylistData;

            EditorGUILayout.LabelField("Playlist Name", data.categoryName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Total Tracks", _player.TotalTracks.ToString());

            if (_player.TotalTracks > 0) {
                EditorGUILayout.LabelField(
                    "Current Track",
                    $"{_player.CurrentTrackIndex + 1} / {_player.TotalTracks}"
                );

                EditorGUILayout.Space(5);
                DrawTrackList();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawTrackList() {
        if (_player.CurrentPlaylistData?.musicTracks == null) return;

        EditorGUILayout.LabelField("Track List:", EditorStyles.boldLabel);

        _trackListScroll = EditorGUILayout.BeginScrollView(
            _trackListScroll,
            GUILayout.Height(Mathf.Min(_player.TotalTracks * 22 + 10, 150))
        );

        var tracks = _player.CurrentPlaylistData.musicTracks;

        for (int i = 0; i < tracks.Count; i++) {
            if (tracks[i] == null) continue;

            EditorGUILayout.BeginHorizontal();

            bool isCurrent = i == _player.CurrentTrackIndex;
            GUI.backgroundColor = isCurrent ? new Color(0.5f, 1f, 0.5f) : Color.white;

            string icon = GetTrackIcon(isCurrent);
            string trackName = $"{icon} {tracks[i].name}";

            EditorGUILayout.LabelField($"{i + 1}.", GUILayout.Width(30));
            EditorGUILayout.LabelField(trackName);

            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("Play", GUILayout.Width(50))) {
                _player.PlayTrackAt(i).Forget();
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private string GetTrackIcon(bool isCurrent) {
        if (!isCurrent) return "";

        return _player.State switch {
            PlaybackState.Playing => "▶",
            PlaybackState.Paused => "⏸",
            _ => "■"
        };
    }

    private void DrawPlayerControls() {
        EditorGUILayout.Space(5);
        _showPlayerControls = EditorGUILayout.BeginFoldoutHeaderGroup(_showPlayerControls, "Player Controls");

        if (_showPlayerControls) {
            EditorGUILayout.BeginVertical(_infoBoxStyle);

            bool hasPlaylist = _player.CurrentPlaylistData != null && _player.TotalTracks > 0;
            GUI.enabled = hasPlaylist;

            // Основна кнопка Play/Stop
            EditorGUILayout.BeginHorizontal();

            string mainButtonText = GetMainButtonText();
            if (GUILayout.Button(mainButtonText, GUILayout.Height(30))) {
                HandleMainButtonClick();
            }

            // Кнопка Pause/Resume (тільки коли є що паузити)
            if (_player.State != PlaybackState.Stopped) {
                string pauseButtonText = _player.State == PlaybackState.Paused ? "▶ Resume" : "⏸ Pause";
                if (GUILayout.Button(pauseButtonText, GUILayout.Height(30))) {
                    if (_player.State == PlaybackState.Paused) {
                        _player.Resume();
                    } else {
                        _player.Pause();
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Навігація
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("⏮ Previous", GUILayout.Height(25))) {
                _player.PlayPrevious().Forget();
            }

            if (GUILayout.Button("↻ Restart", GUILayout.Height(25))) {
                _player.RestartCurrentTrack();
            }

            if (GUILayout.Button("Next ⏭", GUILayout.Height(25))) {
                _player.PlayNext().Forget();
            }

            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private string GetMainButtonText() {
        return _player.State switch {
            PlaybackState.Stopped => "▶ Play",
            PlaybackState.Playing => "■ Stop",
            PlaybackState.Paused => "■ Stop",
            _ => "▶ Play"
        };
    }

    private void HandleMainButtonClick() {
        if (_player.State != PlaybackState.Stopped) {
            _player.Stop();
        } else {
            _player.Play().Forget();
        }
    }

    private void DrawPlaybackOptions() {
        EditorGUILayout.Space(5);
        _showPlaybackOptions = EditorGUILayout.BeginFoldoutHeaderGroup(_showPlaybackOptions, "Playback Options");

        if (_showPlaybackOptions) {
            EditorGUILayout.BeginVertical(_infoBoxStyle);

            bool hasPlaylist = _player.CurrentPlaylistData != null;
            GUI.enabled = hasPlaylist;

            // Loop toggle
            EditorGUI.BeginChangeCheck();
            bool newLoop = EditorGUILayout.Toggle("Loop Playlist", _player.IsLooping);
            if (EditorGUI.EndChangeCheck()) {
                _player.IsLooping = newLoop;
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.Space(5);

            // Volume slider
            EditorGUI.BeginChangeCheck();
            float newVolume = EditorGUILayout.Slider("Volume", _player.Volume, 0f, 1f);
            if (EditorGUI.EndChangeCheck()) {
                _player.Volume = newVolume;
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.Space(5);

            // Shuffle button
            if (GUILayout.Button("🔀 Shuffle Playlist", GUILayout.Height(25))) {
                _player.ToggleShuffle();
                EditorUtility.SetDirty(target);
            }

            GUI.enabled = true;
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawRuntimeInfo() {
        EditorGUILayout.Space(5);
        _showRuntimeInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_showRuntimeInfo, "Runtime Info");

        if (_showRuntimeInfo) {
            EditorGUILayout.BeginVertical(_infoBoxStyle);

            // Status with color
            DrawStatusInfo();

            // Current track info
            if (_player.CurrentPlaylistData != null) {
                EditorGUILayout.LabelField("Current Track:", _player.CurrentTrackName);

                // Progress bar
                if (_player.State != PlaybackState.Stopped) {
                    DrawProgressBar();
                }
            } else {
                EditorGUILayout.HelpBox("No playlist assigned", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawStatusInfo() {
        string status = GetStatusString();
        Color statusColor = GetStatusColor();

        var oldColor = GUI.contentColor;
        GUI.contentColor = statusColor;
        EditorGUILayout.LabelField("Status:", status, EditorStyles.boldLabel);
        GUI.contentColor = oldColor;
    }

    private void DrawProgressBar() {
        float currentTime = _player.CurrentTime;
        var currentClip = _player.CurrentClip;

        if (currentClip != null) {
            float duration = currentClip.length;
            float progress = duration > 0 ? currentTime / duration : 0f;

            EditorGUILayout.Space(3);

            // Progress bar
            Rect progressRect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
            EditorGUI.ProgressBar(progressRect, progress,
                $"{FormatTime(currentTime)} / {FormatTime(duration)}");

            // Time scrubber (optional - can drag to seek)
            EditorGUI.BeginChangeCheck();
            float newTime = EditorGUILayout.Slider("Seek", currentTime, 0f, duration);
            if (EditorGUI.EndChangeCheck() && _player.State != PlaybackState.Stopped) {
                _player.SetTime(newTime);
            }
        }
    }

    private string GetStatusString() {
        return _player.State switch {
            PlaybackState.Stopped => "■ Stopped",
            PlaybackState.Paused => "⏸ Paused",
            PlaybackState.Playing => "▶ Playing",
            _ => "Unknown"
        };
    }

    private Color GetStatusColor() {
        return _player.State switch {
            PlaybackState.Stopped => Color.gray,
            PlaybackState.Paused => Color.yellow,
            PlaybackState.Playing => Color.green,
            _ => Color.white
        };
    }

    private string FormatTime(float timeInSeconds) {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    private void DrawSeparator() {
        EditorGUILayout.Space(5);
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(2));
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(5);
    }
}
#endif