using System;
using System.Collections.Generic;
using Zenject;
using static UnityEngine.Rendering.HDROutputUtils;

public abstract class Card : UnitModel {
    public Action<CardContainer> OnContainerChanged { get; internal set; }
    public CardContainer CurrentContainer { get; protected set; }
    public CardData Data { get; protected set; }
    public Cost Cost { get; protected set; }
    public string Id { get; set; } // Unique identifier for the card
    private List<OperationData> _operationDatas;
    protected IOperationFactory _operationFactory;
    
    public Card(CardData cardData, IOperationFactory operationFactory)  // Add owner to constructor
    {
        Data = cardData;
        Cost = new Cost(cardData.cost);
        Id = $"{Data.Name}_{Guid.NewGuid()}";
        _operationDatas = new();
    }

    public virtual void SetContainer(CardContainer newContainer) {
        CurrentContainer = newContainer;
    }

    public void AddOperation(OperationData operation) {
        if (operation == null) {
            throw new ArgumentNullException(nameof(operation));
        }
        _operationDatas.Add(operation);
    }

    public bool RemoveOperation(OperationData operation) {
        return _operationDatas.Remove(operation);
    }

    public List<OperationData> GetOperationData() {
        return new(_operationDatas);
    }
}


public class CreatureCard : Card, IHealthable {
    public CreatureCardData CreatureCardData => (CreatureCardData)Data;

    public Health Health { get; private set; }
    public Attack Attack { get; private set; }

    public CreatureCard(CreatureCardData cardData, IOperationFactory operationFactory)
        : base(cardData, operationFactory) {
        
        Health = new(cardData.Health, this);
        Attack = new(cardData.Attack, this);
    }

    public override string ToString() {
        return $"{CreatureCardData.Name} Hp: {Health}";
    }
}

public class SpellCard : Card {
    public SpellCard(SpellCardData cardData, IOperationFactory operationFactory) : base(cardData, operationFactory) {
    }
}
