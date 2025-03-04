using System.Collections.Generic;

public struct BattleStartedEvent : IEvent {
    public List<Opponent> Opponents { get; }
    public BattleStartedEvent(List<Opponent> opponents) => Opponents = opponents;
}
public struct BattleEndEventData : IEvent {
    private Opponent testLooser;

    public BattleEndEventData(Opponent testLooser) {
        this.testLooser = testLooser;
    }
}
