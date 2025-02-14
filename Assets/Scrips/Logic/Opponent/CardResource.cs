using System;

public class CardResource {
    private readonly Mana mana;
    private readonly Health health;

    public bool IsManaEnabled { get; private set; } = false;

    public CardResource(Mana mana, Health health) {
        this.mana = mana;
        this.health = health;
    }

    public void EnableMana() {
        IsManaEnabled = true;
    }

    public void DisableMana() {
        IsManaEnabled = false;
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
        if (IsManaEnabled && mana.CurrentValue > 0) {
            return mana.Spend(amount);
        }
        return 0;
    }

    private int SpendHealth(int amount) {
        if (health.CurrentValue > 0) {
            return health.ApplyDamage(amount);
        }
        return 0;
    }

    // Restore resources when undoing
    public void Add(ResourceData resources) {
        RestoreMana(resources.Mana);
        HealHealth(resources.Health);
    }

    private void RestoreMana(int amount) {
        if (IsManaEnabled && amount > 0) {
            mana.Modify(amount);
        }
    }

    private void HealHealth(int amount) {
        if (amount > 0) {
            health.Modify(amount);
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
