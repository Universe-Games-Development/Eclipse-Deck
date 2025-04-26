public struct GameEnterEvent : IEvent {
    public IGameUnit Summoned;
    public GameEnterEvent(IGameUnit summoned) {
        Summoned = summoned;
    }
}