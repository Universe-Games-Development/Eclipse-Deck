using System;
using System.Collections.Generic;

public class CardHand {
    private const int DEFAULT_SIZE = 3;

    private List<Card> cardsInHand = new();

    public event Action<Card> OnCardAdd;
    public event Action<Card> OnCardRemove;

    private Opponent owner;
    private readonly int maxHandSize;
    private IEventQueue eventManager;

    public CardHand(Opponent owner, IEventQueue eventManager, int maxHandSize = DEFAULT_SIZE) {
        this.owner = owner;
        this.maxHandSize = maxHandSize;
        this.eventManager = eventManager;
    }

    public void AddCard(Card card) {
        if (cardsInHand.Count < maxHandSize) {
            card.ChangeState(CardState.InHand);

            var drawnCardData = new CardHandEventData(owner, card);
            eventManager.TriggerEvent(EventType.ON_CARD_DRAWN, drawnCardData);

            cardsInHand.Add(card);
            OnCardAdd?.Invoke(card);
        } else {
            card.ChangeState(CardState.Discarded);
        }
    }

    public void RemoveCard(Card card) {
        if (cardsInHand.Contains(card)) {
            cardsInHand.Remove(card);

            var removedCardData = new CardHandEventData(owner, card);
            eventManager.TriggerEvent(EventType.ON_CARD_REMOVED, removedCardData);

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

public class CardHandEventData {
    public Opponent Owner { get; private set; }
    public Card Card { get; private set; }

    public CardHandEventData(Opponent owner, Card card) {
        Owner = owner;
        Card = card;
    }
}