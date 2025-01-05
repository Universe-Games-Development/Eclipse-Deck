using System;
using System.Collections;
using UnityEngine;

public enum MusicType {
    IN_GAME_MUSIC, UI_MUSIC,
    WIN_MUSIC, DEFEAT_MUSIC
}

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour {
    public MusicType currentMusicType;
    [SerializeField] private bool PlayOnAwake;
    [SerializeField] private SoundList[] musicList;

    private AudioSource audioSource;
    private Coroutine musicCoroutine;

    private void OnEnable() {
        string[] enumNames = Enum.GetNames(typeof(MusicType));
        Array.Resize(ref musicList, enumNames.Length);
        for (int i = 0; i < musicList.Length; i++) {
            musicList[i].name = enumNames[i];
        }
    }

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
        if (PlayOnAwake) {
            PlayMusic();
        }
    }


    public void PlayMusic() {
        if (musicCoroutine != null) {
            StopCoroutine(musicCoroutine);
        }
        musicCoroutine = StartCoroutine(PlayMusicCoroutine());
    }

    private IEnumerator PlayMusicCoroutine() {
        while (true) {
            // Перевіряємо, чи є вікно гри активним
            while (!Application.isFocused) {
                yield return null;
            }

            SoundList selectedSoundList = Array.Find(musicList, list => list.name == currentMusicType.ToString());
            if (selectedSoundList.Sounds.Length == 0) {
                Debug.LogWarning("No music found for type: " + currentMusicType);
                yield break;
            }

            AudioClip selectedClip = selectedSoundList.Sounds[UnityEngine.Random.Range(0, selectedSoundList.Sounds.Length)];
            audioSource.clip = selectedClip;
            audioSource.Play();

            // Чекаємо поки трек завершиться
            while (audioSource.isPlaying) {
                yield return null;
            }
        }
    }

    public void ChangeMusicType(MusicType newMusicType) {
        currentMusicType = newMusicType;
        PlayMusic(); // Перезапускаємо відтворення музики з новим типом
    }
}