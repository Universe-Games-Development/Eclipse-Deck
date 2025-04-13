using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public abstract class Card : IAbilityOwner {
    public event Action<Card> OnCardDrawn;
    public event Action<Card> OnCardShuffled;
    public event Action<Card> OnCardRDiscarded;
    public Action<CardState> OnStateChanged { get; internal set; }
    public CardState CurrentState { get; protected set; }
    public CardData Data { get; protected set; }
    public Cost Cost { get; protected set; }

    public CardView cardUI;
    public AbilityManager<CardAbilityData, Card> _abilityManager;

    [Inject]
    public void Construct(DiContainer diContainer) {
        _abilityManager = new AbilityManager<CardAbilityData, Card>(this, diContainer);
        _abilityManager.AddAbilities(Data.abilities);
    }

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
                    OnCardRDiscarded?.Invoke(this);
                    break;
                default:
                    throw new ArgumentException("Wrong new state");
            }
        }
    }
    internal void Deselect() {
        Debug.LogError("Deselect");
    }

    internal void Select() {
        Debug.LogError("Select");
    }

    internal List<GameOperation> GetCardPlayOperations() {
        throw new NotImplementedException();
    }
}

public struct DiscardCardEvent : IEvent {
    public Card card;
    public DiscardCardEvent(Card card, Opponent owner) {
        this.card = card;
    }
}

public struct ExileCardEvent : IEvent {
    public Card card;
    public ExileCardEvent(Card card) {
        this.card = card;
    }
}

public class CreatureCard : Card {
    public Stat Health { get; private set; }
    public Stat Attack { get; private set; }
    public CreatureCardData creatureCardData;

    public CreatureCard(CreatureCardData cardData)
        : base(cardData) {
        creatureCardData = cardData;
        Health = new(cardData.Health);
        Attack = new(cardData.Attack);
    }
}
