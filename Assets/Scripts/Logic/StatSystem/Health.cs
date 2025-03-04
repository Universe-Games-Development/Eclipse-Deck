using System;
using static Unity.VisualScripting.Member;

// TO DO: Add regen stat
public interface IHealth {
    int Current { get; }
    int Max { get; }
    void TakeDamage(int amount, IDamageDealer source = null);
    void Heal(int amount);
    event Action OnDeath;
}

public class Health : IHealth {
    public Stat Stat { get; private set; }
    private readonly IHealthEntity _owner;
    private readonly GameEventBus _eventBus;

    public event Action OnDeath;
    public int Current => Stat.CurrentValue;
    public int Max => Stat.MaxValue;

    public Health(IHealthEntity owner, Stat stat, GameEventBus eventBus) {
        _owner = owner;
        Stat = stat;
        _eventBus = eventBus;

        Stat.OnValueChanged += HandleStatChange;
    }

    public Action<int, IDamageDealer> OnDamageTaken { get; internal set; }
    public Action<int, int> OnChangedMaxValue { get; internal set; }
    public bool isDead = false;

    public void TakeDamage(int amount, IDamageDealer source = null) {
        if (amount <= 0 || isDead) return;

        var damage = Math.Min(amount, Stat.CurrentValue);
        Stat.Modify(-damage);

        _eventBus.Raise(new OnDamageTaken(_owner, source, damage));

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
        return Current > 0 || isDead;
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
