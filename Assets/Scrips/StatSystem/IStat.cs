using System;

public interface IStat {
    int CurrentValue { get; }
    int MaxValue { get; }
    event Action<int, int> OnValueChanged; // Подія зміни значення
    void Modify(int amount); // Зміна поточного значення
    void SetMaxValue(int maxValue); // Зміна максимального значення
}
