using System.Collections.Generic;

public struct BattleStartedEvent : IEvent {
    public List<Character> Opponents { get; }
    public BattleStartedEvent(List<Character> opponents) => Opponents = opponents;
}
public struct BattleEndEventData : IEvent {
    public Character Looser;

    public BattleEndEventData(Character testLooser) {
        this.Looser = testLooser;
    }
}
