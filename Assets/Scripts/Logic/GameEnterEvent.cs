public struct GameEnterEvent : IEvent {
    public UnitModel Summoned;
    public GameEnterEvent(UnitModel summoned) {
        Summoned = summoned;
    }
}