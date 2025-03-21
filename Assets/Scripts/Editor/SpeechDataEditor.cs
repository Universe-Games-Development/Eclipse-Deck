using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(SpeechData))]
public class SpeechDataEditor : Editor {
    private SerializedProperty speechSound;
    private SerializedProperty characterName;
    private SerializedProperty characterPortrait;
    private SerializedProperty storyDialogues;
    private SerializedProperty eventDialogues;
    private SerializedProperty typingSpeed;

    private Dictionary<string, Editor> storyDialogueEditors = new Dictionary<string, Editor>();
    private Dictionary<string, Editor> eventDialogueEditors = new Dictionary<string, Editor>();
    private Dictionary<string, bool> storyDialogueFoldouts = new Dictionary<string, bool>();
    private Dictionary<string, bool> eventDialogueFoldouts = new Dictionary<string, bool>();

    private void OnEnable() {
        speechSound = serializedObject.FindProperty("speechSound");
        characterName = serializedObject.FindProperty("characterName");
        characterPortrait = serializedObject.FindProperty("characterPortrait");
        storyDialogues = serializedObject.FindProperty("storyDialogues");
        eventDialogues = serializedObject.FindProperty("eventDialogues");
        typingSpeed = serializedObject.FindProperty("typingSpeed");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.LabelField("Character Metadata", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(speechSound);
        EditorGUILayout.PropertyField(characterName);
        EditorGUILayout.PropertyField(characterPortrait);
        EditorGUILayout.PropertyField(typingSpeed);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dialogue Data", EditorStyles.boldLabel);

        // Story Dialogues
        EditorGUILayout.LabelField("Story Dialogues", EditorStyles.boldLabel);
        DrawStoryDialogues();

        // Event Dialogues
        EditorGUILayout.LabelField("Event Dialogues", EditorStyles.boldLabel);
        DrawEventDialogues();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawStoryDialogues() {
        for (int i = 0; i < storyDialogues.arraySize; i++) {
            SerializedProperty dialogueProperty = storyDialogues.GetArrayElementAtIndex(i);
            StoryDialogueData dialogueData = dialogueProperty.objectReferenceValue as StoryDialogueData;

            if (dialogueData != null) {
                string key = dialogueData.GetInstanceID().ToString();

                // Create foldout and get editor
                if (!storyDialogueFoldouts.ContainsKey(key)) {
                    storyDialogueFoldouts[key] = false;
                }

                EditorGUILayout.BeginHorizontal();
                storyDialogueFoldouts[key] = EditorGUILayout.Foldout(storyDialogueFoldouts[key],
                    dialogueData.name, true);

                // Add buttons for inline editing
                if (GUILayout.Button("Edit", GUILayout.Width(50))) {
                    Selection.activeObject = dialogueData;
                }

                if (GUILayout.Button("Remove", GUILayout.Width(70))) {
                    storyDialogues.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    return;
                }
                EditorGUILayout.EndHorizontal();

                // Draw inline editor
                if (storyDialogueFoldouts[key]) {
                    if (!storyDialogueEditors.ContainsKey(key)) {
                        storyDialogueEditors[key] = Editor.CreateEditor(dialogueData);
                    }

                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    storyDialogueEditors[key].OnInspectorGUI();
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
            } else {
                EditorGUILayout.PropertyField(dialogueProperty);
            }
        }

        // Add new story dialogue button
        if (GUILayout.Button("Add Story Dialogue")) {
            AddNewStoryDialogue();
        }
    }

    private void DrawEventDialogues() {
        for (int i = 0; i < eventDialogues.arraySize; i++) {
            SerializedProperty dialogueProperty = eventDialogues.GetArrayElementAtIndex(i);
            BaseDialogueData dialogueData = dialogueProperty.objectReferenceValue as BaseDialogueData;

            if (dialogueData != null) {
                string key = dialogueData.GetInstanceID().ToString();

                // Create foldout and get editor
                if (!eventDialogueFoldouts.ContainsKey(key)) {
                    eventDialogueFoldouts[key] = false;
                }

                EditorGUILayout.BeginHorizontal();
                eventDialogueFoldouts[key] = EditorGUILayout.Foldout(eventDialogueFoldouts[key],
                    dialogueData.name, true);

                // Add buttons for inline editing
                if (GUILayout.Button("Edit", GUILayout.Width(50))) {
                    Selection.activeObject = dialogueData;
                }

                if (GUILayout.Button("Remove", GUILayout.Width(70))) {
                    eventDialogues.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    return;
                }
                EditorGUILayout.EndHorizontal();

                // Draw inline editor
                if (eventDialogueFoldouts[key]) {
                    if (!eventDialogueEditors.ContainsKey(key)) {
                        eventDialogueEditors[key] = Editor.CreateEditor(dialogueData);
                    }

                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    eventDialogueEditors[key].OnInspectorGUI();
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
            } else {
                EditorGUILayout.PropertyField(dialogueProperty);
            }
        }

        // Add new event dialogue button
        if (GUILayout.Button("Add Event Dialogue")) {
            // Note: This requires an implementation based on your specific event dialogue types
            // You'll need to modify this to create the correct type of event dialogue
            AddNewEventDialogue();
        }
    }

    private void AddNewStoryDialogue() {
        StoryDialogueData newDialogue = ScriptableObject.CreateInstance<StoryDialogueData>();
        newDialogue.name = "New Story Dialogue";

        // Create asset
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Story Dialogue",
            "NewStoryDialogue",
            "asset",
            "Please enter a filename for the new story dialogue");

        if (!string.IsNullOrEmpty(path)) {
            AssetDatabase.CreateAsset(newDialogue, path);
            AssetDatabase.SaveAssets();

            // Add to list
            storyDialogues.arraySize++;
            storyDialogues.GetArrayElementAtIndex(storyDialogues.arraySize - 1).objectReferenceValue = newDialogue;
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void AddNewEventDialogue() {
        // This implementation depends on your specific event dialogue types
        // You'll need to create a menu or selection dialog to choose the type of event dialogue
        EditorUtility.DisplayDialog("Add Event Dialogue",
            "Implementation needed for specific event dialogue types. " +
            "This requires knowing the concrete types that inherit from BaseDialogueData.", "OK");
    }
}