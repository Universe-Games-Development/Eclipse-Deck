
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


public readonly struct HoverUnitEvent : IEvent {
    public UnitPresenter UnitPresenter { get; }
    public bool IsHovered { get; }

    public HoverUnitEvent(UnitPresenter unitPresenter, bool isHovered) {
        UnitPresenter = unitPresenter;
        IsHovered = isHovered;
    }
}

public readonly struct ClickUnitEvent : IEvent {
    public UnitPresenter UnitPresenter { get; }

    public ClickUnitEvent(UnitPresenter unitPresenter) {
        UnitPresenter = unitPresenter;
    }
}