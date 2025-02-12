using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card : IAbilitySource {
    public string Id { get; protected set; }
    public CardSO Data { get; protected set; }
    public Cost Cost { get; protected set; }
    public CardState CurrentState { get; protected set; }
    public GameEventBus eventBus { get; protected set; }
    public Action<CardState> OnStateChanged { get; internal set; }
    public Opponent Owner { get; protected set; } // Add Owner here

    public List<CardAbility> cardAbilities;
    public CardUI cardUI;

    public Card(CardSO cardSO, Opponent owner, GameEventBus eventBus)  // Add owner to constructor
    {
        Data = cardSO;
        Id = Guid.NewGuid().ToString();
        this.eventBus = eventBus;
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

    internal IInputCommand GetPlayCardCommand() {
        throw new NotImplementedException();
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
        // Додати логику ефекту заклинання
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