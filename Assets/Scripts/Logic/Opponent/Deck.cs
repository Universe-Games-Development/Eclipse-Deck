using System.Collections.Generic;
using UnityEngine;

public class Deck {
    private Stack<Card> cards = new();
    private CardFactory cardFactory;

    public Deck() {
        cardFactory = new CardFactory();
    }

    public void Initialize(CardCollection collection) {
        List<Card> collectionCards = cardFactory.CreateCardsFromCollection(collection);
        foreach (var card in collectionCards) {
            cards.Push(card);
            card.ChangeState(CardState.InDeck);
        }
        ShuffleDeck();
    }

    public Card DrawCard() {
        if (cards.Count > 0) {
            Card drawnCard = cards.Pop();
            
            return drawnCard;
        }
        Debug.Log("Player doesn`t have more cards he need to take damage (TO DO Soon)");
        return null;
    }

    public void ShuffleDeck() {
        List<Card> tempCards = new(cards);
        cards.Clear();
        ShuffleList(tempCards);
        foreach (var card in tempCards) {
            cards.Push(card);
        }
    }

    private void ShuffleList(List<Card> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public void AddCard(Card card) {
        cards.Push(card);
    }


    public void ClearDeck() {
        cards.Clear();
    }

    public int GetCount() {
        return cards.Count;
    }
}

public class DeckPresenter {
    private DeckView deckView;

    public Deck Deck { get; private set; }

    public DeckPresenter(Deck deckModel, DeckView deckView) {
        Deck = deckModel;
        this.deckView = deckView;
    }
}

public class CardFactory {

    public List<Card> CreateCardsFromCollection(CardCollection collection) {
        List<Card> cards = new();
        foreach (var cardEntry in collection.cardEntries) {
            for (int i = 0; i < cardEntry.Value; i++) {
                CardData cardData = cardEntry.Key;
                Card newCard = CreateCard(cardData);
                if (newCard == null) continue;
                cards.Add(newCard);
            }
        }
        return cards;
    }

    public Card CreateCard(CardData cardData) {
        return cardData switch {
            CreatureCardData creatureData => new CreatureCard(creatureData),
            _ => null
        };
    }
}

public struct OnCardDrawn : IEvent {
    public Card card;
    public Opponent owner;

    public OnCardDrawn(Card card, Opponent owner) {
        this.card = card;
        this.owner = owner;
    }
}

public struct OnDeckEmptyDrawn : IEvent {
    public Opponent owner;

    public OnDeckEmptyDrawn(Opponent owner) {
        this.owner = owner;
    }
}

// Новий івент для повідомлення про те, що колода знову заповнена
public struct OnDeckRefilled : IEvent {
    public Opponent owner;

    public OnDeckRefilled(Opponent owner) {
        this.owner = owner;
    }
}