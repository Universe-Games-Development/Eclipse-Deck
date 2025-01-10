using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    private EventInstance ambienceEventInstance;
    private EventInstance musicEventInstance;

    private readonly List<EventInstance> eventInstances = new();
    private readonly List<StudioEventEmitter> eventEmitters = new();

    [SerializeField] private FMODEvents fmodEvents;

    private readonly Dictionary<AudioType, Bus> audioBuses = new();
    private readonly Dictionary<AudioType, float> volumes = new();

    private void Awake() {
        InitializeBuses();
        InitializeAmbience(fmodEvents.sewersAmbient);
        InitializeMusic(fmodEvents.testMusic);
    }

    private void InitializeBuses() {
        audioBuses[AudioType.MASTER] = RuntimeManager.GetBus("bus:/");
        audioBuses[AudioType.MUSIC] = RuntimeManager.GetBus("bus:/Music");
        audioBuses[AudioType.AMBIENCE] = RuntimeManager.GetBus("bus:/Ambience");
        audioBuses[AudioType.SFX] = RuntimeManager.GetBus("bus:/SFX");

        volumes[AudioType.MASTER] = 0.01f;
        volumes[AudioType.MUSIC] = 1f;
        volumes[AudioType.AMBIENCE] = 1f;
        volumes[AudioType.SFX] = 1f;

        var volumeCopy = new Dictionary<AudioType, float>(volumes);

        foreach (var kvp in volumeCopy) {
            SetVolume(kvp.Key, kvp.Value);
        }
    }


    public float GetVolume(AudioType type) => volumes[type];

    public void SetVolume(AudioType type, float value) {
        volumes[type] = Mathf.Clamp01(value);
        if (audioBuses.TryGetValue(type, out var bus)) {
            bus.setVolume(value);
        }
    }

    public void PlayOneShot(EventReference soundEvent, Vector3 position) {
        RuntimeManager.PlayOneShot(soundEvent, position);
    }

    public void InitializeAmbience(EventReference ambientReference) {
        ambienceEventInstance = CreateEventInstance(ambientReference);
        StartEventInstance(ambienceEventInstance);
    }

    public void InitializeMusic(EventReference musicReference) {
        musicEventInstance = CreateEventInstance(musicReference);
        StartEventInstance(musicEventInstance);
    }

    private EventInstance CreateEventInstance(EventReference eventReference) {
        var eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public StudioEventEmitter CreateStudioEventEmitter(EventReference eventReference, GameObject emitterObject) {
        var emitter = emitterObject.GetComponent<StudioEventEmitter>();
        if (emitter == null) {
            Debug.LogError($"StudioEventEmitter not found on {emitterObject.name}");
            return null;
        }

        emitter.EventReference = eventReference;
        eventEmitters.Add(emitter);
        return emitter;
    }

    public void SetAmbienceParameter(string parameterName, float parameterValue) {
        ambienceEventInstance.setParameterByName(parameterName, parameterValue);
    }

    private void StartEventInstance(EventInstance eventInstance) {
        eventInstance.start();
    }

    private void StopAndReleaseAllEvents() {
        foreach (var eventInstance in eventInstances) {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
        eventInstances.Clear();
    }

    private void StopAllEmitters() {
        foreach (var emitter in eventEmitters) {
            emitter.Stop();
        }
        eventEmitters.Clear();
    }

    private void OnDestroy() {
        StopAndReleaseAllEvents();
        StopAllEmitters();
    }
}

public enum AudioType {
    MASTER,
    MUSIC,
    AMBIENCE,
    SFX
}
