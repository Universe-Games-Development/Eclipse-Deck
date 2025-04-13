using System;

// TO DO: Add regen stat
public interface IHealth {
    event Action<int, IDamageDealer> OnDamageTaken;
    int Current { get; }
    int Max { get; }
    Stat Stat { get;}
    
    void TakeDamage(int amount, IDamageDealer source = null);
    void Heal(int amount);
    bool IsAlive();
    event Action OnDeath;
}

public class Health : IHealth {
    public event Action<int, IDamageDealer> OnDamageTaken;
    public event Action OnDeath;
    public Stat Stat { get; private set; }
    
    public bool isDead = false;
    public int Current => Stat.CurrentValue;
    public int Max => Stat.MaxValue;
    private readonly IHealthEntity _owner;
    private readonly GameEventBus _eventBus;

    public Health(IHealthEntity owner, Stat stat, GameEventBus eventBus) {
        _owner = owner;
        Stat = stat;
        _eventBus = eventBus;

        Stat.OnValueChanged += HandleStatChange;
    }

    
    

    public void TakeDamage(int amount, IDamageDealer source = null) {
        if (amount <= 0 || isDead) return;

        var damage = Math.Min(amount, Stat.CurrentValue);
        Stat.Modify(-damage);

        _eventBus.Raise(new OnDamageTaken(_owner, source, damage));
        OnDamageTaken?.Invoke(damage, source);

        if (Stat.CurrentValue <= 0) {
            isDead = true;
            OnDeath?.Invoke();
            _eventBus.Raise(new DeathEvent(_owner));
        }
    }

    public void Heal(int amount) {
        if (amount <= 0) return;
        Stat.Modify(amount);
    }

    private void HandleStatChange(int oldValue, int newValue) {
        // Додаткова логіка при зміні здоров'я
    }

    public bool IsAlive() {
        return !isDead;
    }

    internal void SetMaxValue(int healthIncrease) {
        Stat.SetMaxValue(healthIncrease);
    }
}

public struct OnDamageTaken : IEvent {
    public IDamageDealer Source { get; }
    public IHealthEntity Target { get; }
    public int Amount { get; }

    public OnDamageTaken(IHealthEntity target, IDamageDealer source, int amount) {
        Source = source;
        Target = target;
        Amount = amount;
    }
}

public struct DeathEvent : IEvent {
    public IHealthEntity DeadEntity { get; }

    public DeathEvent(IHealthEntity deadEntity) {
        DeadEntity = deadEntity;
    }
}
