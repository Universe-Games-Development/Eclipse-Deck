using System;
using UnityEngine;
using System.Collections.Generic;

public class BattleRegistrator {
    public Action<PlayerPresenter, EnemyPresenter> OnMatchSetup;

    // Properties
    private PlayerPresenter _player;
    private EnemyPresenter _enemy;

    // Event for game start when both opponents are ready
    public bool IsMatchReady => _player != null && _enemy != null;

    // Register player presenter
    public void RegisterPlayer(PlayerPresenter playerPresenter) {
        if (playerPresenter == null) return;
        if (_player != null) {
            Debug.LogWarning("A player is already registered.");
            return;
        }

        _player = playerPresenter;

        // Subscribe to player's model defeat event
        _player.Player.OnDefeat += (opponent) => UnregisterPlayer();
        Debug.Log($"Player {playerPresenter.Player} registered.");

        CheckAndTriggerMatchSetup();
    }

    // Register enemy presenter
    public void RegisterEnemy(EnemyPresenter enemyPresenter) {
        if (enemyPresenter == null) return;
        if (_enemy != null) {
            Debug.LogWarning("An enemy is already registered.");
            return;
        }

        _enemy = enemyPresenter;

        // Subscribe to enemy's model defeat event
        _enemy.Enemy.OnDefeat += (opponent) => UnregisterEnemy();
        Debug.Log($"Enemy {enemyPresenter.Enemy} registered.");

        CheckAndTriggerMatchSetup();
    }

    // Helper to check if both opponents are registered and trigger match setup
    private void CheckAndTriggerMatchSetup() {
        if (IsMatchReady) {
            Debug.Log("Both opponents registered. Match setup complete.");
            OnMatchSetup?.Invoke(_player, _enemy);
        }
    }

    // Unregister player
    public void UnregisterPlayer() {
        if (_player == null) return;
        Debug.Log($"Player {_player.Player} unregistered.");
        _player = null;
    }

    // Unregister enemy
    public void UnregisterEnemy() {
        if (_enemy == null) return;
        Debug.Log($"Enemy {_enemy.Enemy} unregistered.");
        _enemy = null;
    }

    // Helper methods to get opponents
    public PlayerPresenter GetPlayerPresenter() => _player;
    public EnemyPresenter GetEnemyPresenter() => _enemy;
    public Player GetPlayer() => _player.Player;
    public Enemy GetEnemy() => _enemy.Enemy;

    // Get all active opponent models
    public List<Opponent> GetActiveOpponents() {
        var list = new List<Opponent>();
        if (_player != null) list.Add(_player.Player);
        if (_enemy != null) list.Add(_enemy.Enemy);
        return list;
    }
}