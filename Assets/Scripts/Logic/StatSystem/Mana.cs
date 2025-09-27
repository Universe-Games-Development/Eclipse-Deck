using System;

public class Mana : Attribute {
    public event Action<int> OnManaSpent;
    public int RestoreAmount { get; private set; } = 1;

    public Mana(int initialValue) : base(initialValue) {

        // Підписуємося на події зміни значень атрибуту
        OnTotalValueChanged += HandleTotalValueChanged;
    }

    private void HandleTotalValueChanged(object sender, AttributeTotalChangedEvent e) {
        // Можна додати додаткову логіку при зміні значення мани
    }

    public void RestoreMana() {
        if (RestoreAmount <= 0) return;

        int previousMana = Current;
        int restored = Add(RestoreAmount);

        if (Current == TotalValue) {
            //_eventBus.Raise(new OnManaRestored(Owner));
            //OnManaRestored?.Invoke(Owner);
        } else {
            Console.WriteLine($"Restored {restored} mana. Current mana: {Current}/{TotalValue}");
        }
    }

    public int Spend(int amount) {
        if (amount <= 0) return 0;

        int previousMana = Current;

        int amountSpent = Subtract(amount);

        //_eventBus.Raise(new OnManaSpent(Owner, amountSpent));
        //OnManaSpent?.Invoke(Owner, amountSpent);

        if (amountSpent > 0) {
            OnManaSpent?.Invoke(amountSpent);
        } else {
            Console.WriteLine($"Spent {amountSpent} mana. Current mana: {Current}/{TotalValue}");
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
        return $"Mana: {Current}/{TotalValue}";
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