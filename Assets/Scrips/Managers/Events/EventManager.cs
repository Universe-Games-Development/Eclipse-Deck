using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class EventManager : MonoBehaviour, IEventManager {
    private Dictionary<Type, Dictionary<Enum, List<EventListenerInfo>>> listeners =
        new Dictionary<Type, Dictionary<Enum, List<EventListenerInfo>>>();
    private Dictionary<Type, Dictionary<Enum, List<EventListenerInfo>>> phantomListeners =
        new Dictionary<Type, Dictionary<Enum, List<EventListenerInfo>>>();

    public void RegisterListener<T>(IEventListener listener, T eventType, ExecutionType executionType, int executionOrder = 0, bool isPhantomListener = false) where T : Enum {
        var targetDict = isPhantomListener ? GetOrAddPhantomDictionary<T>() : GetOrAddDictionary<T>();

        if (!targetDict.ContainsKey(eventType)) {
            targetDict[eventType] = new List<EventListenerInfo>();
        }
        targetDict[eventType].Add(new EventListenerInfo(listener, executionType, executionOrder));
    }

    public void UnregisterListener<T>(IEventListener listener, T eventType, bool isPhantomListener = false) where T : Enum {
        var targetDict = isPhantomListener ? GetOrAddPhantomDictionary<T>() : GetOrAddDictionary<T>();

        if (targetDict.ContainsKey(eventType)) {
            targetDict[eventType].RemoveAll(entry => entry.Listener == listener);
        }
    }

    public async UniTask TriggerEventAsync<T>(T eventType, GameContext gameContext, CancellationToken cancellationToken = default, bool isPhantomListener = false) where T : Enum {
        var targetDict = isPhantomListener ? GetOrAddPhantomDictionary<T>() : GetOrAddDictionary<T>();

        if (targetDict.ContainsKey(eventType)) {
            var eventListeners = new List<EventListenerInfo>(targetDict[eventType]);

            await TriggerParallelEventsAsync(eventListeners, gameContext, cancellationToken);
            await TriggerSequentialEventsAsync(eventListeners, gameContext, cancellationToken);
        }
    }

    private Dictionary<Enum, List<EventListenerInfo>> GetOrAddDictionary<T>() where T : Enum {
        var type = typeof(T);
        if (!listeners.ContainsKey(type)) {
            listeners[type] = new Dictionary<Enum, List<EventListenerInfo>>();
        }
        return listeners[type];
    }

    private Dictionary<Enum, List<EventListenerInfo>> GetOrAddPhantomDictionary<T>() where T : Enum {
        var type = typeof(T);
        if (!phantomListeners.ContainsKey(type)) {
            phantomListeners[type] = new Dictionary<Enum, List<EventListenerInfo>>();
        }
        return phantomListeners[type];
    }

    private async UniTask TriggerSequentialEventsAsync(List<EventListenerInfo> eventListeners, GameContext gameContext, CancellationToken cancellationToken) {
        foreach (var listenerInfo in eventListeners
                     .Where(l => l.ExecutionType == ExecutionType.Sequential)
                     .OrderBy(l => l.ExecutionOrder)) {
            await SafeEventInvokeAsync(listenerInfo.Listener, gameContext, cancellationToken);
        }
    }

    private async UniTask TriggerParallelEventsAsync(List<EventListenerInfo> eventListeners, GameContext gameContext, CancellationToken cancellationToken) {
        var parallelTasks = eventListeners
            .Where(l => l.ExecutionType == ExecutionType.Parallel)
            .Select(l => SafeEventInvokeAsync(l.Listener, gameContext, cancellationToken))
            .ToList();

        await UniTask.WhenAll(parallelTasks);
    }

    private async UniTask SafeEventInvokeAsync(IEventListener listener, GameContext gameContext, CancellationToken cancellationToken) {
        try {
            await listener.OnEventAsync(default, gameContext, cancellationToken);
        } catch (OperationCanceledException) {
            Debug.Log($"Event handling was cancelled.");
        } catch (Exception e) {
            Debug.LogError($"Error in event listener: {e.Message}\n{e.StackTrace}");
        }
    }

    private class EventListenerInfo {
        public readonly IEventListener Listener;
        public readonly ExecutionType ExecutionType;
        public readonly int ExecutionOrder;

        public EventListenerInfo(IEventListener listener, ExecutionType executionType, int executionOrder) {
            Listener = listener;
            ExecutionType = executionType;
            ExecutionOrder = executionOrder;
        }
    }
}
