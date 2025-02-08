public class CardHandEventData {
    public Opponent Owner { get; private set; }
    public Card Card { get; private set; }

    public CardHandEventData(Opponent owner, Card card) {
        Owner = owner;
        Card = card;
    }
}