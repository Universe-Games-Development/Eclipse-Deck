using System;

public interface IMannable {
    public Mana Mana { get; }
}
public interface IHealthable {
    public Health Health { get; }
}

public interface IDamageDealer {
    public Attack Attack { get; }
}

public class UnitModel {
    public Action<Opponent> OnChangedOwner;
    public Action<GameEnterEvent> OnUnitDeployed;
    private Opponent _owner;
    public string Id { get; protected set; }
    public void ChangeOwner(Opponent newOwner) {
        if (newOwner == _owner) return;
        _owner = newOwner;
        OnChangedOwner?.Invoke(newOwner);
    }

    public Opponent GetPlayer() {
        return _owner;
    }

    public virtual string GetName() {
        return "";
    }
}