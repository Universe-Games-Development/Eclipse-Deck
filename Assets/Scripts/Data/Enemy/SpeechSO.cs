using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpeech", menuName = "Dialogue/Speech")]
public class SpeechSO : ScriptableObject {
    [Header("Character Metadata")]
    public AudioClip speechSound; // Звук для озвучення
    public string characterName; // Ім'я персонажа
    public Sprite characterPortrait; // Портрет персонажа

    [Header("Dialogue Data")]
    public List<DialogueData> dialogue; // Текст діалогу
}
