using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IOpponentRegistry {
    void RegisterOpponent(Opponent newOpponent);

    void UnregisterOpponent(string opponentId);
    Opponent GetAgainstOpponentId(string opponentId);
    List<Opponent> GetOpponents();
}

public class OpponentRegistry : IOpponentRegistry {
    private readonly Dictionary<string, Opponent> _opponents = new();

    public void RegisterOpponent(Opponent newOpponent) {
        string opponentId = newOpponent.Id;
        if (_opponents.ContainsKey(opponentId)) {
            Debug.LogWarning($"Opponent already registered with Id {opponentId}");
            return;
        }
        _opponents[opponentId] = newOpponent;
    }

    public void UnregisterOpponent(string opponentId) {
        if (!_opponents.Remove(opponentId)) {
            Debug.LogWarning($"No opponent found to unregister with Id {opponentId}");
        }
    }

    public Opponent GetAgainstOpponentId(string opponentId) {
        foreach (var kvp in _opponents) {
            if (kvp.Key != opponentId) {
                return kvp.Value;
            }
        }
        Debug.LogWarning($"No opponent found against player {opponentId}");
        return null;
    }

    // --- Шорткати для зручності ---
    public void UnregisterOpponent(Opponent opponent)
        => UnregisterOpponent(opponent.Id);

    public Opponent GetAgainstOpponentId(Opponent opponent)
        => GetAgainstOpponentId(opponent.Id);

    public List<Opponent> GetOpponents() {
        return _opponents.Values.ToList();
    }
}

