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
    private IEventQueue eventManager;

    public Card SelectedCard { get; private set; }

    public CardHand(Opponent owner, IEventQueue eventQueue, int maxHandSize = DEFAULT_SIZE) {
        this.owner = owner;
        this.maxHandSize = maxHandSize;
        this.eventManager = eventQueue;
    }

    public bool AddCard(Card card) {
        if (card == null) {
            Debug.LogWarning("Received null card in card Hand!");
            return false;
        }
        if (cardsInHand.Count < maxHandSize) {
            card.ChangeState(CardState.InHand);

            TriggerCardEvent(EventType.ON_CARD_DRAWN, card);

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

            var removedCardData = new CardHandEventData(owner, card);
            eventManager.TriggerEvent(EventType.ON_CARD_REMOVED, removedCardData);

            OnCardRemove?.Invoke(card);
        }
    }

    private void TriggerCardEvent(EventType eventType, Card card) {
        var eventData = new CardHandEventData(owner, card);
        eventManager.TriggerEvent(eventType, eventData);
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
