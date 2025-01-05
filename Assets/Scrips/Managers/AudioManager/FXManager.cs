using System;
using UnityEngine;

public enum SFXType {
    CRYSTAL_CLICK,
    BUTTON_CLICK
}

[RequireComponent(typeof(AudioSource))]
public class FXManager : MonoBehaviour {

    [SerializeField] private SoundList[] fxList;
    private AudioSource audioSource;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable() {
        string[] enumNames = Enum.GetNames(typeof(SFXType));
        Array.Resize(ref fxList, enumNames.Length);
        for (int i = 0; i < fxList.Length; i++) {
            fxList[i].name = enumNames[i];
        }
    }

    public void PlaySoundEffect(SFXType fxType, float volume = 1f) {
        AudioClip[] soundList = fxList[(int)fxType].Sounds;
        if (soundList.Length == 0) {
            Debug.Log("Sound List is empty");
            return;
        }
        AudioClip randomClip = soundList[UnityEngine.Random.Range(0, soundList.Length)];
        audioSource.PlayOneShot(randomClip, volume);

    }
}