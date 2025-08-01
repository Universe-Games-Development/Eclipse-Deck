using System;

public interface IMana {
    int Current { get; }
    int Max { get; }
    Character Owner { get; }

    event Action<Character> OnManaEmpty;
    event Action<Character> OnManaRestored;
    event Action<Character, int> OnManaSpent;
    int Spend(int amount);
    void ModifyMax(int amount);

    void SetRestoreAmount(int newRestoreAmount);
    
    string ToString();
    void Dispose();
}