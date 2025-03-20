using System;
using Unity.VisualScripting;
using UnityEngine;

public class Mana : IMana {
    private readonly Stat _stat;
    public Opponent Owner { get; }

    public int Current => _stat.CurrentValue;

    public int Max => _stat.MaxValue;

    public event Action<Opponent> OnManaEmpty;
    public event Action<Opponent, int> OnManaSpent;
    public event Action<Opponent> OnManaRestored;
    private readonly GameEventBus _eventBus;
    public int restoreAmount = 1;

    public Mana(Opponent owner, Stat manaStat, GameEventBus eventBus) {
        Owner = owner;
        _eventBus = eventBus;
    }

    private void RestoreMana(ref OnTurnStart eventData) {
        Opponent startTurnOpponent = eventData.StartingOpponent;
        if (startTurnOpponent != Owner) {
            return;
        }
        if (restoreAmount <= 0) return;
        var previousMana = Current;
        _stat.Modify(restoreAmount);
        if (Current == Max) {
            _eventBus.Raise(new OnManaRestored(Owner));
            OnManaRestored?.Invoke(Owner);
        } else {
            Console.WriteLine($"Restored {restoreAmount} mana. Current mana: {Current}");
        }
    }

    public int Spend(int amount) {
        if (amount <= 0) return 0;

        var previousMana = Current;
        var amountSpent = Mathf.Min(amount, Current);
        _stat.Modify(-amountSpent);

        _eventBus.Raise(new OnManaSpent(Owner, amountSpent));
        OnManaSpent?.Invoke(Owner, amountSpent);

        if (Current <= 0 && previousMana > 0) {
            _eventBus.Raise(new OnManaEmpty(Owner));
            OnManaEmpty?.Invoke(Owner);
        } else {
            Console.WriteLine($"Spent {amountSpent} mana. Current mana: {Current}");
        }

        return amount - amountSpent;
    }

    public void SetRestoreAmount(int newRestoreAmount) {
        if (newRestoreAmount < 0) return;
        restoreAmount = newRestoreAmount;
    }

    public void ModifyMax(int amount) {
        if (amount == 0) return;

        int newMaxValue = Mathf.Max(_stat.MinValue, Max + amount);
        _stat.SetMaxValue(newMaxValue);

        if (Current > newMaxValue) {
            _stat.Modify(newMaxValue - Current);
        }
    }

    public override string ToString() {
        return $"Mana: {Current}/{Max}";
    }

    public void Dispose() {
        if (_eventBus != null) {
            _eventBus.UnsubscribeFrom<OnTurnStart>(RestoreMana);
        }
    }

    public void EnableManaRestoreation() {
        _eventBus.SubscribeTo<OnTurnStart>(RestoreMana);
    }

    public void DisableManaRestoreation() {
        _eventBus.UnsubscribeFrom<OnTurnStart>(RestoreMana);
    }
}

public struct OnManaEmpty : IEvent {
    public Opponent Owner { get; }
    public OnManaEmpty(Opponent owner) {
        Owner = owner;
    }
}

public struct OnManaSpent : IEvent {
    public Opponent Owner { get; }
    public int Amount { get; }
    public OnManaSpent(Opponent owner, int amount) {
        Owner = owner;
        Amount = amount;
    }
}

public struct OnManaRestored : IEvent {
    public Opponent Owner { get; }
    public OnManaRestored(Opponent owner) {
        Owner = owner;
    }
}