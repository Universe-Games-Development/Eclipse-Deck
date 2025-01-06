using System;
using System.Collections.Generic;
using UnityEngine;

public class Card {
    private const int MAX_CARD_HEALTH = 100;
    private const int MAX_CARD_ATTACK = 100;
    private const int MAX_CARD_COST = 30;
    public string Id { get; private set; }
    public string ResourseId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Opponent Owner { get; private set; }
    public Cost Cost { get; private set; }
    public Attack Attack { get; private set; }
    public Health Health { get; private set; }
    public CardState CurrentState { get; private set; }
    public List<string> AbilityDescriptions { get; private set; }
    public IEventManager EventManager { get; private set; }
    public Sprite MainImage { get; private set; }
    public Action<CardState> OnStateChanged { get; internal set; }

    public List<CardAbility> abilities;

    public Card(CardSO cardSO, Opponent owner, IEventManager eventManager) {
        Id = Guid.NewGuid().ToString();
        ResourseId = cardSO.id;

        EventManager = eventManager;
        Owner = owner;
        Name = cardSO.cardName;
        Description = cardSO.description;

        Cost = new(MAX_CARD_COST, cardSO.cost);
        Attack = new(MAX_CARD_ATTACK, cardSO.attack);
        Health = new(MAX_CARD_HEALTH, cardSO.health);
        MainImage = cardSO.mainImage;

        abilities = new List<CardAbility>();
        AbilityDescriptions = new List<string>();

        foreach (var abilitySO in cardSO.abilities) {
            var ability = new CardAbility(abilitySO, this, eventManager);
            abilities.Add(ability);
            AbilityDescriptions.Add(abilitySO.abilityDescription);
        }

        ChangeState(CardState.InDeck);
    }

    public void ChangeState(CardState newState) {
        if (CurrentState == newState) return;
        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }

    public void Reset() {
        ChangeState(CardState.InDeck);
        Cost.Reset();
        Attack.Reset();
        Health.Reset();
    }

    public void Exile() {
        Reset();
        foreach (var ability in abilities) {
            ability.Reset();
        }
    }
}
