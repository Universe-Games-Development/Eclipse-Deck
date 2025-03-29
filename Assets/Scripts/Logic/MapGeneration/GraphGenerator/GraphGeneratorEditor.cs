//#if UNITY_EDITOR
//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(DungeonGenerator))]
//public class GraphGeneratorEditor : Editor {
//    public override void OnInspectorGUI() {
//        DungeonGenerator generator = (DungeonGenerator)target;

//        EditorGUILayout.Space();
//        EditorGUILayout.LabelField("Graph Generator Settings", EditorStyles.boldLabel);

//        // Draw default inspector properties
//        DrawDefaultInspector();

//        EditorGUILayout.Space();
//        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

//        // Перевіряємо, чи редактор знаходиться в Play Mode
//        if (EditorApplication.isPlaying) {
//            // Add a button to regenerate the graph
//            if (GUILayout.Button("Generate New Graph")) {
//                generator.SendMessage("GenerateDungeon");
//            }
//            if (GUILayout.Button("ClearDungeon")) {
//                generator.SendMessage("ClearDungeon");
//            }
//        } else {
//            // Інформація для користувача, якщо не Play Mode
//            EditorGUILayout.HelpBox("These actions are available only in Play Mode.", MessageType.Info);
//        }
//    }
//}
//#endif
