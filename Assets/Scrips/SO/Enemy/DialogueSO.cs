using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue")]
public class DialogueSO : ScriptableObject {
    [Header("Dialogue Information")]
    public List<string> pages = new List<string>(); // Список сторінок тексту

    [Header("Event Link")]
    public EventType triggerEvent; // Подія, яка може запускати діалог (nullable)
}
