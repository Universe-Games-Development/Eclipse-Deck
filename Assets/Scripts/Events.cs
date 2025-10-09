
public struct OnDamageTaken : IEvent {
    public IAttacker Source { get; }
    public IHealthable Target { get; }
    public int Amount { get; }

    public OnDamageTaken(IHealthable target, IAttacker source, int amount) {
        Source = source;
        Target = target;
        Amount = amount;
    }
}

public struct DeathEvent : IEvent {
    public UnitModel DeadUnit { get; }

    public DeathEvent(UnitModel deadUnit) {
        DeadUnit = deadUnit;
    }
}