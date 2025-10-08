using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMusicPlaylist", menuName = "Audio/Music Playlist")]
public class MusicPlaylistData : ScriptableObject {
    public string categoryName;
    public List<AudioClip> musicTracks = new List<AudioClip>();

    public int currentTrackIndex = 0;
}