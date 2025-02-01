using System;
using System.Collections.Generic;

public class Card {
    private const int MAX_CARD_HEALTH = 100;
    private const int MAX_CARD_ATTACK = 100;
    private const int MAX_CARD_COST = 30;
    public string Id;
    public string ResourseId { get; private set; }
    public Opponent Owner { get; private set; }
    public Cost Cost { get; private set; }
    public Attack Attack { get; private set; }
    public Health Health { get; private set; }
    public CardState CurrentState { get; private set; }
    public IEventQueue eventQueue { get; private set; }
    public Action<CardState> OnStateChanged { get; internal set; }

    public List<CardAbility> abilities;

    public CardSO data;

    public Card(CardSO cardSO, Opponent owner, IEventQueue eventQueue) {
        data = cardSO;

        Id = Guid.NewGuid().ToString(); // generate own uni id
        ResourseId = cardSO.resourseId;
        this.eventQueue = eventQueue;
        Owner = owner;

        Cost = new(MAX_CARD_COST, cardSO.cost);
        Attack = new(MAX_CARD_ATTACK, cardSO.attack);
        Health = new(MAX_CARD_HEALTH, cardSO.health);

        abilities = new List<CardAbility>();
        foreach (var abilitySO in cardSO.abilities) {
            var ability = new CardAbility(abilitySO, this, eventQueue);
            abilities.Add(ability);
        }


        ChangeState(CardState.InDeck);
    }

    public void ChangeState(CardState newState) {
        if (CurrentState == newState) return;
        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }

    public void RemoveRandomAbility() {
        if (abilities == null || abilities.Count == 0) {
            return;
        }

        // Вибір випадкової здібності
        int randomIndex = UnityEngine.Random.Range(0, abilities.Count);
        var abilityToRemove = abilities[randomIndex];

        // Виклик Reset для здібності, якщо потрібно очистити стан
        abilityToRemove.Reset();

        // Видалення здібності зі списку
        abilities.RemoveAt(randomIndex);
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
