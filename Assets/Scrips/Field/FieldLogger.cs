using UnityEngine;

public static class FieldLogger {
    public static void Log(string message) {
        Debug.Log($"[Field Log] {message}");
    }

    public static void Warning(string message) {
        Debug.LogWarning($"[Field Warning] {message}");
    }

    public static void Error(string message) {
        Debug.LogError($"[Field Error] {message}");
    }
}

