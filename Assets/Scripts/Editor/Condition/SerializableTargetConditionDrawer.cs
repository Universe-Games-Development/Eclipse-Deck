using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SerializableTargetCondition<>), true)]
public class SerializableTargetConditionDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        try {
            // ���������� �������� ������������ ��'�
            string displayName = GetDisplayName(property);

            // ���� �� ������� �������� ������������ ��'�, ������������� ����������
            if (string.IsNullOrEmpty(displayName)) {
                displayName = label.text;
            }

            // ������� ���� � ������������ ����
            EditorGUI.PropertyField(position, property, new GUIContent(displayName), true);
        } catch (System.Exception ex) {
            // � ��� ������� ���������� ���������� ���� � ����������� ��� �������
            Debug.LogWarning($"Failed to draw SerializableTargetCondition: {ex.Message}");
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    private string GetDisplayName(SerializedProperty property) {
        try {
            // �������� ��'��� ��� ������� GetDisplayName()
            object targetObject = GetTargetObjectOfProperty(property);

            if (targetObject == null) {
                return "Null Condition";
            }

            // ����������, �� ������ ��'��� ��������� ISerializableCondition
            if (targetObject is ISerializableCondition condition) {
                return condition.GetDisplayName() ?? "Unnamed Condition";
            }

            // �������������� ����� ����� �������� (�� �������, ���� ��������� �� ��������� ����)
            var method = targetObject.GetType().GetMethod("GetDisplayName",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            if (method != null && method.ReturnType == typeof(string)) {
                var result = method.Invoke(targetObject, null) as string;
                return result ?? targetObject.GetType().Name;
            }

            return targetObject.GetType().Name;
        } catch (System.Exception ex) {
            Debug.LogWarning($"Failed to get display name: {ex.Message}");
            return "Error Getting Name";
        }
    }

    // ��������� ����� ������ ��� ��������� ��'���� � SerializedProperty
    private object GetTargetObjectOfProperty(SerializedProperty prop) {
        if (prop == null || prop.serializedObject == null || prop.serializedObject.targetObject == null)
            return null;

        try {
            string path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            string[] elements = path.Split('.');

            foreach (string element in elements) {
                if (element.Contains("[")) {
                    string elementName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    string indexStr = element.Substring(element.IndexOf("[", StringComparison.Ordinal))
                        .Replace("[", "").Replace("]", "");

                    if (int.TryParse(indexStr, out int index)) {
                        obj = GetValueFromEnumerable(obj, elementName, index);
                    } else {
                        return null;
                    }
                } else {
                    obj = GetFieldOrPropertyValue(obj, element);
                }

                if (obj == null) break;
            }
            return obj;
        } catch (System.Exception ex) {
            Debug.LogWarning($"Error getting target object: {ex.Message}");
            return null;
        }
    }

    private object GetValueFromEnumerable(object source, string name, int index) {
        var enumerable = GetFieldOrPropertyValue(source, name) as IEnumerable;
        if (enumerable == null) return null;

        var enumerator = enumerable.GetEnumerator();
        int currentIndex = 0;

        while (enumerator.MoveNext()) {
            if (currentIndex == index)
                return enumerator.Current;
            currentIndex++;
        }

        return null;
    }

    private object GetFieldOrPropertyValue(object source, string name) {
        if (source == null) return null;

        Type type = source.GetType();

        // �������� ������ ����
        FieldInfo field = type.GetField(name,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (field != null)
            return field.GetValue(source);

        // ���� ������ ����������
        PropertyInfo property = type.GetProperty(name,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
        if (property != null && property.CanRead)
            return property.GetValue(source);

        // ���� �� �������, ������� � ����������� ������
        Type baseType = type.BaseType;
        while (baseType != null && baseType != typeof(object)) {
            field = baseType.GetField(name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
                return field.GetValue(source);

            property = baseType.GetProperty(name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null && property.CanRead)
                return property.GetValue(source);

            baseType = baseType.BaseType;
        }

        return null;
    }
}
#endif