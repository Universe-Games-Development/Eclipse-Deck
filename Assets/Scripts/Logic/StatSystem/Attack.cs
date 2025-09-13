using UnityEngine;

public class Attack : Attribute {
    private readonly IHealthable _owner;
    private readonly GameEventBus _eventBus;

    public Attack(int initialValue, IHealthable owner) : base(initialValue) {
        _owner = owner;
    }

    public void DealDamage(IHealthable target) {
        target.Health.TakeDamage(Current);
    }
}

