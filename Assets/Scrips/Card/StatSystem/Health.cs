using System;

public class Health : Stat {
    public event Action OnDeath;

    public Health(int maxHealth, int initialHealth) : base(maxHealth, initialHealth) { }

    public void ApplyDamage(int damage) {
        if (damage <= 0) return;

        Modify(-damage);
        if (CurrentValue <= 0) {
            OnDeath?.Invoke();
            Console.WriteLine("Character has died.");
        } else {
            Console.WriteLine($"Took {damage} damage. Current health: {CurrentValue}");
        }
    }

    public void Heal(int amount) {
        if (amount <= 0) return;

        Modify(amount);
        Console.WriteLine($"Healed for {amount}. Current health: {CurrentValue}");
    }
}
