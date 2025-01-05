using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    private MixManager mixManager;
    private MusicManager musicManager;
    private FXManager fXManager;

    private void Start() {
        mixManager = GetComponentInChildren<MixManager>();
        musicManager = GetComponentInChildren<MusicManager>();
        fXManager = GetComponentInChildren<FXManager>();
    }
}

[Serializable]
public struct SoundList {
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
    public AudioClip[] Sounds { get => sounds; }
}