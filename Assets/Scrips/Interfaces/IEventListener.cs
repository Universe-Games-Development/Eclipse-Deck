public interface IEventListener {
    void OnEvent(EventType eventType, GameContext data);
}
