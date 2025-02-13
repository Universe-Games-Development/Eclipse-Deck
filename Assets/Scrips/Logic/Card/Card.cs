using System;
using UnityEngine;

public abstract class Card : IAbilityOwner {
    public event Action<Card> OnCardDrawn;
    public event Action<Card> OnCardShuffled;
    public event Action<Card> OnCardRDiscarded;
    public Action<CardState> OnStateChanged { get; internal set; }
    public CardState CurrentState { get; protected set; }

    public string Id { get; protected set; }
    public CardSO Data { get; protected set; }
    public Cost Cost { get; protected set; }
    public GameEventBus eventBus { get; protected set; }
    public AbilityManager AbilityManager { get; protected set; }
    public Opponent Owner { get; protected set; } // Add Owner here

    public CardUI cardUI;
    public AbilityManager abilityManager;
    public Card(CardSO cardSO, Opponent owner, GameEventBus eventBus)  // Add owner to constructor
    {
        Data = cardSO;
        Id = Guid.NewGuid().ToString();
        this.eventBus = eventBus;
        Owner = owner; // Assign the owner
        Cost = new Cost(cardSO.MAX_CARDS_COST, cardSO.cost);

        abilityManager = new AbilityManager();
        abilityManager.InitializeAbilities(this, cardSO.abilities);
        

        ChangeState(CardState.InDeck);
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

    // To do: Define how card will play will it be command or simple method
    public abstract void Play();

    public AbilityManager GetAbilityManager() {
        return AbilityManager;
    }

    public Command GetPlayCardCommand(Field field) {
        return new PlayCardCommand(Owner, this, field);
    }
}

public class CreatureCard : Card {
    public Stat Attack { get; private set; }
    public Stat HealthStat { get; private set; }

    public CreatureCard(CreatureCardSO cardSO, Opponent owner, GameEventBus eventBus)
        : base(cardSO, owner, eventBus) {
        Attack = new(cardSO.MAX_CARD_ATTACK, cardSO.Attack);
        HealthStat = new(cardSO.MAX_CARD_HEALTH, cardSO.Health);
    }

    public override void Play() {
        Debug.Log($"Creature is played on the board!");
    }
}

public class SpellCard : Card {
    public SpellCard(CardSO cardSO, Opponent owner, GameEventBus eventBus)
        : base(cardSO, owner, eventBus) { }

    public override void Play() {
        Debug.Log($"Spell is cast!");
    }
}

public class SupportCard : Card {
    public SupportCard(CardSO cardSO, Opponent owner, GameEventBus eventBus)
        : base(cardSO, owner, eventBus) { }

    public override void Play() {
        Debug.Log($"Support is applied for the entire battle!");
        // Додати логику підтримки
    }
}