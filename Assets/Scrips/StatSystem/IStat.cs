using System;

public interface IStat {
    int CurrentValue { get; }
    int MaxValue { get; }
    event Action<int, int> OnValueChanged; // ���� ���� ��������
    void Modify(int amount); // ���� ��������� ��������
    void SetMaxValue(int maxValue); // ���� ������������� ��������
}
