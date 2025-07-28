using System;

// TO DO: Add regen stat
public class Health : Attribute {
    public event Action<OnDamageTaken> OnDamageTaken;
    public event Action<DeathEvent> OnDeath;
    
    public bool IsDead = false;
    private readonly IDamageable _owner;

    public Health(int initialValue, IDamageable owner) : base(initialValue) {
        _owner = owner;
    }

    public void TakeDamage(int damage, IDamageDealer source = null) {
        if (IsDead) return;

        Subtract(damage); // ������ � Subtract(-amount) �� Subtract(amount)
        OnDamageTaken?.Invoke(new OnDamageTaken(_owner, source, damage));

        if (CurrentValue <= 0 && !IsDead) {
            IsDead = true;
            OnDeath?.Invoke(new DeathEvent(_owner));
        }
    }

    public void Heal(int amount, out int excess) {
        excess = 0;
        if (IsDead || amount <= 0) return;

        // ³��������� ������'� ����� �� �������� ��������
        int mainDifference = BaseValue - MainValue;

        if (mainDifference > 0) {
            // �������� �������� ������� ���������
            int healAmount = Math.Min(mainDifference, amount);
            excess = Add(healAmount);
        }
        // �� ������ �����, ��������� �� ����, ������ �������� ����������
    }

    public void Resurrect(int healthAmount) {
        if (!IsDead) return;

        IsDead = false;
        Heal(healthAmount, out int excess);
    }

    public bool IsAlive() {
        return !IsDead;
    }
}

public struct OnDamageTaken : IEvent {
    public IDamageDealer Source { get; }
    public IDamageable Target { get; }
    public int Amount { get; }

    public OnDamageTaken(IDamageable target, IDamageDealer source, int amount) {
        Source = source;
        Target = target;
        Amount = amount;
    }
}

public struct DeathEvent : IEvent {
    public IDamageable DeadEntity { get; }

    public DeathEvent(IDamageable deadEntity) {
        DeadEntity = deadEntity;
    }
}
