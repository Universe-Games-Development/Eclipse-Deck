public struct GameEnterEvent : IEvent {
    public UnitInfo Summoned;
    public GameEnterEvent(UnitInfo summoned) {
        Summoned = summoned;
    }
}