using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class VolumeSlider : MonoBehaviour {
    [Header("Audio Settings")]
    public AudioType audioType;

    [Inject] private AudioManager audioManager;

    private void Awake() {
        Slider slider = GetComponentInChildren<Slider>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        slider.value = audioManager.GetVolume(audioType);
    }

    private void OnSliderValueChanged(float value) {
        audioManager.SetVolume(audioType, value);
    }
}

