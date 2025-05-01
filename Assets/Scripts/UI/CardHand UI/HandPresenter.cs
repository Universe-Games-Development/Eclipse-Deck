using System;
using System.Collections.Generic;

public class HandPresenter : IDisposable {
    public CardHand CardHand { get; private set; }
    private readonly ICardHandView handView;
    private readonly Dictionary<string, CardPresenter> cardPresenters = new();

    public HandPresenter(CardHand cardHand, ICardHandView handView) {
        CardHand = cardHand ?? throw new ArgumentNullException(nameof(cardHand));
        this.handView = handView ?? throw new ArgumentNullException(nameof(handView));
        handView.Toggle(true);

        // Підписуємося на події моделі
        cardHand.CardAdded += OnCardAdded;
        cardHand.CardRemoved += OnCardRemoved;
        cardHand.OnCardSelected += SelectCard;
        cardHand.OnCardDeselected += DeselectCard;
        cardHand.OnToggled += handView.SetInteractable;
        // Підписуємося на події представлення
        handView.CardClicked += OnCardViewClicked;

        // Початкова синхронізація
        SyncViewWithModel();
    }

    private void SyncViewWithModel() {
        // Синхронізуємо представлення з поточним станом моделі
        foreach (var card in CardHand.Cards) {
            OnCardAdded(card);
        }

        // Встановлюємо вибрану карту, якщо вона є
        if (CardHand.SelectedCard != null) {
            SelectCard(CardHand.SelectedCard);
        }
    }

    // Handle Model events
    private void OnCardAdded(Card card) {
        ICardView cardView = handView.CreateCardView(card.Id);
        CardPresenter cardPresenter = new CardPresenter(card, cardView);
        cardPresenters.Add(card.Id, cardPresenter);
    }

    private void OnCardRemoved(Card card) {
        if (cardPresenters.TryGetValue(card.Id, out CardPresenter cardPresenter)) {
            handView.RemoveCardView(card.Id);
            cardPresenters.Remove(card.Id);
            cardPresenter.HandleRemoval();
        }
    }

    private void SelectCard(Card card) {
        if (cardPresenters.TryGetValue(card.Id, out CardPresenter cardPresenter)) {
            handView.SelectCardView(card.Id);
        }
    }

    private void DeselectCard(Card card) {
        if (cardPresenters.TryGetValue(card.Id, out CardPresenter cardPresenter)) {
            handView.DeselectCardView(card.Id);
        }
    }

    // Handle View events
    private void OnCardViewClicked(string id) {
        if (cardPresenters.TryGetValue(id, out CardPresenter cardPresenter)) {
            Card card = cardPresenter.Model;
            CardHand.SelectCard(card);
        }
    }

    public void Dispose() {
        // Відписуємося від подій
        CardHand.CardAdded -= OnCardAdded;
        CardHand.CardRemoved -= OnCardRemoved;
        handView.Cleanup();
    }

    internal void ClearHand() {
        throw new NotImplementedException();
    }
}


public class CardPresenter {
    public Card Model;
    public ICardView View;
    public CardPresenter(Card card, ICardView cardView) {
        Model = card;
        View = cardView;
    }

    private void UpdateCost(int from, int to) {
        View.UpdateCost(from, to);
    }

    private void UpdateAttack(int from, int to) {
        View.UpdateHealth(from, to);
    }

    private void UpdateHealth(int from, int to) {
        View.UpdateHealth(from, to);
    }

    private void UpdateDescriptionContent(Card card) {

    }

    internal void HandleRemoval() {
        throw new NotImplementedException();
    }
}
