using System;

public class CardSpendable {
    private readonly Mana mana;
    private readonly Health health;

    public bool IsManaEnabled { get; private set; } = false;

    public CardSpendable(Mana mana, Health health) {
        this.mana = mana;
        this.health = health;
    }

    public void EnableMana() {
        IsManaEnabled = true;
        mana.EnableManaRestoreation();
    }

    public void DisableMana() {
        IsManaEnabled = false;
        mana.DisableManaRestoreation();
    }

    // Try to spend mana, if not enough mana - spend health
    public ResourceData TrySpend(int amount) {
        int spentHealth = 0;
        int spentMana = TrySpendMana(amount);

        if (spentMana < amount) {
            spentHealth = SpendHealth(amount - spentMana);
        }

        return new ResourceData(spentMana, spentHealth);
    }

    private int TrySpendMana(int amount) {
        if (IsManaEnabled && mana.Current > 0) {
            return mana.Spend(amount);
        }
        return 0;
    }

    private int SpendHealth(int amount) {
        if (health.Current > 0) {
            health.TakeDamage(amount);
            return amount;
        }
        return 0;
    }

    private void HealHealth(int amount) {
        if (amount > 0) {
            health.Heal(amount);
        }
    }
}

public class ResourceData {
    public int Mana { get; private set; }
    public int Health { get; private set; }

    public ResourceData(int mana = 0, int health = 0) {
        Mana = mana;
        Health = health;
    }
}
