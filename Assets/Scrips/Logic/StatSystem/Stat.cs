using System;

public class Stat : IStat {
    public int CurrentValue { get; private set; }
    public int MinValue { get; private set; }
    public int MaxValue { get; private set; }
    public int InitialValue { get; private set; }
    public event Action<int, int> OnValueChanged;

    public Stat(int initialValue, int maxValue, int minValue = 0) {
        MinValue = minValue;
        MaxValue = Math.Max(minValue, maxValue);
        InitialValue = initialValue;
        CurrentValue = Math.Clamp(initialValue, minValue, maxValue);
    }

    public void Modify(int amount) {
        int beforeValue = CurrentValue;
        int newValue = Math.Clamp(CurrentValue + amount, MinValue, MaxValue);
        if (newValue != CurrentValue) {
            CurrentValue = newValue;
            OnValueChanged?.Invoke(beforeValue, CurrentValue);
        }
    }

    public void SetMaxValue(int newMaxValue) {
        if (newMaxValue < MinValue) return;
        MaxValue = newMaxValue;
    }

    public void SetMinValue(int newMinValue) {
        if (newMinValue > MaxValue) return;
        MinValue = newMinValue;
    }

    public void Reset() {
        CurrentValue = InitialValue;
    }
}
