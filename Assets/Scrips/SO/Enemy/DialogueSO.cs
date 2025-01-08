using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue")]
public class DialogueSO : ScriptableObject {
    [Header("Dialogue Information")]
    public List<string> pages = new List<string>(); // ������ ������� ������

    [Header("Event Link")]
    public EventType triggerEvent; // ����, ��� ���� ��������� ����� (nullable)
}
