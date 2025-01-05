using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EventManager : MonoBehaviour, IEventManager {
    private Dictionary<EventType, List<EventListenerInfo>> listeners =
        new Dictionary<EventType, List<EventListenerInfo>>();

    public void RegisterListener(IEventListener listener, EventType eventType, ExecutionType executionType, int executionOrder = 0) {
        if (!listeners.ContainsKey(eventType)) {
            listeners[eventType] = new List<EventListenerInfo>();
        }
        listeners[eventType].Add(new EventListenerInfo(listener, executionType, executionOrder));
    }

    public void UnregisterListener(IEventListener listener, EventType eventType) {
        if (listeners.ContainsKey(eventType)) {
            listeners[eventType].RemoveAll(entry => entry.Listener == listener);
        }
    }

    public async UniTask TriggerEventAsync(EventType eventType, GameContext gameContext, CancellationToken cancellationToken = default) {
        if (listeners.ContainsKey(eventType)) {
            var eventListeners = new List<EventListenerInfo>(listeners[eventType]);

            await TriggerParallelEventsAsync(eventListeners, eventType, gameContext, cancellationToken);
            await TriggerSequentialEventsAsync(eventListeners, eventType, gameContext, cancellationToken);
        }
    }

    private async UniTask TriggerSequentialEventsAsync(List<EventListenerInfo> eventListeners, EventType eventType, GameContext gameContext, CancellationToken cancellationToken) {
        foreach (var listenerInfo in eventListeners
                     .Where(l => l.ExecutionType == ExecutionType.Sequential)
                     .OrderBy(l => l.ExecutionOrder)) {
            await SafeEventInvokeAsync(listenerInfo.Listener, eventType, gameContext, cancellationToken);
        }
    }

    private async UniTask TriggerParallelEventsAsync(List<EventListenerInfo> eventListeners, EventType eventType, GameContext gameContext, CancellationToken cancellationToken) {
        var parallelTasks = eventListeners
            .Where(l => l.ExecutionType == ExecutionType.Parallel)
            .Select(l => SafeEventInvokeAsync(l.Listener, eventType, gameContext, cancellationToken))
            .ToList();

        await UniTask.WhenAll(parallelTasks);
    }

    private async UniTask SafeEventInvokeAsync(IEventListener listener, EventType eventType, GameContext gameContext, CancellationToken cancellationToken) {
        try {
            await listener.OnEventAsync(eventType, gameContext, cancellationToken); // Передача токена в обробник
        } catch (OperationCanceledException) {
            // Обробка скасування
            Debug.Log($"Event {eventType} handling was cancelled.");
        } catch (Exception e) {
            Debug.LogError($"Error in event listener for {eventType}: {e.Message}\n{e.StackTrace}");
        }
    }
}

public class EventListenerInfo {
    public readonly IEventListener Listener;
    public readonly ExecutionType ExecutionType;
    public readonly int ExecutionOrder;

    public EventListenerInfo(IEventListener listener, ExecutionType executionType, int executionOrder) {
        Listener = listener;
        ExecutionType = executionType;
        ExecutionOrder = executionOrder;
    }
}

public enum ExecutionType {
    Sequential,
    Parallel
}
