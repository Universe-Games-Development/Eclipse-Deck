using System;

public class Cost : Stat {
    public event Action OnCostIncreased; // Подія для збільшення витрат
    public event Action OnCostDecreased; // Подія для зменшення витрат

    public Cost(int maxCost, int initialCost) : base(maxCost, initialCost) { }

    /// <summary>
    /// Збільшення витрат на вказану кількість.
    /// </summary>
    public void IncreaseCost(int amount) {
        if (amount <= 0) return;

        Modify(amount);
        OnCostIncreased?.Invoke();
        Console.WriteLine($"Cost increased by {amount}. Current cost: {CurrentValue}");
    }

    /// <summary>
    /// Зменшення витрат на вказану кількість.
    /// </summary>
    public void DecreaseCost(int amount) {
        if (amount <= 0) return;

        Modify(-amount);
        OnCostDecreased?.Invoke();
        Console.WriteLine($"Cost decreased by {amount}. Current cost: {CurrentValue}");
    }

    /// <summary>
    /// Перевірка, чи вистачає ресурсів для витрат.
    /// </summary>
    public bool CanAfford(int cost) {
        return CurrentValue >= cost;
    }

    /// <summary>
    /// Використовувати ресурси, якщо вистачає.
    /// </summary>
    public bool UseCost(int cost) {
        if (CanAfford(cost)) {
            Modify(-cost);
            Console.WriteLine($"Used {cost} resources. Remaining cost: {CurrentValue}");
            return true;
        } else {
            Console.WriteLine("Not enough resources to use.");
            return false;
        }
    }
}
