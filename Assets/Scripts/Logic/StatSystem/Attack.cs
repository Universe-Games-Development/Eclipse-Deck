using UnityEngine;

public class Attack : Attribute {
    private readonly IDamageable _owner;
    private readonly GameEventBus _eventBus;

    public Attack(int initialValue, IDamageable owner) : base(initialValue) {
        _owner = owner;
    }

    public void DealDamage(IDamageable target) {
        target.Health.TakeDamage(CurrentValue);
    }
}

