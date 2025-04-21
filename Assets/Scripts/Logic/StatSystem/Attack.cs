using UnityEngine;

public class Attack : Attribute {
    public GameEventBus eventBus;
    private readonly IDamageable _owner;
    private readonly GameEventBus _eventBus;

    public Attack(int initialValue, IDamageable owner, GameEventBus eventBus) : base(initialValue) {
        _owner = owner;
        _eventBus = eventBus;
    }

    public void DealDamage(IDamageable target) {
        target.Health.TakeDamage(CurrentValue);
    }
}

