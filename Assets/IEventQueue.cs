using System;

public interface IEventQueue {
    void RegisterListener<T>(IEventListener listener, T eventType) where T : Enum;
    void TriggerEvent<T>(T eventType, object eventData) where T : Enum;
    void UnregisterListener<T>(IEventListener listener, T eventType) where T : Enum;
}