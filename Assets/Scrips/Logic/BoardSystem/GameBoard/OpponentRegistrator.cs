using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    internal List<Opponent> GetActiveOpponents() {
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
