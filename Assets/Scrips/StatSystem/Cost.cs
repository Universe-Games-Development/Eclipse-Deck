using System;

public class Cost : Stat {
    public event Action OnCostIncreased; // ���� ��� ��������� ������
    public event Action OnCostDecreased; // ���� ��� ��������� ������

    public Cost(int maxCost, int initialCost) : base(maxCost, initialCost) { }

    /// <summary>
    /// ��������� ������ �� ������� �������.
    /// </summary>
    public void IncreaseCost(int amount) {
        if (amount <= 0) return;

        Modify(amount);
        OnCostIncreased?.Invoke();
        Console.WriteLine($"Cost increased by {amount}. Current cost: {CurrentValue}");
    }

    /// <summary>
    /// ��������� ������ �� ������� �������.
    /// </summary>
    public void DecreaseCost(int amount) {
        if (amount <= 0) return;

        Modify(-amount);
        OnCostDecreased?.Invoke();
        Console.WriteLine($"Cost decreased by {amount}. Current cost: {CurrentValue}");
    }

    /// <summary>
    /// ��������, �� ������� ������� ��� ������.
    /// </summary>
    public bool CanAfford(int cost) {
        return CurrentValue >= cost;
    }

    /// <summary>
    /// ��������������� �������, ���� �������.
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
