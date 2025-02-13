using System;

public class HealthStat : Stat {
    public HealthStat(int maxValue, int initialValue)
         : base(maxValue, initialValue) {
        if (maxValue < 1) throw new ArgumentException("Max value must be at least 1.");
    }

    
}

public class Attacktat : Stat {
    public Attacktat(int maxValue, int initialValue)
         : base(maxValue, initialValue) {
    }

    public void Buff(int amount) {
        int newValue = Math.Max(0, CurrentValue + amount);
        Modify(newValue);
    }

    public void Debuff(int amount) {
        int newValue = Math.Max(0, CurrentValue - amount);
        Modify(newValue);
    }
}


public class Health : Stat {
    public IHasHealth Owner { get; }
    public event Action OnDeath;
    public event Action<int, IDamageDealer> OnDamageTaken;
    private readonly GameEventBus _eventBus;

    public Health(IHasHealth owner, int maxHealth, int initialHealth, GameEventBus eventBus)
        : base(maxHealth, initialHealth) {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public void ApplyDamage(int damage, IDamageDealer? damageSource = null) {
        if (damage <= 0) return;

        var previousHealth = CurrentValue;
        Modify(-damage);

        if (damageSource is IDamageDealer damageDealer) {
            _eventBus.Raise(new OnDamageTaken(damageDealer, Owner, damage));
            OnDamageTaken?.Invoke(damage, damageDealer);
        }


        if (CurrentValue <= 0 && previousHealth > 0) {
            Die();
        } else {
            Console.WriteLine($"Took {damage} damage. Current health: {CurrentValue}");
        }
    }

    private void Die() {
        OnDeath?.Invoke();
        _eventBus.Raise(new OnDeathEvent(Owner, this));
        Console.WriteLine("Character has died.");
    }

    internal void Heal(int healAmount) {
        throw new NotImplementedException();
    }

    public bool IsAlive() {
        return CurrentValue > 0;
    }

    internal bool IsDamaged() {
        return CurrentValue < InitialValue;
    }
}

public struct OnDamageTaken : IEvent {
    public IDamageDealer Source { get; }
    public IHasHealth Target { get; }
    public int Amount { get; }

    public OnDamageTaken(IDamageDealer source, IHasHealth target, int amount) {
        Source = source;
        Target = target;
        Amount = amount;
    }
}

public struct OnDeathEvent : IEvent {
    public IHasHealth DeadEntity { get; }
    public Health HealthInfo { get; }

    public OnDeathEvent(IHasHealth deadEntity, Health healthInfo) {
        DeadEntity = deadEntity;
        HealthInfo = healthInfo;
    }
}
