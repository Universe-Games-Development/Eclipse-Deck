using System;
using UnityEngine;
using Zenject;

public class TurnButtonController : MonoBehaviour {
    [Inject] private TurnManager turnManager;
    [Inject] private Player player;

    [SerializeField] private TurnButtonView turnButtonView;

    private void Awake() {
        if (turnButtonView == null) {
            Debug.LogError("TurnButtonView is not assigned!");
            return;
        }
        turnManager.OnOpponentChanged += HandleInteraction;
        turnButtonView.OnTurnButtonClicked += HandleTurnButtonClicked;
    }

    private void HandleInteraction(Opponent opponent) {
        turnButtonView.SetInteractive(opponent == player);
    }

    private void HandleTurnButtonClicked() {
        if (turnManager == null || player == null) {
            Debug.LogError("Dependencies not initialized!");
            return;
        }

        if (turnManager.ActiveOpponent == player) {
            turnManager.EndTurnRequest(player);
        } else {
            Debug.LogWarning("Not player turn!");
        }
    }

    private void OnDestroy() {
        if (turnButtonView != null)
            turnButtonView.OnTurnButtonClicked -= HandleTurnButtonClicked;
        if (turnManager != null)
            turnManager.OnOpponentChanged -= HandleInteraction;
    }
}
