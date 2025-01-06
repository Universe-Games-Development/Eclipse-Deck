using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueSO))]
public class DialogueSOEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        DialogueSO dialogue = (DialogueSO)target;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Сторінки діалогу:");

        for (int i = 0; i < dialogue.pages.Count; i++) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Сторінка {i + 1}:", GUILayout.Width(80)); // Заголовок з номером сторінки
            dialogue.pages[i] = EditorGUILayout.TextArea(dialogue.pages[i], GUILayout.Height(100)); // Поле для тексту сторінки

            if (GUILayout.Button("X", GUILayout.Width(20))) {
                dialogue.pages.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }
}