using System;



public class CardSpendable {
    private readonly Mana mana;
    private readonly Health health;

    public bool IsManaEnabled { get; private set; } = false;
    private IEventBus<IEvent> _eventBus;

    public CardSpendable(Mana mana, Health health) {
        this.mana = mana;
        this.health = health;
        //_eventBus = eventBus;
    }

    public void EnableMana() {
        IsManaEnabled = true;
        _eventBus.SubscribeTo<TurnStartEvent>(RestoreMana);
    }

    public void DisableMana() {
        IsManaEnabled = false;
        _eventBus.UnsubscribeFrom<TurnStartEvent>(RestoreMana);
    }

    private void RestoreMana(ref TurnStartEvent eventData) {
        throw new NotImplementedException();
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
            health.Heal(amount, out int excess);
        }
    }

    internal void TryRefund(ResourceData resourceData) {
        throw new NotImplementedException();
    }

    public void Dispose() {
        if (_eventBus != null) {
            _eventBus.UnsubscribeFrom<TurnStartEvent>(RestoreMana);
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
