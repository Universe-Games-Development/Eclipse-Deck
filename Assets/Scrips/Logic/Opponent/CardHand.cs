using System;
using System.Collections.Generic;
using UnityEngine;

public class CardHand {
    public Action<Card> OnCardAdd;
    public Action<Card> OnCardRemove;
    public Action<Card> OnCardSelected;
    public Action<Card> OnCardDeselected;

    private const int DEFAULT_SIZE = 10;

    private List<Card> cardsInHand = new();
    private Dictionary<string, Card> idToCardMap = new();

    private Opponent owner;
    private readonly int maxHandSize;
    private GameEventBus gameEventBus;

    public Card SelectedCard { get; private set; }

    public CardHand(Opponent owner, GameEventBus gameEventBus, int maxHandSize = DEFAULT_SIZE) {
        this.owner = owner;
        this.maxHandSize = maxHandSize;
        this.gameEventBus = gameEventBus;
    }

    public bool AddCard(Card card) {
        if (card == null) {
            Debug.LogWarning("Received null card in card Hand!");
            return false;
        }
        if (cardsInHand.Count < maxHandSize) {
            card.ChangeState(CardState.InHand);

            var drawnCardData = new CardDrawnEvent(owner, card);
            gameEventBus.Raise(drawnCardData);

            idToCardMap[card.Id] = card;
            cardsInHand.Add(card);
            OnCardAdd?.Invoke(card);
            return true;
        } else {
            return false;
        }
    }

    public void RemoveCard(Card card) {
        if (cardsInHand.Contains(card)) {
            if (card == SelectedCard) {
                DeselectCurrentCard();
            }

            cardsInHand.Remove(card);
            idToCardMap.Remove(card.Id);

            var removedCardData = new CardPullEvent(owner, card);
            gameEventBus.Raise(removedCardData);

            OnCardRemove?.Invoke(card);
        }
    }

    public Card GetCard(int index) {
        return (index >= 0 && index < cardsInHand.Count) ? cardsInHand[index] : null;
    }


    public void SelectCard(Card newCard) {
        if (newCard == null) return;
        if (SelectedCard == newCard) return;

        DeselectCurrentCard();

        SelectedCard = newCard;
        Debug.Log("Selected: " + newCard.Data.Name);

        OnCardSelected?.Invoke(newCard); // Сповіщення про вибір карти
    }

    public void DeselectCurrentCard() {
        if (SelectedCard == null) return;

        OnCardDeselected?.Invoke(SelectedCard); // Сповіщення про скасування вибору

        SelectedCard = null;
    }



    public Card GetRandomCard() {
        if (cardsInHand.Count > 0) {
            return RandomUtil.GetRandomFromList(cardsInHand);
        }
        return null;
    }
}
