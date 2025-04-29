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
    public EffectManager Effects { get; set; }

    public BoardPlayer ControlledBy { get; set; }

    public CardView cardUI;

    public Card(CardData cardData)  // Add owner to constructor
    {
        Data = cardData;
        Cost = new Cost(cardData.MAX_CARDS_COST, cardData.cost);
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

public interface IStatable {
    CreatureStats Stats { get; }
}


public class CreatureCard : Card, IStatable, IDamageable {
    public CreatureStats Stats { get; }
    public CreatureCardData CreatureCardData => (CreatureCardData)Data;

    public Health Health => Stats.Health;

    public CreatureCard(CreatureCardData cardData, GameEventBus gameEventBus, TurnManager turnManager)
        : base(cardData) {
        
        Health health = new(cardData.Health, this, gameEventBus);
        Attack attack = new(cardData.Attack, this, gameEventBus);
        EffectManager effectManager = new(turnManager);

        Stats = new CreatureStats(attack, health, effectManager);
    }

    public override string ToString() {
        return $"{CreatureCardData.Name} {Stats}";
    }
}

