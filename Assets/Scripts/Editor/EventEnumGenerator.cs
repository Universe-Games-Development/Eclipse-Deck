using System;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EventEnumGenerator {
    private const string ENUM_TEMPLATE = @"// Auto-generated enum for all IEvent types
public enum EventEnum
{{
{0}
}}";
    // Lol

    [InitializeOnLoadMethod]
    private static void Initialize() {
        // ��������� enum ��� ������� ���������
        GenerateEventEnum();
    }

    [MenuItem("Tools/Regenerate Event Enum")]
    public static void RegenerateEventEnumManually() {
        GenerateEventEnum();
        AssetDatabase.Refresh();
    }

    public static void GenerateEventEnum() {
        var eventTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IEvent).IsAssignableFrom(t) && t.IsValueType && !t.IsEnum)
            .ToList();

        var enumEntries = eventTypes
            .Select((type, index) => $"    {SanitizeEnumName(type.Name)} = {index},")
            .ToList();

        var enumContent = string.Format(ENUM_TEMPLATE, string.Join("\n", enumEntries));

        string folderPath = "Assets/Scripts/Generated";
        string filePath = Path.Combine(folderPath, "EventEnum.cs");

        // ������� �����, ���� ��� �� ����������
        Directory.CreateDirectory(folderPath);

        // ���������� enum � ����
        File.WriteAllText(filePath, enumContent);

        Debug.Log($"Generated Event Enum with {enumEntries.Count} entries");
    }

    private static string SanitizeEnumName(string name) {
        // ������� ������������ ������� � �������� ��
        return new string(name
            .Replace("Event", "")
            .Select(c => char.IsLetterOrDigit(c) ? c : '_')
            .ToArray());
    }
}