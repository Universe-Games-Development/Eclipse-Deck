using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EventQueue : IEventQueue {
    [Inject] private CommandManager _commandManager;
    private Dictionary<Type, Dictionary<Enum, List<IEventListener>>> listeners = new();

    public EventQueue(CommandManager commandManager) {
        _commandManager = commandManager;
    }

    public void RegisterListener<T>(IEventListener listener, T eventType) where T : Enum {
        var targetDict = GetOrAddDictionary<T>();

        if (!targetDict.ContainsKey(eventType)) {
            targetDict[eventType] = new List<IEventListener>();
        }
        targetDict[eventType].Add(listener);
    }

    public void UnregisterListener<T>(IEventListener listener, T eventType) where T : Enum {
        var targetDict = GetDictionary<T>();

        if (targetDict == null) {
            Debug.LogWarning($"Trying to unregister listener for an absent event type: {typeof(T)}");
            return;
        }

        if (targetDict.TryGetValue(eventType, out List<IEventListener> listeners)) {
            if (listeners.Remove(listener)) {
                Debug.Log($"Listener successfully unregistered from event: {eventType}");
            } else {
                Debug.LogWarning("Trying to unregister a listener that was not found.");
            }
        } else {
            Debug.LogWarning($"Trying to unregister from an absent event list: {eventType}");
        }
    }

    public void TriggerEvent<T>(T eventType, object eventData) where T : Enum {
        var targetDict = GetDictionary<T>();
        if (targetDict == null || !targetDict.ContainsKey(eventType)) {
            Debug.Log($"No listeners found for event: {eventType}");
            return;
        }

        if (eventData == null) {
            Debug.LogWarning($"Event {eventType} received null data!");
        }

        List<ICommand> commands = new();

        foreach (var listener in targetDict[eventType]) {
            object result = listener.OnEventReceived(eventData);

            switch (result) {
                case ICommand singleCommand:
                    commands.Add(singleCommand);
                    break;
                case List<ICommand> commandList:
                    commands.AddRange(commandList);
                    break;
                case null:
                    Debug.LogWarning($"Listener {listener} returned null for event {eventType}");
                    break;
                default:
                    Debug.LogError($"Unexpected type {result.GetType()} returned by listener {listener} for event {eventType}");
                    break;
            }
        }

        _commandManager.EnqueueCommands(commands);
    }


    private Dictionary<Enum, List<IEventListener>> GetOrAddDictionary<T>() where T : Enum {
        var type = typeof(T);
        if (!listeners.ContainsKey(type)) {
            listeners[type] = new Dictionary<Enum, List<IEventListener>>();
        }
        return listeners[type];
    }

    private Dictionary<Enum, List<IEventListener>> GetDictionary<T>() where T : Enum {
        listeners.TryGetValue(typeof(T), out var dictionary);
        return dictionary;
    }
}
