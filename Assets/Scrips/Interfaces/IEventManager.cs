public interface IEventManager
{
    public void RegisterListener(IEventListener listener, EventType eventType);
    public void UnregisterListener(IEventListener listener, EventType eventType);
    public void TriggerEvent(EventType eventType, GameContext gameContext);
}
