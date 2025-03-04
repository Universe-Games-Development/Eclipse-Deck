using System;
using System.ComponentModel;
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

        if (turnManager.EndTurnRequest(player)) {
            turnButtonView.SetInteractive(false);
        }
    }

    private void OnDestroy() {
        if (turnButtonView != null)
            turnButtonView.OnTurnButtonClicked -= HandleTurnButtonClicked;
        if (turnManager != null)
            turnManager.OnOpponentChanged -= HandleInteraction;
    }
}
