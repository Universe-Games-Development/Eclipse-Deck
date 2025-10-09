using System;

// TO DO: Add regen stat
public class Health : Attribute {
    public Action OnDeath;
    public Action OnHealthChanged;

    public bool IsDead = false;

    public Health(int baseValue, int minValue = -999) : base(baseValue, minValue) {
    }

    public Health(Attribute attribute) : base(attribute) {
    }

    public void TakeDamage(int damage, IAttacker source = null) {
        if (IsDead) return;

        Subtract(damage);

        if (Current <= 0 && !IsDead) {
            IsDead = true;
            OnDeath?.Invoke();
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

