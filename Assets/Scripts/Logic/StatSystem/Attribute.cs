using System;
using System.Collections.Generic;
using System.Linq;

public struct ModifierChangedEvent : IEvent {
    public int OldValue { get; }
    public int NewValue { get; }
    public int Difference => NewValue - OldValue;

    public ModifierChangedEvent(int oldValue, int newValue) {
        OldValue = oldValue;
        NewValue = newValue;
    }
}

public struct AttributeTotalChangedEvent : IEvent {
    public int OldValue { get; }
    public int NewValue { get; }
    public int Change { get; }

    public AttributeTotalChangedEvent(int oldValue, int newValue, int change) {
        OldValue = oldValue;
        NewValue = newValue;
        Change = change;
    }
}
public class AttributeModifier {
    private readonly int MinValue;
    private int _totalValue;
    private int _currentValue;

    public int TotalValue {
        get => _totalValue;
        private set {
            int oldValue = _totalValue;
            _totalValue = value;
            if (_currentValue > _totalValue) {
                _currentValue = _totalValue; // �������� CurrentValue ����� TotalValue
            }

            if (oldValue != _totalValue) {
                OnTotalValueChanged?.Invoke(this, new ModifierChangedEvent(oldValue, _totalValue));
            }
        }
    }

    public int CurrentValue {
        get => _currentValue;
        private set {
            int oldValue = _currentValue;
            // ������ ��������� �� ���������� �������� 0
            _currentValue = Math.Max(0, Math.Min(value, _totalValue)); // CurrentValue ���������� 0 � TotalValue
            if (oldValue != _currentValue) {
                OnCurrentValueChanged?.Invoke(this, new ModifierChangedEvent(oldValue, _currentValue));
            }
        }
    }

    public event EventHandler<ModifierChangedEvent> OnTotalValueChanged;
    public event EventHandler<ModifierChangedEvent> OnCurrentValueChanged;

    public AttributeModifier(int initialTotal = 0, int minValue = -999) {
        _totalValue = Math.Max(0, initialTotal);
        _currentValue = 0;
        MinValue = minValue;
    }

    // �������� ��������� ����������� � ������ ������� ��������
    public void Increase(int amount) {
        if (amount <= 0) return;

        // �������� �������� �������� ����
        int oldTotal = TotalValue;
        TotalValue += amount;
    }

    // �������� ��������� ����������� � ������� ��������
    public int Decrease(int amount) {
        if (amount <= 0) return 0;

        // �������� ������� ������� ��������
        int oldTotal = TotalValue;
        int subtracted = Subtract(amount);

        // ���� �������� �������� �������� ������������
        TotalValue -= amount;

        return subtracted;
    }

    // ����������� ������� ��������� ������������, ������� ������ �� ������� ������
    public int Subtract(int amount) {
        if (amount <= 0) return 0;

        int canSubtract = Math.Min(CurrentValue, amount);
        CurrentValue -= canSubtract;

        return amount - canSubtract; // ������� ������������� �������
    }

    // ������ �� ��������� ������������, ������� ������ �� ������� ������
    public int Add(int amount) {
        if (amount <= 0) return amount;

        int canAdd = Math.Min(TotalValue - CurrentValue, amount);
        CurrentValue += canAdd;

        return amount - canAdd; // ������� ������������� �������
    }

    // ������� �����������
    public void Reset() {
        TotalValue = 0;
        CurrentValue = 0;
    }

    // ���������� ������� ��������
    public void SetCurrentValue(int value) {
        CurrentValue = value;
    }

    public override string ToString() {
        return $"Total: {TotalValue}, Current: {CurrentValue}";
    }
}

public class Attribute {
    // ������ �������� ��������
    public int BaseValue { get; private set; }

    // �������� ����������� �������� (���� + ������������)
    public int TotalValue => BaseValue + AttributeModifier.TotalValue;

    // ������� �������� ��������
    private int _mainValue;
    public int MainValue {
        get => _mainValue;
        private set {
            int oldValue = _mainValue;
            // �����������, �� �������� �� ���� ����� MinValue
            _mainValue = Math.Max(MinValue, Math.Min(value, BaseValue));
            if (oldValue != _mainValue) {
                OnMainValueChanged?.Invoke(this, new ModifierChangedEvent(oldValue, _mainValue));
            }
        }
    }

    // ������� �������� �������� � ����������� ������������
    public int CurrentValue => MainValue + AttributeModifier.CurrentValue;

    // ̳������� �������� ��������
    public int MinValue { get; set; } = 0;

    // ����������� ��������
    public AttributeModifier AttributeModifier { get; private set; }

    // ��䳿
    public event EventHandler<ModifierChangedEvent> OnMainValueChanged;
    public event EventHandler<ModifierChangedEvent> OnBaseValueChanged;
    public event EventHandler<AttributeTotalChangedEvent> OnTotalValueChanged;

