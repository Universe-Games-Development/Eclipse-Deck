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
        unit.Id = $"{typeof(T)}_{Guid.NewGuid()}";
        return unit;
    }

    public Creature CreateCreatureFromCard(CreatureCard card) {
        return Create<Creature>(card); ;
    }
}