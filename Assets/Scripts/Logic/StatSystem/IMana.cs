using System;

public interface IMana {
    int Current { get; }
    int Max { get; }
    Opponent Owner { get; }

    event Action<Opponent> OnManaEmpty;
    event Action<Opponent> OnManaRestored;
    event Action<Opponent, int> OnManaSpent;
    int Spend(int amount);
    void ModifyMax(int amount);

    void SetRestoreAmount(int newRestoreAmount);

    string ToString();
    void Dispose();
}