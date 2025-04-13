using System.Collections.Generic;
using System;
using UnityEngine;

public class CardHand {
    // Події, які сповіщають про зміни в моделі
    public event Action<Card> CardAdded;
    public event Action<Card> CardRemoved;
    public event Action<Card> OnCardSelected;
    public event Action<Card> OnCardDeselected;
    public event Action<bool> OnToggled;

    private const int DEFAULT_SIZE = 10;

    private List<Card> cardsInHand = new();

    private readonly int maxHandSize;

    public Card SelectedCard { get; private set; }

    public CardHand(int maxHandSize = DEFAULT_SIZE) {
        this.maxHandSize = maxHandSize;
    }

    public bool AddCard(Card card) {
        if (card == null) {
            Debug.LogWarning("Received null card in card Hand!");
            return false;
        }

        if (cardsInHand.Count < maxHandSize) {
            card.ChangeState(CardState.InHand);

            cardsInHand.Add(card);
            CardAdded?.Invoke(card);
            return true;
        } else {
            Debug.Log("Failed to add card to hand");
            return false;
        }
    }

    public bool RemoveCard(Card card) {
        if (cardsInHand.Contains(card)) {
            if (card == SelectedCard) {
                DeselectCurrentCard();
            }

            cardsInHand.Remove(card);

            CardRemoved?.Invoke(card);
            return true;
        }
        return false;
    }

    public void SelectCard(Card newCard) {
        if (newCard == null) return;
        if (SelectedCard == newCard) return;
        OnCardSelected?.Invoke(SelectedCard);
        DeselectCurrentCard();
        SelectedCard = newCard;
        Debug.Log("Selected: " + newCard.Data.Name);
    }

    public void DeselectCurrentCard() {
        if (SelectedCard == null) return;
        OnCardDeselected?.Invoke(SelectedCard);
        SelectedCard = null;
    }

    public Card GetRandomCard() {
        if (cardsInHand.Count > 0) {
            return cardsInHand.GetRandomElement();
        }
        return null;
    }

    public void ClearHand() {
        List<Card> cards = new List<Card>(cardsInHand);
        foreach (Card card in cards) {
            RemoveCard(card);
        }
    }

    public void SetInteraction(bool v) {
        OnToggled?.Invoke(v);
    }

    public int Count => cardsInHand.Count;
    public List<Card> Cards => new List<Card>(cardsInHand);
}