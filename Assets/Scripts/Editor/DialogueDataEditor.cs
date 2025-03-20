using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(DialogueSet))]
public class DialoguePageDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        // Відступ для вкладених елементів
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Отримуємо властивість lines
        var linesProperty = property.FindPropertyRelative("messages");
        if (linesProperty != null) {
            EditorGUI.PropertyField(position, linesProperty, true);
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        var linesProperty = property.FindPropertyRelative("messages");
        return EditorGUI.GetPropertyHeight(linesProperty, true);
    }
}