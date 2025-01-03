using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour, IEventManager {
    private Dictionary<EventType, List<IEventListener>> listeners = new Dictionary<EventType, List<IEventListener>>();

    public void RegisterListener(IEventListener listener, EventType eventType) {
        if (!listeners.ContainsKey(eventType)) {
            listeners[eventType] = new List<IEventListener>();
        }
        listeners[eventType].Add(listener);
    }

    public void UnregisterListener(IEventListener listener, EventType eventType) {
        if (listeners.ContainsKey(eventType)) {
            listeners[eventType].Remove(listener);
        }
    }

    public void TriggerEvent(EventType eventType, GameContext gameContext) {
        if (listeners.ContainsKey(eventType)) {
            foreach (var listener in listeners[eventType]) {
                listener.OnEvent(eventType, gameContext);
            }
        }
    }
}