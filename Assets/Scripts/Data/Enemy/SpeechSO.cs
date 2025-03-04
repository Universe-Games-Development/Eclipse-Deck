using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpeech", menuName = "Dialogue/Speech")]
public class SpeechSO : ScriptableObject {
    [Header("Character Metadata")]
    public AudioClip speechSound; // ���� ��� ���������
    public string characterName; // ��'� ���������
    public Sprite characterPortrait; // ������� ���������

    [Header("Dialogue Data")]
    public List<DialogueData> dialogue; // ����� ������
}
