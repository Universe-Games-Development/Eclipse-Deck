using System;
using System.Collections.Generic;

public abstract class Card : UnitModel {
    public Action<CardContainer> OnContainerChanged { get; internal set; }
    public CardContainer CurrentContainer { get; protected set; }
    public CardData Data { get; protected set; }
    public Cost Cost { get; protected set; }
    public string Id { get; set; } // Unique identifier for the card
    public List<GameOperation> Operations { get; private set; }

    public Card(CardData cardData)  // Add owner to constructor
    {
        Data = cardData;
        Cost = new Cost(cardData.cost);
        Id = $"{Data.Name}_{Guid.NewGuid()}";
        Operations = new List<GameOperation>();
    }

    public virtual void SetContainer(CardContainer newContainer) {
        CurrentContainer = newContainer;
    }

    public void AddOperation(GameOperation operation) {
        if (operation == null) {
            throw new ArgumentNullException(nameof(operation));
        }
        Operations.Add(operation);
    }

    public bool RemoveOperation(GameOperation operation) {
        return Operations.Remove(operation);
    }
}


public class CreatureCard : Card, IHealthable {
    public CreatureCardData CreatureCardData => (CreatureCardData)Data;

    public Health Health { get; private set; }
    public Attack Attack { get; private set; }

    public CreatureCard(CreatureCardData cardData)
        : base(cardData) {
        
        Health = new(cardData.Health, this);
        Attack = new(cardData.Attack, this);
        Operations.Add(new SpawnCreatureOperation(this));
    }

    public override string ToString() {
        return $"{CreatureCardData.Name} Hp: {Health}";
    }
}

public class SpellCard : Card {
    public SpellCard(SpellCardData cardData) : base(cardData) {
    }
}
