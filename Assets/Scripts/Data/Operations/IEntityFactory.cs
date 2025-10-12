using System;
using Zenject;

public interface IEntityFactory {
    T Create<T>(params object[] args) where T : UnitModel;
    Creature CreateCreatureFromCard(CreatureCard card);
}

public class EntityFactory : IEntityFactory {
    private readonly DiContainer _container;

    public EntityFactory(DiContainer container) {
        _container = container;
    }

    public T Create<T>(params object[] args) where T : UnitModel {
        T unit = _container.Instantiate<T>(args);
        unit.InstanceId = $"{typeof(T)}_{Guid.NewGuid()}";
        return unit;
    }

    public Creature CreateCreatureFromCard(CreatureCard card) {
        Health creatureHealth = new(card.Health);
        Attack creatureAttack = new(card.Attack);
        Cost creatureCost = new(card.Cost);

        return Create<Creature>(card.Data, creatureHealth, creatureAttack, creatureCost, card.InstanceId); ;
    }
}