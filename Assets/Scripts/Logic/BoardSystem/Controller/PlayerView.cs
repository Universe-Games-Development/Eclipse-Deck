using System;
using UnityEngine;
using Zenject;

public class PlayerView : OpponentView {
    [SerializeField] private SelectorView selectionDisplay;
    [SerializeField] private CardHandView cardHandView;

    public event Action<string> OnCardClicked;

    private void Awake() {
        cardHandView.OnCardClicked += HandleCardHandClicked;
    }

    private void HandleCardHandClicked(string cardId) {
        OnCardClicked?.Invoke(cardId);
    }

    public void SetInteractableHand(bool isEnabled) {
        cardHandView.SetInteractable(isEnabled);
    }

    public void UpdateHandCardsOrder() {
        cardHandView.UpdateCardPositions();
    }

    private void OnDestroy() {
        cardHandView.OnCardClicked -= HandleCardHandClicked;
    }
}
