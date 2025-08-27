using System;

public class Mana : Attribute {
    public event Action<ManaSpentEvent> OnManaSpent;
    public int RestoreAmount { get; private set; } = 1;
    private IMannable _owner;

    public Mana(IMannable owner, int initialValue) : base(initialValue) {
        _owner = owner;

        // Підписуємося на події зміни значень атрибуту
        OnTotalValueChanged += HandleTotalValueChanged;
    }

    private void HandleTotalValueChanged(object sender, AttributeTotalChangedEvent e) {
        // Можна додати додаткову логіку при зміні значення мани
    }

    public void RestoreMana() {
        if (RestoreAmount <= 0) return;

        int previousMana = CurrentValue;
        int restored = Add(RestoreAmount);

        if (CurrentValue == TotalValue) {
            //_eventBus.Raise(new OnManaRestored(Owner));
            //OnManaRestored?.Invoke(Owner);
        } else {
            Console.WriteLine($"Restored {restored} mana. Current mana: {CurrentValue}/{TotalValue}");
        }
    }

    public int Spend(int amount) {
        if (amount <= 0) return 0;

        int previousMana = CurrentValue;

        int amountSpent = Subtract(amount);

        //_eventBus.Raise(new OnManaSpent(Owner, amountSpent));
        //OnManaSpent?.Invoke(Owner, amountSpent);

        if (amountSpent > 0) {
            OnManaSpent?.Invoke(new ManaSpentEvent(_owner, amountSpent));
        } else {
            Console.WriteLine($"Spent {amountSpent} mana. Current mana: {CurrentValue}/{TotalValue}");
        }

        return amountSpent;
    }

    public void SetRestoreAmount(int newRestoreAmount) {
        if (newRestoreAmount < 0) return;
        RestoreAmount = newRestoreAmount;
    }

    public void ModifyMax(int amount) {
        if (amount == 0) return;

        // Додаємо або видаляємо модифікатор максимального значення
        if (amount > 0) {
            AddModifier(amount);
        } else {
            RemoveModifier(-amount);
        }
    }

    public override string ToString() {
        return $"Mana: {CurrentValue}/{TotalValue}";
    }

    public void Dispose() {
        // Відписуємося від власних подій
        OnTotalValueChanged -= HandleTotalValueChanged;
    }
}

public struct OnManaEmpty : IEvent {
    public IMannable Owner { get; }
    public OnManaEmpty(IMannable owner) {
        Owner = owner;
    }
}

public struct ManaSpentEvent : IEvent {
    public IMannable Owner { get; }
    public int Amount { get; }
    public ManaSpentEvent(IMannable owner, int amount) {
        Owner = owner;
        Amount = amount;
    }
}

public struct OnManaRestored : IEvent {
    public IMannable Owner { get; }
    public OnManaRestored(IMannable owner) {
        Owner = owner;
    }
}