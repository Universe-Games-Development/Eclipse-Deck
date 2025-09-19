using System;
using System.Collections.Generic;
using UnityEngine;

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

public class GameLogger : ILogger {
    private readonly Dictionary<LogCategory, bool> categoryStates = new Dictionary<LogCategory, bool>();
    private readonly Dictionary<LogLevel, Color> levelColors = new Dictionary<LogLevel, Color>();

    private LoggingSettings settings;

    public GameLogger(LoggingSettings settings) {
        this.settings = settings;
        foreach (LogCategory category in Enum.GetValues(typeof(LogCategory))) {
            if (category != LogCategory.None && category != LogCategory.All) {
                categoryStates[category] = (settings.enabledCategories & category) != 0;
            }
        }

        // Initialize colors
        levelColors[LogLevel.Debug] = settings.debugColor;
        levelColors[LogLevel.Info] = settings.infoColor;
        levelColors[LogLevel.Warning] = settings.warningColor;
        levelColors[LogLevel.Error] = settings.errorColor;
    }
    public void Log(string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.None) {
        if (!ShouldLog(category, level))
            return;

        var formattedMessage = FormatMessage(message, level, category);

        // Log to Unity console
        if (settings.enableUnityConsole) {
            LogToUnityConsole(formattedMessage, level);
        }

        // Log to file (if enabled)
        if (settings.enableFileLogging) {
            LogToFile(formattedMessage, level, category);
        }
    }

    public void LogDebug(string message, LogCategory category = LogCategory.None) {
        Log(message, LogLevel.Debug, category);
    }

    public void LogInfo(string message, LogCategory category = LogCategory.None) {
        Log(message, LogLevel.Info, category);
    }

    public void LogWarning(string message, LogCategory category = LogCategory.None) {
        Log(message, LogLevel.Warning, category);
    }

    public void LogError(string message, LogCategory category = LogCategory.None) {
        Log(message, LogLevel.Error, category);
    }

    public void LogException(Exception exception, LogCategory category = LogCategory.None) {
        var message = $"Exception: {exception.Message}";
        if (settings.enableStackTrace) {
            message += $"\nStack Trace: {exception.StackTrace}";
        }

        Log(message, LogLevel.Error, category);
    }

    public bool IsEnabled(LogCategory category, LogLevel level = LogLevel.Debug) {
        return ShouldLog(category, level);
    }

    public void SetCategoryEnabled(LogCategory category, bool enabled) {
        if (category == LogCategory.All) {
            foreach (var key in new List<LogCategory>(categoryStates.Keys)) {
                categoryStates[key] = enabled;
            }
            settings.enabledCategories = enabled ? LogCategory.All : LogCategory.None;
        } else if (category != LogCategory.None) {
            categoryStates[category] = enabled;

            if (enabled)
                settings.enabledCategories |= category;
            else
                settings.enabledCategories &= ~category;
        }

        Log($"Category {category} {(enabled ? "enabled" : "disabled")}", LogLevel.Info, LogCategory.None);
    }

    public void SetMinLogLevel(LogLevel minLevel) {
        settings.minLogLevel = minLevel;
        Log($"Minimum log level set to {minLevel}", LogLevel.Info, LogCategory.None);
    }

    private bool ShouldLog(LogCategory category, LogLevel level) {
        // Check minimum log level
        if (level < settings.minLogLevel)
            return false;

        // Always allow logs without category or with None category
        if (category == LogCategory.None)
            return true;

        // Check if category is enabled
        return categoryStates.ContainsKey(category) && categoryStates[category];
    }

    private string FormatMessage(string message, LogLevel level, LogCategory category) {
        var formattedMessage = message;

        // Add timestamp
        if (settings.enableTimestamps) {
            formattedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {formattedMessage}";
        }

        // Add category prefix
        if (category != LogCategory.None) {
            var categoryPrefix = GetCategoryPrefix(category);
            formattedMessage = $"{categoryPrefix} {formattedMessage}";
        }

        // Add level prefix for non-info messages
        if (level != LogLevel.Info) {
            formattedMessage = $"[{level.ToString().ToUpper()}] {formattedMessage}";
        }

        return formattedMessage;
    }

    private string GetCategoryPrefix(LogCategory category) {
        return category switch {
            LogCategory.OperationManager => "[OM]",
            LogCategory.TargetsFiller => "[TF]",
            LogCategory.GameLogic => "[GL]",
            LogCategory.UI => "[UI]",
            LogCategory.Network => "[NET]",
            LogCategory.Audio => "[AUD]",
            LogCategory.Animation => "[ANIM]",
            LogCategory.AI => "[AI]",
            LogCategory.Physics => "[PHYS]",
            LogCategory.CardModule => "[CPM]",
            _ => $"[{category.ToString().ToUpper()}]"
        };
    }

    private void LogToUnityConsole(string message, LogLevel level) {
        // Add color in editor
#if UNITY_EDITOR
        if (levelColors.ContainsKey(level)) {
            var color = levelColors[level];
            var colorHex = ColorUtility.ToHtmlStringRGB(color);
            message = $"<color=#{colorHex}>{message}</color>";
        }
#endif

        switch (level) {
            case LogLevel.Debug:
            case LogLevel.Info:
                UnityEngine.Debug.Log(message);
                break;
            case LogLevel.Warning:
                UnityEngine.Debug.LogWarning(message);
                break;
            case LogLevel.Error:
                UnityEngine.Debug.LogError(message);
                break;
        }
    }

    private void LogToFile(string message, LogLevel level, LogCategory category) {
        // File logging implementation
        // This would write to a file in persistent data path
        try {
            var logPath = System.IO.Path.Combine(Application.persistentDataPath, "game_logs.txt");
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] [{category}] {message}\n";
            System.IO.File.AppendAllText(logPath, logEntry);
        } catch (Exception ex) {
            UnityEngine.Debug.LogError($"Failed to write to log file: {ex.Message}");
        }
    }
}