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
    private readonly Stat _stat;
    private readonly IHealthEntity _owner;
    private readonly GameEventBus _eventBus;

    public event Action OnDeath;
    public int Current => _stat.CurrentValue;
    public int Max => _stat.MaxValue;

    public Health(IHealthEntity owner, Stat stat, GameEventBus eventBus) {
        _owner = owner;
        _stat = stat;
        _eventBus = eventBus;

        _stat.OnValueChanged += HandleStatChange;
    }

    public Action<int, IDamageDealer> OnDamageTaken { get; internal set; }

    public void TakeDamage(int amount, IDamageDealer source = null) {
        if (amount <= 0) return;

        var damage = Math.Min(amount, _stat.CurrentValue);
        _stat.Modify(-damage);

        _eventBus.Raise(new OnDamageTaken(_owner, source, damage));

        if (_stat.CurrentValue <= 0) {
            OnDeath?.Invoke();
            _eventBus.Raise(new DeathEvent(_owner));
        }
    }

    public void Heal(int amount) {
        if (amount <= 0) return;
        _stat.Modify(amount);
    }

    private void HandleStatChange(int oldValue, int newValue) {
        // Додаткова логіка при зміні здоров'я
    }

    public bool IsAlive() {
        return Current > 0;
    }

    internal void SetMaxValue(int healthIncrease) {
        _stat.SetMaxValue(healthIncrease);
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
