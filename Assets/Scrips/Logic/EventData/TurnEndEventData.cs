using System.Collections.Generic;

public class TurnChangeEventData {
    public Opponent activeOpponent;
    public Opponent endTurnOpponent;

    public TurnChangeEventData(Opponent activeOpponent, Opponent endTurnOpponent) {
        this.activeOpponent = activeOpponent;
        this.endTurnOpponent = endTurnOpponent;
    }
}

public class TurnEndEventData {
    public Opponent endTurnOpponent;

    public TurnEndEventData(Opponent endTurnOpponent) {
        this.endTurnOpponent = endTurnOpponent;
    }
}

public class TurnStartEventData {
    public Opponent startTurnOpponent;

    public TurnStartEventData(Opponent startTurnOpponent) {
        this.startTurnOpponent = startTurnOpponent;
    }
}

public class BattleStartEventData {
    private List<Opponent> opponents;

    public BattleStartEventData(List<Opponent> opponents) {
        this.opponents = opponents;
    }
}

// Card Events
public class CardDebufEvent {
    public Card card;
    // public Debuff debuff; 
}

// Creature Events