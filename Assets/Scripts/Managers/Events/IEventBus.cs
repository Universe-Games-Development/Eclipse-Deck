using System;

public interface IEventBus<TBaseEvent> {
    bool CurrentEventIsConsumed { get; }
    bool IsEventBeingRaised { get; }

    void ClearListeners<TEvent>() where TEvent : TBaseEvent;
    void ConsumeCurrentEvent();
    void Dispose();
    bool Raise<TEvent>(in TEvent @event) where TEvent : TBaseEvent;
    bool RaiseImmediately<TEvent>(ref TEvent @event) where TEvent : TBaseEvent;
    bool RaiseImmediately<TEvent>(TEvent @event) where TEvent : TBaseEvent;
    void SubscribeTo<TEvent>(GenericEventBus<TBaseEvent>.EventHandler<TEvent> handler, float priority = 0) where TEvent : TBaseEvent;
    void UnsubscribeFrom<TEvent>(GenericEventBus<TBaseEvent>.EventHandler<TEvent> handler) where TEvent : TBaseEvent;
}