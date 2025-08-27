using System;
using UnityEngine;
using System.Diagnostics;

public enum LogLevel {
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

[Flags]
public enum LogCategory {
    None = 0,
    OperationManager = 1 << 0,
    TargetsFiller = 1 << 1,
    GameLogic = 1 << 2,
    UI = 1 << 3,
    Network = 1 << 4,
    Audio = 1 << 5,
    Animation = 1 << 6,
    AI = 1 << 7,
    Physics = 1 << 8,
    All = ~0
}

public interface ILogger {
    void Log(string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.None);
    void LogDebug(string message, LogCategory category = LogCategory.None);
    void LogInfo(string message, LogCategory category = LogCategory.None);
    void LogWarning(string message, LogCategory category = LogCategory.None);
    void LogError(string message, LogCategory category = LogCategory.None);
    void LogException(Exception exception, LogCategory category = LogCategory.None);

    bool IsEnabled(LogCategory category, LogLevel level = LogLevel.Debug);
    void SetCategoryEnabled(LogCategory category, bool enabled);
    void SetMinLogLevel(LogLevel minLevel);
}

[System.Serializable]
public class LoggingSettings {
    [Header("General Settings")]
    public LogLevel minLogLevel = LogLevel.Debug;
    public bool enableUnityConsole = true;
    public bool enableFileLogging = false;
    public bool enableTimestamps = true;
    public bool enableStackTrace = false;

    [Header("Category Settings")]
    public LogCategory enabledCategories = LogCategory.All;

    [Header("Colors (Editor Only)")]
    public Color debugColor = Color.white;
    public Color infoColor = Color.cyan;
    public Color warningColor = Color.yellow;
    public Color errorColor = Color.red;
}

// Extension methods for easier usage
public static class LoggerExtensions {
    public static void Log(this MonoBehaviour behaviour, string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.None) {
        GameLoggingService.Instance?.Log($"[{behaviour.GetType().Name}] {message}", level, category);
    }

    public static void LogDebug(this MonoBehaviour behaviour, string message, LogCategory category = LogCategory.None) {
        GameLoggingService.Instance?.LogDebug($"[{behaviour.GetType().Name}] {message}", category);
    }

    public static void LogInfo(this MonoBehaviour behaviour, string message, LogCategory category = LogCategory.None) {
        GameLoggingService.Instance?.LogInfo($"[{behaviour.GetType().Name}] {message}", category);
    }

    public static void LogWarning(this MonoBehaviour behaviour, string message, LogCategory category = LogCategory.None) {
        GameLoggingService.Instance?.LogWarning($"[{behaviour.GetType().Name}] {message}", category);
    }

    public static void LogError(this MonoBehaviour behaviour, string message, LogCategory category = LogCategory.None) {
        GameLoggingService.Instance?.LogError($"[{behaviour.GetType().Name}] {message}", category);
    }
}

// Static helper for non-MonoBehaviour classes
public static class GameLogger {
    public static void Log(string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.None) {
        GameLoggingService.Instance?.Log(message, level, category);
    }

    public static void LogDebug(string message, LogCategory category = LogCategory.None) {
        GameLoggingService.Instance?.LogDebug(message, category);
    }

    public static void LogInfo(string message, LogCategory category = LogCategory.None) {
        GameLoggingService.Instance?.LogInfo(message, category);
    }

    public static void LogWarning(string message, LogCategory category = LogCategory.None) {
        GameLoggingService.Instance?.LogWarning(message, category);
    }

    public static void LogError(string message, LogCategory category = LogCategory.None) {
        GameLoggingService.Instance?.LogError(message, category);
    }

    public static void LogException(Exception exception, LogCategory category = LogCategory.None) {
        GameLoggingService.Instance?.LogException(exception, category);
    }

    public static bool IsEnabled(LogCategory category, LogLevel level = LogLevel.Debug) {
        return GameLoggingService.Instance?.IsEnabled(category, level) ?? false;
    }
}