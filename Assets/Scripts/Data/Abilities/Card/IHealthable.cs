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
    public BoardPlayer Owner { get; private set; }

    public void ChangeOwner(BoardPlayer newOwner) {
        if (newOwner == Owner) return;
        Owner = newOwner;
        OnChangedOwner?.Invoke(newOwner);
    }
}