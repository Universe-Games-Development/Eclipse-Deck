using System;
using UnityEngine;
using System.Collections.Generic;

public class BattleRegistrator {
    public Action<Player, Enemy> OnMatchSetup;
    public Action<Player> OnPlayerRegistered;

    // Properties
    private Player _player;
    private Enemy _enemy;

    // Event for game start when both opponents are ready
    public bool IsMatchReady => _player != null && _enemy != null;

    // Register player presenter
    public void RegisterPlayer(Player player) {
        if (player == null) return;
        if (_player != null) {
            Debug.LogWarning("A player is already registered.");
            return;
        }

        _player = player;

        // Subscribe to player's model defeat event
        _player.OnDefeat += (opponent) => UnregisterPlayer();
        OnPlayerRegistered?.Invoke(_player);
        Debug.Log($"Player {player} registered.");

        CheckAndTriggerMatchSetup();
    }

    // Register enemy presenter
    public void RegisterEnemy(Enemy enemy) {
        if (enemy == null) return;
        if (_enemy != null) {
            Debug.LogWarning("An enemy is already registered.");
            return;
        }

        _enemy = enemy;

        // Subscribe to enemy's model defeat event
        _enemy.OnDefeat += (opponent) => UnregisterEnemy();
        Debug.Log($"Enemy {_enemy} registered.");

        CheckAndTriggerMatchSetup();
    }

    // Helper to check if both opponents are registered and trigger match setup
    private void CheckAndTriggerMatchSetup() {
        if (IsMatchReady) {
            Debug.Log("Both opponents registered. Match setup complete.");
            if (OnMatchSetup != null) {
                OnMatchSetup.Invoke(_player, _enemy);
            }
        }
    }

    // Unregister player
    public void UnregisterPlayer() {
        if (_player == null) return;
        Debug.Log($"Player {_player} unregistered.");
        _player = null;
    }

    // Unregister enemy
    public void UnregisterEnemy() {
        if (_enemy == null) return;
        Debug.Log($"Enemy {_enemy} unregistered.");
        _enemy = null;
    }

    // Helper methods to get opponents
    public Player GetPlayer() => _player;
    public Enemy GetEnemy() => _enemy;

    // Get all active opponent models
    public List<Opponent> GetActiveOpponents() {
        var list = new List<Opponent>();
        if (_player != null) list.Add(_player);
        if (_enemy != null) list.Add(_enemy);
        return list;
    }
}