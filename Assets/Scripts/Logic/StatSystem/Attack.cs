public class Attack : Attribute {
    private readonly IEventBus<IEvent> _eventBus;

    public Attack(Attribute attribute) : base(attribute) {
    }

    public Attack(int baseValue, int minValue = -999) : base(baseValue, minValue) {
    }

    public void DealDamage(IHealthable target) {
        target.Health.TakeDamage(Current);
    }
}

