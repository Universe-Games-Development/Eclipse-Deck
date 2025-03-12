#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonGenerator))]
public class GraphGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        DungeonGenerator generator = (DungeonGenerator)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Graph Generator Settings", EditorStyles.boldLabel);

        // Draw default inspector properties
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        // Add a button to regenerate the graph
        if (GUILayout.Button("Generate New Graph")) {
            // Видаляємо всі попередні вузли
            foreach (Transform child in generator.transform) {
                DestroyImmediate(child.gameObject);
            }

            // Викликаємо метод для генерації нового графа
            generator.SendMessage("GenerateDungeonGraph");
        }
    }
}
#endif