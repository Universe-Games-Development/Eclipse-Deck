using System.Collections.Generic;

public struct TurnChangeEventData : IEvent {
    public Opponent activeOpponent;
    public Opponent endTurnOpponent;

    public TurnChangeEventData(Opponent activeOpponent, Opponent endTurnOpponent) {
        this.activeOpponent = activeOpponent;
        this.endTurnOpponent = endTurnOpponent;
    }
}

public struct TurnEndEvent : IEvent {
    public Opponent endTurnOpponent;

    public TurnEndEvent(Opponent endTurnOpponent) {
        this.endTurnOpponent = endTurnOpponent;
    }
}

public struct TurnStartEventData : IEvent {
    public Opponent startTurnOpponent;

    public TurnStartEventData(Opponent startTurnOpponent) {
        this.startTurnOpponent = startTurnOpponent;
    }
}

public struct BattleStartEventData : IEvent {
    private List<Opponent> opponents;

    public BattleStartEventData(List<Opponent> opponents) {
        this.opponents = opponents;
    }
}

public struct BattleEndEventData : IEvent {
    private Opponent testLooser;
    private Opponent testWinner;

    public BattleEndEventData(Opponent testWinner, Opponent testLooser) {
        this.testWinner = testWinner;
        this.testLooser = testLooser;
    }
}
