using System;
using System.Collections.Generic;

public abstract class Card : UnitModel {
    public Action<CardContainer> OnContainerChanged { get; internal set; }
    public CardContainer CurrentContainer { get; protected set; }
    public CardData Data { get; protected set; }
    public Cost Cost { get; protected set; }
    protected List<OperationData> _operationDatas;

    public Card(CardData cardData)  // Add owner to constructor
    {
        Data = cardData;
        Cost = new Cost(cardData.cost);
        Id = $"{Data.Name}_{Guid.NewGuid()}";
        _operationDatas = cardData.operationsData;
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


public class CreatureCard : Card, IHealthable, IAttacker {
    public CreatureCardData CreatureCardData { get; private set; }

    public Health Health { get; private set; }
    public Attack Attack { get; private set; }

    public CreatureCard(CreatureCardData cardData, Health health, Attack attack)
        : base(cardData) {

        CreatureCardData = cardData;
        Health = health;
        Attack = attack;
    }

    #region IAttacker
    public int CurrentAttack => Attack.Current;

    #endregion

    #region IHealthable
    public bool IsDead => Health.IsDead;

    public int CurrentHealth => Health.Current;

    public void TakeDamage(int damage) {
        Health.TakeDamage(damage);
    }
    #endregion

    public override string ToString() {
        return $"{CreatureCardData.Name} Hp: {Health}";
    }
}

public class SpellCard : Card {
    public SpellCard(SpellCardData cardData) : base(cardData) {
    }
}
