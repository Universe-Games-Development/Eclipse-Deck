using System;

public interface IHealthEntity {
    public IHealth Health { get; }
}

public interface IDamageDealer {
    public Attack Attack { get; }
}

public interface IAbilityOwner {
}