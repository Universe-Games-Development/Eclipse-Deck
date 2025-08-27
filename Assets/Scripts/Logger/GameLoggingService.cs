using System;
using System.Collections.Generic;
using UnityEngine;

public class GameLoggingService : MonoBehaviour, ILogger {
    public static GameLoggingService Instance { get; private set; }

    [SerializeField] private LoggingSettings settings = new LoggingSettings();

    private readonly Dictionary<LogCategory, bool> categoryStates = new Dictionary<LogCategory, bool>();
    private readonly Dictionary<LogLevel, Color> levelColors = new Dictionary<LogLevel, Color>();

    // Events for external log handlers
    public event Action<string, LogLevel, LogCategory> OnLogMessage;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLogger();
        } else {
            Destroy(gameObject);
        }
    }

    private void InitializeLogger() {
        // Initialize category states
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

        //Log("Logging service initialized", LogLevel.Info, LogCategory.None);
    }

    public void Log(string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.None) {
        if (!ShouldLog(category, level))
            return;

        var formattedMessage = FormatMessage(message, level, category);

        // Notify subscribers
        OnLogMessage?.Invoke(formattedMessage, level, category);

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

    // Runtime configuration methods
    [ContextMenu("Toggle Operation Manager Logs")]
    public void ToggleOperationManagerLogs() {
        var currentState = categoryStates.GetValueOrDefault(LogCategory.OperationManager, true);
        SetCategoryEnabled(LogCategory.OperationManager, !currentState);
    }

    [ContextMenu("Toggle Targets Filler Logs")]
    public void ToggleTargetsFillerLogs() {
        var currentState = categoryStates.GetValueOrDefault(LogCategory.TargetsFiller, true);
        SetCategoryEnabled(LogCategory.TargetsFiller, !currentState);
    }

    [ContextMenu("Enable All Categories")]
    public void EnableAllCategories() {
        SetCategoryEnabled(LogCategory.All, true);
    }

    [ContextMenu("Disable All Categories")]
    public void DisableAllCategories() {
        SetCategoryEnabled(LogCategory.All, false);
    }

    // Debug methods for editor
#if UNITY_EDITOR
    [ContextMenu("Test All Log Levels")]
    private void TestAllLogLevels() {
        LogDebug("This is a debug message", LogCategory.OperationManager);
        LogInfo("This is an info message", LogCategory.TargetsFiller);
        LogWarning("This is a warning message", LogCategory.GameLogic);
        LogError("This is an error message", LogCategory.UI);
    }

    [UnityEditor.CustomEditor(typeof(GameLoggingService))]
    public class GameLoggingServiceEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            var loggingService = (GameLoggingService)target;

            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.LabelField("Runtime Controls", UnityEditor.EditorStyles.boldLabel);

            if (Application.isPlaying) {
                // Show current category states
                foreach (LogCategory category in Enum.GetValues(typeof(LogCategory))) {
                    if (category != LogCategory.None && category != LogCategory.All) {
                        var isEnabled = loggingService.categoryStates.GetValueOrDefault(category, true);
                        var newState = UnityEditor.EditorGUILayout.Toggle(category.ToString(), isEnabled);

                        if (newState != isEnabled) {
                            loggingService.SetCategoryEnabled(category, newState);
                        }
                    }
                }

                UnityEditor.EditorGUILayout.Space();

                // Min log level control
                var newMinLevel = (LogLevel)UnityEditor.EditorGUILayout.EnumPopup("Min Log Level", loggingService.settings.minLogLevel);
                if (newMinLevel != loggingService.settings.minLogLevel) {
                    loggingService.SetMinLogLevel(newMinLevel);
                }
            } else {
                UnityEditor.EditorGUILayout.HelpBox("Runtime controls available only in play mode", UnityEditor.MessageType.Info);
            }
        }
    }
#endif
}
