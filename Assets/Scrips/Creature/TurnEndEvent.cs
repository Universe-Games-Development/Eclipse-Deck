public class TurnEndEvent {
    public Opponent activePlayer; // previous player

    public TurnEndEvent(Opponent activePlayer) {
        this.activePlayer = activePlayer;
    }
}

// Card Events
public class CardDebufEvent {
    public Card card;
    // public Debuff debuff; 
}

// Creature Events