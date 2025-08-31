using System;

public class CreaturePresenter : UnitPresenter, IHealthable {
    public Creature Model;

    public Health Health => throw new NotImplementedException();

    public override UnitModel GetInfo() {
        return Model;
    }
    public override BoardPlayer GetPlayer() {
        return Model.Owner;
    }
}

public class Creature : UnitModel {

}
