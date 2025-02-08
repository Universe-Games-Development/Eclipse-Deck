using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Zenject;

public class OpponentRegistrator {
    public Action<List<Opponent>> OnOpponentsRegistered;
    public Action<Opponent> OnOpponentUnregistered;
    public Action<Opponent> OnOpponentRegistered;

    public List<Opponent> registeredOpponents = new();
    public int RequiredOpponents = 2;

    public void RegisterOpponent(Opponent opponent) {
        if (opponent == null) return;

        if (registeredOpponents.Contains(opponent)) {
            Debug.LogWarning("Opponent already registered.");
            return;
        }

        if (registeredOpponents.Count >= RequiredOpponents) {
            Debug.LogWarning("Cannot register more opponents. Maximum players reached.");
            return;
        }

        registeredOpponents.Add(opponent);
        opponent.OnDefeat += UnregisterOpponent;
        OnOpponentRegistered?.Invoke(opponent);

        if (registeredOpponents.Count == RequiredOpponents) {
            OnOpponentsRegistered?.Invoke(registeredOpponents);
        }

        Debug.Log($"Opponent {opponent.Name} registered.");
    }

    public void UnregisterOpponent(Opponent opponent) {
        if (opponent == null) return;

        if (!registeredOpponents.Remove(opponent)) return;

        opponent.OnDefeat -= UnregisterOpponent; // Відписуємо подію
        OnOpponentUnregistered?.Invoke(opponent);
        Debug.Log($"Opponent {opponent.Name} unregistered.");
    }

    public bool IsAllRegistered() => registeredOpponents.Count >= RequiredOpponents;

    public Opponent GetEnemy() {
        var enemy = registeredOpponents.FirstOrDefault(opponent => opponent is Enemy);
        if (enemy == null) Debug.LogWarning("No Enemy found in registered opponents.");
        return enemy;
    }

    public Opponent GetPlayer() {
        var player = registeredOpponents.FirstOrDefault(opponent => opponent is Player);
        if (player == null) Debug.LogWarning("No Player found in registered opponents.");
        return player;
    }

    internal IEnumerable<Opponent> GetActiveOpponents() {
        return registeredOpponents;
    }

    public Opponent GetRandomOpponent() {
        if (registeredOpponents.Count == 0) {
            Debug.LogWarning("No registered opponents available.");
            return null;
        }
        return registeredOpponents[UnityEngine.Random.Range(0, registeredOpponents.Count)];
    }

    public Opponent GetNextOpponent(Opponent ActiveOpponent) {
        if (registeredOpponents.Count == 0) {
            Debug.LogWarning("No registered opponents available.");
            return null;
        }

        if (ActiveOpponent == null || !registeredOpponents.Contains(ActiveOpponent)) {
            return registeredOpponents[0];
        }

        int currentIndex = registeredOpponents.IndexOf(ActiveOpponent);
        int nextIndex = (currentIndex + 1) % registeredOpponents.Count;
        return registeredOpponents[nextIndex];
    }
}


public class TurnManager {
    public Action<Opponent> OnTurnBegan;
    public Func<Opponent, UniTask> OnTurnPerform;
    public Action<Opponent> OnTurnEnd;

    public Opponent ActiveOpponent { get; private set; }

    public OpponentRegistrator registrator;

    [Inject]
    public void Construct(OpponentRegistrator registrator) {
       this.registrator = registrator;
        registrator.OnOpponentsRegistered += InitTurns;
        registrator.OnOpponentUnregistered += RemoveOpponent;
    }

    private void InitTurns(List<Opponent> registeredOpponents) {
        foreach (var opponent in registeredOpponents) {
            opponent.OnDefeat += RemoveOpponent;
        }

        if (registeredOpponents.Count > 0) {
            ActiveOpponent = registeredOpponents[0]; // Встановлюємо першого гравця
            OnTurnBegan?.Invoke(ActiveOpponent);
        }
    }

    public async UniTask PerformTurn(Opponent opponent) {
        if (opponent != ActiveOpponent) {
            Debug.LogWarning($"{opponent.Name} cannot perform a turn right now!");
            return;
        }

        if (OnTurnPerform != null) {
            await OnTurnPerform.Invoke(opponent);
        }

        OnTurnEnd?.Invoke(ActiveOpponent);
        ChangeTurn();
    }

    public void ChangeTurn() {
        if (!registrator.IsAllRegistered()) {
            Debug.LogWarning("Not all registered opponents left. Game should end.");
            return;
        }

        ActiveOpponent = SetNextOpponent();

        if (ActiveOpponent != null) {
            Debug.Log($"It is now {ActiveOpponent.Name}'s turn.");
            OnTurnBegan?.Invoke(ActiveOpponent);
        }
    }

    public void RemoveOpponent(Opponent opponent) {
        if (opponent == null) return;

        opponent.OnDefeat -= RemoveOpponent;

        Debug.Log($"Opponent {opponent.Name} unregistered.");

        if (ActiveOpponent == opponent) {
            ActiveOpponent = SetNextOpponent();

            if (ActiveOpponent != null) {
                OnTurnBegan?.Invoke(ActiveOpponent);
            } else {
                Debug.LogWarning("No more opponents left. Game Over.");
            }
        }
    }

    public Opponent GetActivePlayer() => ActiveOpponent;

    public Opponent SetNextOpponent() {
        ActiveOpponent = registrator.GetNextOpponent(ActiveOpponent);
        return ActiveOpponent;
    }
}
