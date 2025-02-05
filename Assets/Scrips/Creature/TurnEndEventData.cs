public class TurnEndEventData {
    public Opponent activePlayer; // previous player

    public TurnEndEventData(Opponent activePlayer) {
        this.activePlayer = activePlayer;
    }
}

// Card Events
public class CardDebufEvent {
    public Card card;
    // public Debuff debuff; 
}

// Creature Events