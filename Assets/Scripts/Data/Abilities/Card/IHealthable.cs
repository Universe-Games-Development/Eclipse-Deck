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
