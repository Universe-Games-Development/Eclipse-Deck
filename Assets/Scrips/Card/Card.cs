using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card {
    public string Id { get; protected set; }
    public CardSO Data { get; protected set; }
    public Cost Cost { get; protected set; }
    public CardState CurrentState { get; protected set; }
    public IEventQueue eventQueue { get; protected set; }
    public Action<CardState> OnStateChanged { get; internal set; }
    public Opponent Owner { get; protected set; } // Add Owner here

    public List<CardAbility> cardAbilities;

    public Card(CardSO cardSO, Opponent owner, IEventQueue eventQueue)  // Add owner to constructor
    {
        Data = cardSO;
        Id = Guid.NewGuid().ToString();
        this.eventQueue = eventQueue;
        Owner = owner; // Assign the owner
        Cost = new Cost(cardSO.MAX_CARDS_COST, cardSO.cost);

        cardAbilities = new List<CardAbility>();
        foreach (var cardAbilityData in cardSO.cardAbilities) {
            // ... (ability creation remains the same)
        }

        ChangeState(CardState.InDeck);
    }

    public virtual void ChangeState(CardState newState) {
        // ... (same as before)
    }

    public abstract void Play();

    // ... (Other methods like RemoveRandomAbility, Reset, Exile)
}

public class CreatureCard : Card {
    public Attack Attack { get; private set; }
    public Health Health { get; private set; }

    public CreatureCard(CreatureCardSO cardSO, Opponent owner, IEventQueue eventQueue)
        : base(cardSO, owner, eventQueue) {
        Attack = new(cardSO.MAX_CARD_ATTACK, cardSO.Attack);
        Health = new(cardSO.MAX_CARD_HEALTH, cardSO.Health);
    }

    public override void Play() {
        Debug.Log($"Creature is played on the board!");
    }
}

public class SpellCard : Card {
    public SpellCard(CardSO cardSO, Opponent owner, IEventQueue eventQueue)
        : base(cardSO, owner, eventQueue) { }

    public override void Play() {
        Debug.Log($"Spell is cast!");
        // Додати логику ефекту заклинання
    }
}

public class SupportCard : Card {
    public SupportCard(CardSO cardSO, Opponent owner, IEventQueue eventQueue)
        : base(cardSO, owner, eventQueue) { }

    public override void Play() {
        Debug.Log($"Support is applied for the entire battle!");
        // Додати логику підтримки
    }
}