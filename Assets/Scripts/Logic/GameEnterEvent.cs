public struct GameEnterEvent : IEvent {
    public GameUnit Summoned;
    public GameEnterEvent(GameUnit summoned) {
        Summoned = summoned;
    }
}