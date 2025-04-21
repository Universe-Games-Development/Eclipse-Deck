using System;

public class Cost : Attribute {
    public event Action OnCostIncreased; // ���� ��� ��������� ������
    public event Action OnCostDecreased; // ���� ��� ��������� ������

    /// <summary>
    /// ������� ����� ��������� ����� Cost � ������������ �� ���������� �������.
    /// </summary>
    /// <param name="maxCost">����������� ������� (������ ��������)</param>
    /// <param name="initialCost">��������� ������� �������</param>
    public Cost(int maxCost, int initialCost = 0) : base(maxCost) {
        // ���� ������� ��������� �������, ������������ ��
        if (initialCost > 0) {
            // ³������ �� �������������, ������� � ���� ������ MainValue ���������� � BaseValue
            Subtract(BaseValue - Math.Min(initialCost, maxCost));
        }
    }

    /// <summary>
    /// ��������� ������ �� ������� �������.
    /// </summary>
    public void IncreaseCost(int amount) {
        if (amount <= 0) return;

        int previousValue = CurrentValue;
        int added = Add(amount);

        if (added > 0) {
            OnCostIncreased?.Invoke();
            Console.WriteLine($"Cost increased by {added}. Current cost: {CurrentValue}/{TotalValue}");
        }
    }

    /// <summary>
    /// ��������� ������ �� ������� �������.
    /// </summary>
    public void DecreaseCost(int amount) {
        if (amount <= 0) return;

        int previousValue = CurrentValue;
        int subtracted = Subtract(amount);

        if (subtracted > 0) {
            OnCostDecreased?.Invoke();
            Console.WriteLine($"Cost decreased by {subtracted}. Current cost: {CurrentValue}/{TotalValue}");
        }
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
            Subtract(cost);
            Console.WriteLine($"Used {cost} resources. Remaining cost: {CurrentValue}/{TotalValue}");
            return true;
        } else {
            Console.WriteLine("Not enough resources to use.");
            return false;
        }
    }

    /// <summary>
    /// ������ ����������� �������.
    /// </summary>
    public void ModifyMaxCost(int amount) {
        if (amount == 0) return;

        // ��� ��������� ����������� �������
        if (amount > 0) {
            AddModifier(amount);
        }
        // ��� ��������� ����������� �������
        else {
            RemoveModifier(-amount);
        }
    }

    /// <summary>
    /// ���������� ���� ������ ����������� �������.
    /// </summary>
    public bool SetMaxCost(int newMaxCost) {
        return SetBaseValue(newMaxCost);
    }

    /// <summary>
    /// ������� �������� ������� ������� �� ���������.
    /// </summary>
    public void RefillCost() {
        RestoreToBase();
        AttributeModifier.Add(AttributeModifier.TotalValue);
    }

    public override string ToString() {
        return $"Cost: {CurrentValue}/{TotalValue}";
    }
}