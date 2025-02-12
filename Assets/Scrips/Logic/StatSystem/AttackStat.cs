using System;

public class Attack : Stat {
    public event Action OnAttackBoosted; // ���� ��� ��������� �����
    public event Action OnAttackReduced; // ���� ��� ��������� �����
    public GameEventBus eventBus;
    public Attack(IDamageDealer owner, int maxAttack, int initialAttack, GameEventBus gameEventBus) : base(maxAttack, initialAttack) {
        eventBus = gameEventBus;
    }

    /// <summary>
    /// ϳ�������� ����� �� ������� �������.
    /// </summary>
    public void BoostAttack(int amount) {
        if (amount <= 0) return;

        Modify(amount);
        OnAttackBoosted?.Invoke();
        Console.WriteLine($"Attack boosted by {amount}. Current attack: {CurrentValue}");
    }

    /// <summary>
    /// ��������� ����� �� ������� �������.
    /// </summary>
    public void ReduceAttack(int amount) {
        if (amount <= 0) return;

        Modify(-amount);
        OnAttackReduced?.Invoke();
        Console.WriteLine($"Attack reduced by {amount}. Current attack: {CurrentValue}");
    }
}