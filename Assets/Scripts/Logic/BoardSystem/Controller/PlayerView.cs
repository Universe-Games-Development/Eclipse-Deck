using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerView : OpponentView {
    [SerializeField] public SelectorView SelectionDisplay;
    [SerializeField] private CardHandView cardHandView;

    public event Action<string> OnCardClicked;

    public event Action OnCardDrawRequest;
    public event Action OnCardTestRemoveRequest;

    [SerializeField] Button addCardButton;
    [SerializeField] Button removeCardButton;

    private void Awake() {
        cardHandView.OnCardClicked += HandleCardHandClicked;
        addCardButton?.onClick.AddListener(HandleDrawCardButtonClicked);

        removeCardButton?.onClick.AddListener(HandleRemoveCardButtonClicked);
    }

    private void HandleDrawCardButtonClicked() {
        OnCardDrawRequest?.Invoke();
    }

    private void HandleRemoveCardButtonClicked() {
        OnCardTestRemoveRequest?.Invoke();
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
