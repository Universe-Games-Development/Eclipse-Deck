using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueSO))]
public class DialogueSOEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        DialogueSO dialogue = (DialogueSO)target;

        if (dialogue.pages == null) {
            dialogue.pages = new List<string>();
        }

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Сторінки діалогу:");

        for (int i = 0; i < dialogue.pages.Count; i++) {
            if (dialogue.pages[i] == null) {
                EditorGUILayout.LabelField($"Сторінка {i + 1}: (Null)");
                continue;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Сторінка {i + 1}:", GUILayout.Width(80));
            dialogue.pages[i] = EditorGUILayout.TextArea(dialogue.pages[i], GUILayout.Height(100));

            if (GUILayout.Button("X", GUILayout.Width(20))) {
                dialogue.pages.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Page")) {
            dialogue.pages.Add("New Page");
        }

        EditorGUILayout.EndVertical();

        if (GUI.changed) {
            EditorUtility.SetDirty(dialogue);
        }
    }
}
