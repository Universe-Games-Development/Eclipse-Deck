using System;
using System.Collections.Generic;

public abstract class Card : GameUnit {
    public Action<CardContainer> OnContainerChanged { get; internal set; }
    public CardContainer CurrentContainer { get; protected set; }
    public CardData Data { get; protected set; }
    public Cost Cost { get; protected set; }
    public string Id { get; set; } // Unique identifier for the card
    public List<GameOperation> Operations = new List<GameOperation>();

    public Card(CardData cardData)  // Add owner to constructor
    {
        Data = cardData;
        Cost = new Cost(cardData.MAX_CARDS_COST, cardData.cost);
        Id = $"{Data.Name}_{Guid.NewGuid()}";
    }

    public virtual void SetContainer(CardContainer newContainer) {
        CurrentContainer = newContainer;
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
        Operations.Add(new SpawnCreatureOperation(CreatureCardData));
    }

    public override string ToString() {
        return $"{CreatureCardData.Name} Hp: {Health}";
    }
}

