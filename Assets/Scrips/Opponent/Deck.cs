using System.Collections.Generic;
using System.Linq;

public class Deck {
    private Stack<Card> deck = new();
    private IEventManager eventManager;

    private Opponent owner;

    public Deck(Opponent owner, CardCollection collection, IEventManager eventManager) {
        this.owner = owner;
        this.eventManager = eventManager;
        InitializeDeck(collection);
    }

    private void InitializeDeck(CardCollection collection) {
        foreach (var cardEntry in collection.cardEntries) {
            for (int i = 0; i < cardEntry.quantity; i++) {
                Card newCard = new Card(cardEntry.cardSO, owner, eventManager);
                AddCard(newCard);
                newCard.ChangeState(CardState.InDeck);
            }
        }
        ShuffleDeck();
    }

    public Card DrawCard() {
        return deck.Count > 0 ? deck.Pop() : null;
    }

    public void ShuffleDeck() {
        var cards = deck.ToArray();
        deck.Clear();
        foreach (var card in cards.OrderBy(x => UnityEngine.Random.value)) {
            deck.Push(card);
        }
    }

    public void AddCard(Card card) {
        deck.Push(card);
    }

    public void CleanDeck() {
        deck.Clear();
    }

    public int GetCount() {
        return deck.Count();
    }
}
