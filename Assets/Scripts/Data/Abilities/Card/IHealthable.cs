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
    public Action<BoardPlayer> OnChangedOwner;
    public Action<GameEnterEvent> OnUnitDeployed;
    private BoardPlayer _owner;
    public string Id { get; protected set; }
    public void ChangeOwner(BoardPlayer newOwner) {
        if (newOwner == _owner) return;
        _owner = newOwner;
        OnChangedOwner?.Invoke(newOwner);
    }

    public BoardPlayer GetPlayer() {
        return _owner;
    }
}