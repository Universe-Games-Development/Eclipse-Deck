using System;

public class Cost : Attribute {
    public event Action OnCostIncreased;
    public event Action OnCostDecreased; 
    private const int max_Cost = 100;

    public Cost(Attribute attribute) : base(attribute) {
    }

    public Cost(int initialCost = 0, int maxCost = max_Cost) : base(maxCost) {
        // Якщо вказано початкову вартість, встановлюємо її
        if (initialCost > 0) {
            // Віднімаємо від максимального, оскільки в новій системі MainValue починається з BaseValue
            Subtract(BaseValue - Math.Min(initialCost, maxCost));
        }
    }

    /// <summary>
    /// Збільшення витрат на вказану кількість.
    /// </summary>
    public void IncreaseCost(int amount) {
        if (amount <= 0) return;

        int previousValue = Current;
        int added = Add(amount);

        if (added > 0) {
            OnCostIncreased?.Invoke();
            Console.WriteLine($"Cost increased by {added}. Current cost: {Current}/{TotalValue}");
        }
    }

    /// <summary>
    /// Зменшення витрат на вказану кількість.
    /// </summary>
    public void DecreaseCost(int amount) {
        if (amount <= 0) return;

        int previousValue = Current;
        int subtracted = Subtract(amount);

        if (subtracted > 0) {
            OnCostDecreased?.Invoke();
            Console.WriteLine($"Cost decreased by {subtracted}. Current cost: {Current}/{TotalValue}");
        }
    }

    /// <summary>
    /// Перевірка, чи вистачає ресурсів для витрат.
    /// </summary>
    public bool CanAfford(int cost) {
        return Current >= cost;
    }

    /// <summary>
    /// Використовувати ресурси, якщо вистачає.
    /// </summary>
    public bool UseCost(int cost) {
        if (CanAfford(cost)) {
            Subtract(cost);
            Console.WriteLine($"Used {cost} resources. Remaining cost: {Current}/{TotalValue}");
            return true;
        } else {
            Console.WriteLine("Not enough resources to use.");
            return false;
        }
    }

    /// <summary>
    /// Змінити максимальну вартість.
    /// </summary>
    public void ModifyMaxCost(int amount) {
        if (amount == 0) return;

        // Для збільшення максимальної вартості
        if (amount > 0) {
            AddModifier(amount);
        }
        // Для зменшення максимальної вартості
        else {
            RemoveModifier(-amount);
        }
    }

    /// <summary>
    /// Встановити нову базову максимальну вартість.
    /// </summary>
    public bool SetMaxCost(int newMaxCost) {
        return SetBaseValue(newMaxCost);
    }

    /// <summary>
    /// Повністю відновити доступні ресурси до максимуму.
    /// </summary>
    public void RefillCost() {
        RestoreToBase();
        AttributeModifier.Add(AttributeModifier.TotalValue);
    }

    public override string ToString() {
        return $"Cost: {Current}/{TotalValue}";
    }
}