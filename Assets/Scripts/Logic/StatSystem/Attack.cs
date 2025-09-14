using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Attack : Attribute {
    private readonly IHealthable _owner;
    private readonly IEventBus<IEvent> _eventBus;

    public Attack(int initialValue, IHealthable owner) : base(initialValue) {
        _owner = owner;
    }

    public Attack(Attack attack, IHealthable owner) : base(attack) {
        _owner = owner;
    }

    public void DealDamage(IHealthable target) {
        target.Health.TakeDamage(Current);
    }
}

