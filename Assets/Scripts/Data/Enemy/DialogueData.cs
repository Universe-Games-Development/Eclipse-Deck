using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue")]
public class DialogueData : ScriptableObject {
    [Header("Dialogue Information")]
    public List<string> pages = new List<string>(); // Список сторінок тексту
    public AudioClip SpeechSound; // Optional audio for this specific page

    [Header("Event Link")]
    public EventEnum TriggerEventType;

    public Type GetTriggerEventType() {
        return GameEventMapper.GetEventType(TriggerEventType);
    }
}
