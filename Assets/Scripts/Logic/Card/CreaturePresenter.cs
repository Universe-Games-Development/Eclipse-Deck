using System;
using Zenject;

public class CreaturePresenter : UnitPresenter, IHealthable {
    public Creature Creature;
    public CreatureView View;

    public Health Health => throw new NotImplementedException();

    public void Initialize(Creature creature, CreatureView view) {
        Creature = creature;
        View = view;
    }

    public override UnitModel GetModel() {
        return Creature;
    }
    public override BoardPlayer GetPlayer() {
        return Creature.GetPlayer();
    }

    public void Reset() {
        // Do reset logic here
    }
}