using System;
using System.Collections.Generic;

public abstract class Card : IGameUnit {
    public event Action<Card> OnCardDrawn;
    public event Action<Card> OnCardShuffled;
    public event Action<Card> OnCardDiscarded;
    public Action<CardState> OnStateChanged { get; internal set; }
    public CardState CurrentState { get; protected set; }
    public CardData Data { get; protected set; }
    public Cost Cost { get; protected set; }
    public event Action<GameEnterEvent> OnUnitDeployed;
    public BoardPlayer ControlledBy { get; set; }
    public string Id { get; set; } // Unique identifier for the card

    public Card(CardData cardData)  // Add owner to constructor
    {
        Data = cardData;
        Cost = new Cost(cardData.MAX_CARDS_COST, cardData.cost);
        Id = $"{Data.Name}_{Guid.NewGuid()}";
    }

    public virtual void ChangeState(CardState newState) {
        if (newState != CurrentState) {
            CurrentState = newState;
            switch (newState) {
                case CardState.InDeck:
                    OnCardShuffled?.Invoke(this);
                    break;
                case CardState.InHand:
                    OnCardDrawn?.Invoke(this);
                    break;
                case CardState.Discarded:
                    OnCardDiscarded?.Invoke(this);
                    break;
                default:
                    throw new ArgumentException("Wrong new state");
            }
        }
    }

    internal List<GameOperation> GetCardPlayOperations() {
        throw new NotImplementedException();
    }

    // used by deck
    internal void Deploy() {
        OnUnitDeployed?.Invoke(new GameEnterEvent(this));
    }
}


public class CreatureCard : Card, IDamageable {
    public CreatureCardData CreatureCardData => (CreatureCardData)Data;

    public Health Health { get; private set; }
    public Attack Attack { get; private set; }

    public CreatureCard(CreatureCardData cardData)
        : base(cardData) {
        
        Health = new(cardData.Health, this);
        Attack = new(cardData.Attack, this);
    }

    public override string ToString() {
        return $"{CreatureCardData.Name} Hp: {Health}";
    }
}

