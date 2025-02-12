

using System.Collections.Generic;

public struct CardPullEvent : IEvent {
    public Opponent Owner;
    public Card Card;

    public CardPullEvent(Opponent owner, Card card) {
        Owner = owner;
        Card = card;
    }

}

public struct CardDrawnEvent : IEvent {
    public Opponent Owner;
    public Card Card;

    public CardDrawnEvent(Opponent owner, Card card) {
        Owner = owner;
        Card = card;
    }
}

public struct EndTurnActionsPerformed : IEvent {
    public List<Creature> creatures;

    public EndTurnActionsPerformed(List<Creature> creatures) {
        this.creatures = creatures;
    }
}