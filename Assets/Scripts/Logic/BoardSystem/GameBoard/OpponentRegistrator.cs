using System;
using System.Collections.Generic;
using UnityEngine;

public class OpponentRegistrator {
    public Action<PlayerPresenter, EnemyPresenter> OnMatchSetup;

    // Properties
    public PlayerPresenter PlayerPresenter;
    public EnemyPresenter EnemyPresenter;

    // Event for game start when both opponents are ready
    public bool IsMatchReady => PlayerPresenter != null && EnemyPresenter != null;

    public void RegisterOpponent(CharacterPresenter opponentPresenter) {
        if (opponentPresenter == null) {
            Debug.LogError("Attempted to register null opponent");
            return;
        }

        switch (opponentPresenter) {
            case PlayerPresenter playerPresenter:
                PlayerPresenter = playerPresenter;
                Debug.Log($"Player {PlayerPresenter} registered");
                break;
            case EnemyPresenter enemyPresenter:
                EnemyPresenter = enemyPresenter;
                Debug.Log($"Enemy {EnemyPresenter} registered");
                break;
            default:
                Debug.LogWarning($"Unknown opponent type: {opponentPresenter.GetType()}");
                return;
        }

        Opponent opponent = opponentPresenter.Opponent;
        opponent.OnDefeat += UnregisterOpponent;

        // Check if match is ready after registration
        CheckAndTriggerMatchSetup();
    }

    private void UnregisterOpponent(Opponent opponent) {
        if (opponent == null) return;
        opponent.OnDefeat -= UnregisterOpponent;
        switch (opponent) {
            case Player player:
                UnregisterPlayer();
                break;
            case Enemy enemy:
                UnregisterEnemy();
                break;
            default:
                Debug.LogWarning($"Unknown opponent type being unregistered: {opponent.GetType()}");
                break;
        }
    }

    // Helper to check if both opponents are registered and trigger match setup
    private void CheckAndTriggerMatchSetup() {
        if (IsMatchReady) {
            Debug.Log("Both opponents registered. Match setup complete.");
            OnMatchSetup?.Invoke(PlayerPresenter, EnemyPresenter);
        }
    }

    // Unregister player
    public void UnregisterPlayer() {
        if (PlayerPresenter == null) return;

        Debug.Log($"Player {PlayerPresenter} unregistered.");
        PlayerPresenter = null;
    }

    // Unregister enemy
    public void UnregisterEnemy() {
        if (EnemyPresenter == null) return;

        Debug.Log($"Enemy {EnemyPresenter} unregistered.");
        EnemyPresenter = null;
    }

    // Helper methods to get opponents - with safety checks
    public Player GetPlayer() {
        return PlayerPresenter?.GetModel() as Player;
    }

    public Enemy GetEnemy() {
        return EnemyPresenter?.GetModel() as Enemy;
    }

    // Get all active opponent models
    public List<Opponent> GetActiveOpponents() {
        var list = new List<Opponent>();

        Player player = GetPlayer();
        if (player != null) list.Add(player);

        Enemy enemy = GetEnemy();
        if (enemy != null) list.Add(enemy);

        return list;
    }
}