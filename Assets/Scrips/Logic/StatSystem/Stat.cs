using System;

public class Stat : IStat {
    protected int currentValue;
    protected int minValue;
    protected int maxValue;
    protected int initialValue;
    public event Action<int, int> OnValueChanged;

    public int InitialValue {
        get => initialValue;
        private set {
            initialValue = Math.Clamp(value, 0, MaxValue);
        }
    }

    public int CurrentValue {
        get => currentValue;
        private set {
            currentValue = Math.Clamp(value, 0, MaxValue);
            OnValueChanged?.Invoke(currentValue, InitialValue);
        }
    }

    public int MaxValue {
        get => maxValue;
        private set {
            maxValue = Math.Max(0, value);
            CurrentValue = Math.Clamp(CurrentValue, 0, maxValue);
        }
    }

    public int MinValue {
        get => minValue;
        private set {
            minValue = Math.Min(maxValue, value);
            CurrentValue = Math.Clamp(CurrentValue, minValue, MaxValue);
        }
    }

    public Stat(int maxValue, int initialValue, int minValue = 0) {
        this.minValue = minValue;
        this.maxValue = Math.Max(minValue, maxValue);
        this.initialValue = initialValue;
        currentValue = Math.Clamp(initialValue, minValue, maxValue);
    }

    public void Modify(int amount) {
        int beforeValue = currentValue;
        int newValue = Math.Clamp(currentValue + amount, MinValue, MaxValue);

        if (newValue != currentValue) {
            CurrentValue = newValue; // —пов≥щаЇмо про зм≥ни т≥льки тод≥, коли Ї р≥зниц€
            OnValueChanged?.Invoke(beforeValue, currentValue);
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
        CurrentValue = initialValue;
    }
}
