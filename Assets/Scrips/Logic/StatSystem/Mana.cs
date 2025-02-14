using System;
using UnityEngine;

public class Mana : Stat {
    public Opponent Owner { get; }
    public event Action<Opponent> OnManaEmpty;
    public event Action<Opponent, int> OnManaSpent;
    public event Action<Opponent> OnManaRestored;
    private readonly GameEventBus _eventBus;
    public int restoreAmount = 1;

    public Mana(Opponent owner, int maxMana, int initialMana, GameEventBus eventBus)
        : base(maxMana, initialMana) {
        Owner = owner;
        _eventBus = eventBus;
        eventBus.SubscribeTo<OnTurnStart>(RestoreMana);
    }

    private void RestoreMana(ref OnTurnStart eventData) {
        Opponent startTurnOpponent = eventData.startTurnOpponent;
        if (startTurnOpponent != Owner) {
            return;
        }
        if (restoreAmount <= 0) return;
        var previousMana = CurrentValue;
        Modify(restoreAmount);
        if (CurrentValue == MaxValue) {
            _eventBus.Raise(new OnManaRestored(Owner));
            OnManaRestored?.Invoke(Owner);
        } else {
            Console.WriteLine($"Restored {restoreAmount} mana. Current mana: {CurrentValue}");
        }
    }

    public int Spend(int amount) {
        if (amount <= 0) return 0;

        var previousMana = CurrentValue;
        var amountSpent = Mathf.Min(amount, CurrentValue);
        Modify(-amountSpent);

        _eventBus.Raise(new OnManaSpent(Owner, amountSpent));
        OnManaSpent?.Invoke(Owner, amountSpent);

        if (CurrentValue <= 0 && previousMana > 0) {
            _eventBus.Raise(new OnManaEmpty(Owner));
            OnManaEmpty?.Invoke(Owner);
        } else {
            Console.WriteLine($"Spent {amountSpent} mana. Current mana: {CurrentValue}");
        }

        return amount - amountSpent;
    }

    public void SetRestoreAmount(int newRestoreAmount) {
        if (newRestoreAmount < 0) return;
        restoreAmount = newRestoreAmount;
    }

    public void ModifyMax(int amount) {
        if (amount == 0) return;

        int newMaxValue = Mathf.Max(MinValue, MaxValue + amount);
        SetMaxValue(newMaxValue);
    }

    public override string ToString() {
        return $"Mana: {CurrentValue}/{MaxValue}";
    }

    public void Dispose() {
        if (_eventBus != null) {
            _eventBus.UnsubscribeFrom<OnTurnStart>(RestoreMana);
        }
    }
}

public class OnManaEmpty : IEvent {
    public Opponent Owner { get; }
    public OnManaEmpty(Opponent owner) {
        Owner = owner;
    }
}

public class OnManaSpent : IEvent {
    public Opponent Owner { get; }
    public int Amount { get; }
    public OnManaSpent(Opponent owner, int amount) {
        Owner = owner;
        Amount = amount;
    }
}

public class OnManaRestored : IEvent {
    public Opponent Owner { get; }
    public OnManaRestored(Opponent owner) {
        Owner = owner;
    }
}