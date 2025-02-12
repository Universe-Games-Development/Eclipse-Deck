using System;

public class Stat : IStat {
    private int currentValue;
    private int minValue;
    private int maxValue;
    private int initialValue;

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


    public event Action<int, int> OnValueChanged;

    public Stat(int maxValue, int initialValue, int minValue = 0) {
        this.minValue = minValue;
        this.maxValue = Math.Max(minValue, maxValue);
        this.initialValue = initialValue;
        this.currentValue = Math.Clamp(initialValue, minValue, maxValue);
    }

    public void Modify(int amount) {
        CurrentValue += amount;
    }

    public void SetMaxValue(int newMaxValue) {
        MaxValue = newMaxValue;
    }

    public void SetMinValue(int newMinValue) {
        MinValue = newMinValue;
    }

    // Додавання методу Reset
    public void Reset() {
        CurrentValue = initialValue;  // Скидаємо значення до початкового значення
    }
}
