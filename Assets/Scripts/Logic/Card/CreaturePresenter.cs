using System;
using Zenject;

public class CreaturePresenter : UnitPresenter, IHealthable {
    public Creature Creature;
    public CreatureView View;

    public Health Health => throw new NotImplementedException();

    public override UnitModel GetInfo() {
        return Creature;
    }
    public override BoardPlayer GetPlayer() {
        return Creature.Owner;
    }

    [Inject]
    public void Construct(IUnitRegistry unitRegistry) {
        _unitRegistry ??= unitRegistry;
        _unitRegistry.Register(this);
    }

    public void Initialize(Creature creature, CreatureView view) {
        Creature = creature;
        View = view;
    }
}