    public Attribute(int baseValue, int minValue = -999) {
        MinValue = minValue;
        BaseValue = Math.Max(minValue, baseValue);
        _mainValue = BaseValue; // ������������ �������, ��� �������� ������� ��䳿
        AttributeModifier = new AttributeModifier();

        // ϳ��������� �� ��䳿 ������������
        AttributeModifier.OnCurrentValueChanged += (s, e) =>
            OnTotalValueChanged?.Invoke(this, new AttributeTotalChangedEvent(
                e.OldValue + MainValue,
                e.NewValue + MainValue,
                e.Difference));

        AttributeModifier.OnTotalValueChanged += (s, e) =>
            OnTotalValueChanged?.Invoke(this, new AttributeTotalChangedEvent(
                BaseValue + e.OldValue,
                BaseValue + e.NewValue,
                e.Difference));
    }

    // �������� �������� �������� (��������� ����� ����)
    public int Subtract(int amount) {
        if (amount <= 0) return 0;

        int oldTotal = CurrentValue;
        int remainingValue = ApplyDamage(amount);

        int actualDecrease = amount - remainingValue;
        if (actualDecrease > 0) {
            OnTotalValueChanged?.Invoke(this, new AttributeTotalChangedEvent(
                oldTotal, CurrentValue, -actualDecrease));
        }

        return actualDecrease;
    }

    private int ApplyDamage(int amount) {
        // �������� ������� �� ������
        int remainingValue = AttributeModifier.Subtract(amount);

        // ������� ������� �� ��������� ��������
        if (remainingValue > 0) {
            int oldMainValue = MainValue;
            MainValue = Math.Max(MinValue, MainValue - remainingValue);
            int mainValueDecrease = oldMainValue - MainValue;
            remainingValue -= mainValueDecrease;
        }

        return remainingValue;
    }

    private int Restore(int restoreAmount) {
        int mainDifference = BaseValue - MainValue;
        int excess = restoreAmount;

        if (mainDifference > 0) {
            int canAdd = Math.Min(mainDifference, restoreAmount);
            MainValue += canAdd;
            excess = restoreAmount - canAdd;
        }
        return excess;
    }

    // ������ �������� �� �������� (�������� ����)
    public int Add(int amount) {
        if (amount <= 0) return 0;

        int oldTotal = CurrentValue;

        // �������� ���������� ������� �������� �� ���������
        int mainDifference = BaseValue - MainValue;


        int excess = Restore(amount);
        int addedToMain = amount - excess;

        // ������� ������ �� ������������
        int addedToModifier = 0;
        if (excess > 0) {
            int remainingAmount = AttributeModifier.Add(excess);
            addedToModifier = excess - remainingAmount;
        }

        int totalAdded = addedToMain + addedToModifier;
        if (totalAdded > 0) {
            OnTotalValueChanged?.Invoke(this, new AttributeTotalChangedEvent(
                oldTotal, CurrentValue, totalAdded));
        }

        return totalAdded;
    }

    public void AddModifier(int amount) {
        if (amount <= 0) {
            int decreaseAmount = -amount;
            AttributeModifier.Decrease(decreaseAmount);
            return;
        }

        // 1. �������� �������� ����������� �������� ������������
        AttributeModifier.Increase(amount);

        // 2. ³��������� ������� ������'� �� �������� (���� ���� �� �����)
        int excess = Restore(amount);

        // 3. ���� ���������� ������������� ��������, ������ ���� �� ��������� ������������
        if (excess > 0) {
            AttributeModifier.Add(excess);
        }
    }

    // �������� ����������� �������� ��������
    public void RemoveModifier(int amount) {
        if (amount <= 0) {
            int increaseAmount = -amount;
            AttributeModifier.Increase(increaseAmount);
            AttributeModifier.Add(increaseAmount);
            return;
        }

        // if effect was possitive
        if (CurrentValue > BaseValue) {
            int canDecrease = Math.Min(CurrentValue - BaseValue, amount);
            ApplyDamage(canDecrease);
            AttributeModifier.Decrease(canDecrease);
        }
    }

    // ���������� ���� ������ ��������
    public bool SetBaseValue(int newBaseValue) {
        if (newBaseValue < MinValue)
            return false;

        int oldValue = BaseValue;
        BaseValue = newBaseValue;

        // ���� ������� ������� �������� ����� �� ���� ������, �������� ����
        if (MainValue > BaseValue) {
            MainValue = BaseValue;
        }

        OnBaseValueChanged?.Invoke(this, new ModifierChangedEvent(oldValue, BaseValue));
        OnTotalValueChanged?.Invoke(this, new AttributeTotalChangedEvent(
            oldValue + AttributeModifier.TotalValue,
            BaseValue + AttributeModifier.TotalValue,
            BaseValue - oldValue));

        return true;
    }

    // ³������� ������� �������� �� ���������
    public void RestoreToBase() {
        MainValue = BaseValue;
    }

    // ������� �� ��������
    public void Reset() {
        RestoreToBase();
        AttributeModifier.Reset();
    }

    public override string ToString() {
        return $"Base: {BaseValue}, Main: {MainValue}, " +
               $"Modifier (Total/Current): {AttributeModifier.TotalValue}/{AttributeModifier.CurrentValue}, " +
               $"Total: {TotalValue}, Current: {CurrentValue}";
    }
}
