using System;
using System.Collections.Generic;

public class CardHand {
    private const int DEFAULT_SIZE = 3;

    private List<Card> cardsInHand = new();

    public event Action<Card> OnCardAdd;
    public event Action<Card> OnCardRemove;

    private Opponent owner;
    private readonly int maxHandSize;
    private IEventManager eventManager;

    public CardHand(Opponent owner, IEventManager eventManager, int maxHandSize = DEFAULT_SIZE) {
        this.owner = owner;
        this.maxHandSize = maxHandSize;
        this.eventManager = eventManager;
    }

    public void AddCard(Card card) {
        if (cardsInHand.Count < maxHandSize) {
            card.ChangeState(CardState.InHand);

            GameContext gameContext = new GameContext();
            gameContext.sourceCard = card;
            gameContext.activePlayer = owner;

            eventManager.TriggerEventAsync(EventType.ON_CARD_DRAWN, gameContext);
            cardsInHand.Add(card);
            OnCardAdd?.Invoke(card);
        } else {
            card.ChangeState(CardState.Discarded);
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
