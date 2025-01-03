using System;
using System.Collections.Generic;

public class CardHand {
    private const int DEFAULT_SIZE = 3;

    private List<Card> cardsInHand = new();

    public event Action<Card> OnCardAdd;
    public event Action<Card> OnCardRemove;

    private readonly int maxHandSize;

    public CardHand(int maxHandSize = DEFAULT_SIZE) {
        this.maxHandSize = maxHandSize;
    }

    public void AddCard(Card card) {
        if (cardsInHand.Count < maxHandSize) {
            cardsInHand.Add(card);
            OnCardAdd?.Invoke(card);
        }
    }

    public void RemoveCard(Card card) {
        if (cardsInHand.Contains(card)) {
            cardsInHand.Remove(card);
            OnCardRemove?.Invoke(card);
        }
    }

    public Card GetCard(int index) {
        if (index >= 0 && index < cardsInHand.Count) {
            return cardsInHand[index];
        }
        return null;
    }

    public Card GetCardByID(string cardID) {
        return cardsInHand.Find(card => card.Id == cardID);
    }

    public Card GetRandomCard() {
        if (cardsInHand.Count > 0) {
            return cardsInHand[new Random().Next(cardsInHand.Count)];
        }
        return null;
    }
}
