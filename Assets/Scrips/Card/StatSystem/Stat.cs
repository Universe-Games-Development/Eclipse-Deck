using System;

public class Stat : IStat {
    private int currentValue;
    private int maxValue;
    private int initialValue;  // Природне початкове значення

    public int InitialValue {
        get => initialValue;
        private set {
            initialValue = Math.Clamp(value, 0, MaxValue);
            OnValueChanged?.Invoke(initialValue, MaxValue);
        }
    }

    public int CurrentValue {
        get => currentValue;
        private set {
            currentValue = Math.Clamp(value, 0, MaxValue);
            OnValueChanged?.Invoke(currentValue, MaxValue);
        }
    }

    public int MaxValue {
        get => maxValue;
        private set {
            maxValue = Math.Max(0, value);
            CurrentValue = Math.Clamp(CurrentValue, 0, maxValue);
        }
    }

    public event Action<int, int> OnValueChanged;
    public event Action<int, int> OnInitialValueChanged;

    public Stat(int maxValue, int initialValue) {
        this.maxValue = Math.Max(0, maxValue);
        this.initialValue = initialValue;
        this.currentValue = Math.Clamp(initialValue, 0, maxValue);
    }

    public void Modify(int amount) {
        CurrentValue += amount;
    }

    public void SetMaxValue(int newMaxValue) {
        MaxValue = newMaxValue;
    }

    // Додавання методу Reset
    public void Reset() {
        CurrentValue = initialValue;  // Скидаємо значення до початкового значення
    }
}
