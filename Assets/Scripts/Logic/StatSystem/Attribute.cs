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
                _currentValue = _totalValue; // Обмежуємо CurrentValue новим TotalValue
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
            // Додаємо обмеження по мінімальному значенню 0
            _currentValue = Math.Max(0, Math.Min(value, _totalValue)); // CurrentValue обмежується 0 і TotalValue
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

    // Збільшити загальний модифікатор і додати поточне значення
    public void Increase(int amount) {
        if (amount <= 0) return;

        // Спочатку збільшуємо загальну суму
        int oldTotal = TotalValue;
        TotalValue += amount;
    }

    // Зменшити загальний модифікатор і поточне значення
    public int Decrease(int amount) {
        if (amount <= 0) return 0;

        // Спочатку віднімаємо поточне значення
        int oldTotal = TotalValue;
        int subtracted = Subtract(amount);

        // Потім зменшуємо загальне значення модифікатора
        TotalValue -= amount;

        return subtracted;
    }

    // Використати частину поточного модифікатора, повертає скільки не вдалося відняти
    public int Subtract(int amount) {
        if (amount <= 0) return 0;

        int canSubtract = Math.Min(CurrentValue, amount);
        CurrentValue -= canSubtract;

        return amount - canSubtract; // Повертає невикористану частину
    }

    // Додати до поточного модифікатора, повертає скільки не вдалося додати
    public int Add(int amount) {
        if (amount <= 0) return amount;

        int canAdd = Math.Min(TotalValue - CurrentValue, amount);
        CurrentValue += canAdd;

        return amount - canAdd; // Повертає невикористану частину
    }

    // Скинути модифікатор
    public void Reset() {
        TotalValue = 0;
        CurrentValue = 0;
    }

    // Встановити поточне значення
    public void SetCurrentValue(int value) {
        CurrentValue = value;
    }

    public override string ToString() {
        return $"Total: {TotalValue}, Current: {CurrentValue}";
    }
}

public class Attribute {
    // Базове значення атрибуту
    public int BaseValue { get; private set; }

    // Загальне максимальне значення (база + модифікатори)
    public int TotalValue => BaseValue + AttributeModifier.TotalValue;

    // Основне значення атрибуту
    private int _mainValue;
    public int MainValue {
        get => _mainValue;
        private set {
            int oldValue = _mainValue;
            // Забезпечуємо, що значення не падає нижче MinValue
            _mainValue = Math.Max(MinValue, Math.Min(value, BaseValue));
            if (oldValue != _mainValue) {
                OnMainValueChanged?.Invoke(this, new ModifierChangedEvent(oldValue, _mainValue));
            }
        }
    }

    // Поточне значення атрибуту з урахуванням модифікаторів
    public int CurrentValue => MainValue + AttributeModifier.CurrentValue;

    // Мінімальне значення атрибуту
    public int MinValue { get; set; } = 0;

    // Модифікатор атрибуту
    public AttributeModifier AttributeModifier { get; private set; }

    // Події
    public event EventHandler<ModifierChangedEvent> OnMainValueChanged;
    public event EventHandler<ModifierChangedEvent> OnBaseValueChanged;
    public event EventHandler<AttributeTotalChangedEvent> OnTotalValueChanged;

    public Attribute(int baseValue, int minValue = -999) {
        MinValue = minValue;
        BaseValue = Math.Max(minValue, baseValue);
        _mainValue = BaseValue; // Встановлюємо напряму, щоб уникнути виклику події
        AttributeModifier = new AttributeModifier();

        // Підписуємось на події модифікатора
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

    // Зменшити значення атрибуту (отримання шкоди тощо)
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
        // Спочатку віднімаємо від бонусу
        int remainingValue = AttributeModifier.Subtract(amount);

        // Залишок віднімаємо від основного значення
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

    // Додати значення до атрибуту (лікування тощо)
    public int Add(int amount) {
        if (amount <= 0) return 0;

        int oldTotal = CurrentValue;

        // Спочатку відновлюємо основне значення до максимуму
        int mainDifference = BaseValue - MainValue;


        int excess = Restore(amount);
        int addedToMain = amount - excess;

        // Залишок додаємо до модифікатора
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

        // 1. Спочатку збільшуємо максимальне значення модифікатора
        AttributeModifier.Increase(amount);

        // 2. Відновлюємо основне здоров'я до базового (якщо воно не повне)
        int excess = Restore(amount);

        // 3. Якщо залишилось невикористане значення, додаємо його до поточного модифікатора
        if (excess > 0) {
            AttributeModifier.Add(excess);
        }
    }

    // Зменшити максимальне значення атрибуту
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

    // Встановити нове базове значення
    public bool SetBaseValue(int newBaseValue) {
        if (newBaseValue < MinValue)
            return false;

        int oldValue = BaseValue;
        BaseValue = newBaseValue;

        // Якщо поточне основне значення більше за нове базове, обмежуємо його
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

    // Відновити основне значення до максимуму
    public void RestoreToBase() {
        MainValue = BaseValue;
    }

    // Скинути всі значення
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
