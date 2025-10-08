using System;

public interface IManaSystem {
    public Mana Mana { get; }
}
public interface IHealthable {
    public Health Health { get; }
}

public interface IDamageDealer {
    public Attack Attack { get; }
}
