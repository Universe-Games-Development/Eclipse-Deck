using System.Collections.Generic;

public struct OnTurnChange : IEvent {
    public Opponent activeOpponent;
    public Opponent endTurnOpponent;

    public OnTurnChange(Opponent activeOpponent, Opponent endTurnOpponent) {
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

public struct OnTurnStart : IEvent {
    public Opponent startTurnOpponent;

    public OnTurnStart(Opponent startTurnOpponent) {
        this.startTurnOpponent = startTurnOpponent;
    }
}

public struct OnBattleBegin : IEvent {
    private List<Opponent> opponents;

    public OnBattleBegin(List<Opponent> opponents) {
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
