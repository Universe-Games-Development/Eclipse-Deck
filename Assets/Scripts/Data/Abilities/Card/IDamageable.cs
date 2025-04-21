using System;

public interface IDamageable {
    public Health Health { get; }
}

public interface IDamageDealer {
    public Attack Attack { get; }
}

public interface IGameUnit {
    event Action<SummonEvent> OnUnitDeployed;
    Opponent ControlOpponent { get; }
}