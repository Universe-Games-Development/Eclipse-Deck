using System.Collections.Generic;

public struct BattleStartedEvent : IEvent {
    public List<Opponent> Opponents { get; }
    public BattleStartedEvent(List<Opponent> opponents) => Opponents = opponents;
}
public struct BattleEndEventData : IEvent {
    public Opponent Looser;

    public BattleEndEventData(Opponent testLooser) {
        this.Looser = testLooser;
    }
}
