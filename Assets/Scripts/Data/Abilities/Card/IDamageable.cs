using System;

public interface IMannable {
    public Mana Mana { get; }
}
public interface IDamageable {
    public Health Health { get; }
}

public interface IDamageDealer {
    public Attack Attack { get; }
}

public interface IGameUnit {
    event Action<GameEnterEvent> OnUnitDeployed;
    BoardPlayer ControlledBy { get; }
}