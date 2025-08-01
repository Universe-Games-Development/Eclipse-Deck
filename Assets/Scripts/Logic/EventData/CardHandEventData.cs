

using System.Collections.Generic;

public struct CardPullEvent : IEvent {
    public Character Owner;
    public Card Card;

    public CardPullEvent(Character owner, Card card) {
        Owner = owner;
        Card = card;
    }

}

public struct CardDrawnEvent : IEvent {
    public Character Owner;
    public Card Card;

    public CardDrawnEvent(Character owner, Card card) {
        Owner = owner;
        Card = card;
    }
}