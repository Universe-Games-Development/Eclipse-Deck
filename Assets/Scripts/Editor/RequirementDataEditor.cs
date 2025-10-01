#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OperationData), true)] // true - працює для всіх нащадків
public class OperationDataEditor : Editor {
    private SerializedProperty _requirementsProp;
    private SerializedProperty _visualDataProp;

    private void OnEnable() {
        _requirementsProp = serializedObject.FindProperty("requirements");
        _visualDataProp = serializedObject.FindProperty("visualData");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.LabelField("Operation Data", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // --- Спеціальні поля нащадків ---
        DrawDerivedClassFields();
        EditorGUILayout.Space();

        // --- Візуальні Дані ---
        DrawVisualData();
        EditorGUILayout.Space();

        // --- Вимоги ---
        EditorGUILayout.LabelField("Operation Requirements", EditorStyles.boldLabel);
        DrawRequirementsList();
        EditorGUILayout.Space();

        DrawAddRequirementButton();

        serializedObject.ApplyModifiedProperties();
    }

    // Відображає всі поля, які не є requirements і visualData
    private void DrawDerivedClassFields() {
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren)) {
            enterChildren = false;

            // Пропускаємо службові поля та базові поля
            if (prop.name == "m_Script" ||
                prop.name == "requirements" ||
                prop.name == "visualData") {
                continue;
            }

            EditorGUILayout.PropertyField(prop, true);
        }
    }

    private void DrawVisualData() {
        EditorGUILayout.PropertyField(_visualDataProp, true);
    }

    private void DrawRequirementsList() {
        for (int i = 0; i < _requirementsProp.arraySize; i++) {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();

            // Заголовок з номером вимоги
            EditorGUILayout.LabelField($"Requirement #{i + 1}", EditorStyles.boldLabel);

            // Кнопки управління
            if (GUILayout.Button("↑", GUILayout.Width(25)) && i > 0) {
                _requirementsProp.MoveArrayElement(i, i - 1);
            }
            if (GUILayout.Button("↓", GUILayout.Width(25)) && i < _requirementsProp.arraySize - 1) {
                _requirementsProp.MoveArrayElement(i, i + 1);
            }
            if (GUILayout.Button("×", GUILayout.Width(25))) {
                RemoveRequirement(i);
                break;
            }

            EditorGUILayout.EndHorizontal();

            // Відображаємо вимогу
            var requirementProp = _requirementsProp.GetArrayElementAtIndex(i);
            if (requirementProp.objectReferenceValue != null) {
                var editor = CreateEditor(requirementProp.objectReferenceValue);
                editor.OnInspectorGUI();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

    private void DrawAddRequirementButton() {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Add New Requirement", GUILayout.Width(200))) {
            CreateNewRequirement();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void CreateNewRequirement() {
        var newRequirement = CreateInstance<RequirementData>();
        newRequirement.name = $"Requirement {Guid.NewGuid().ToString().Substring(0, 8)}";

        AssetDatabase.AddObjectToAsset(newRequirement, target);
        _requirementsProp.arraySize++;
        _requirementsProp.GetArrayElementAtIndex(_requirementsProp.arraySize - 1)
            .objectReferenceValue = newRequirement;

        AssetDatabase.SaveAssets();
    }

    private void RemoveRequirement(int index) {
        var requirement = _requirementsProp.GetArrayElementAtIndex(index).objectReferenceValue;
        if (requirement != null) {
            DestroyImmediate(requirement, true);
        }

        _requirementsProp.DeleteArrayElementAtIndex(index);
        AssetDatabase.SaveAssets();
    }
}
#endif