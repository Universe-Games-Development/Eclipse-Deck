using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;

[CustomPropertyDrawer(typeof(EventEnum))]
public class GameEventTypeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        // Получаем все зарегистрированные типы событий через рефлексию
        var registeredEventTypes = GetRegisteredEventTypes();

        // Находим текущее выбранное значение
        var currentValue = (GameEventType)property.enumValueIndex;

        // Создаем popup с только зарегистрированными типами
        var selectedIndex = EditorGUI.Popup(
            position,
            label.text,
            registeredEventTypes.ToList().IndexOf(currentValue),
            registeredEventTypes.Select(e => e.ToString()).ToArray()
        );

        // Обновляем значение свойства
        if (selectedIndex >= 0) {
            property.enumValueIndex = (int)registeredEventTypes[selectedIndex];
        }

        EditorGUI.EndProperty();
    }

    private GameEventType[] GetRegisteredEventTypes() {
        // Используем рефлексию для получения приватного поля EnumToTypeMap
        var enumToTypeMapField = typeof(GameEventMapper).GetField(
            "EnumToTypeMap",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Static
        );

        var enumToTypeMap = enumToTypeMapField?.GetValue(null) as System.Collections.IDictionary;

        if (enumToTypeMap == null) {
            Debug.LogError("Could not access GameEventMapper's EnumToTypeMap");
            return new[] { GameEventType.UNKNOWN_EVENT };
        }

        // Получаем только ключи (зарегистрированные типы)
        return enumToTypeMap.Keys
            .Cast<GameEventType>()
            .Where(key => key != GameEventType.UNKNOWN_EVENT)
            .ToArray();
    }
}

[ExecuteInEditMode]
public static class GameEventMapper {
    // Словарь для маппинга типов событий к enum
    private static readonly Dictionary<Type, EventEnum> TypeToEnumMap =
        new Dictionary<Type, EventEnum>();

    // Словарь для обратного маппинга
    private static readonly Dictionary<EventEnum, Type> EnumToTypeMap =
        new Dictionary<EventEnum, Type>();

    public static void RegisterEventType<TEvent>(EventEnum eventTypeEnum)
        where TEvent : IEvent {
        var eventType = typeof(TEvent);

        if (TypeToEnumMap.ContainsKey(eventType)) {
            Debug.LogWarning($"Event type {eventType.Name} is already registered.");
            return;
        }

        TypeToEnumMap[eventType] = eventTypeEnum;
        EnumToTypeMap[eventTypeEnum] = eventType;
    }

    public static EventEnum GetEventTypeEnum<TEvent>()
        where TEvent : IEvent {
        var eventType = typeof(TEvent);
        return TypeToEnumMap.TryGetValue(eventType, out var enumType)
            ? enumType
            : EventEnum.OnManaSpent;
    }

    // Получить тип события по enum
    public static Type GetEventType(EventEnum eventType) {
        return EnumToTypeMap.TryGetValue(eventType, out var type)
            ? type
            : null;
    }

    private static  void DEbug() {

    }
    static GameEventMapper() {
        // Здесь регистрируем все известные типы событий
        RegisterEventType<BattleStartedEvent>(EventEnum.BattleStarted);
    }
}

