using System.Collections.Generic;
using Zenject;

public class Deck : CardContainer {
    public Card Draw() {
        if (cards.Count == 0) return null;
        var card = cards[^1];
        cards.RemoveAt(cards.Count - 1);
        return card;
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



public struct OnCardDrawn : IEvent {
    public Card card;
    public BoardPlayer owner;

    public OnCardDrawn(Card card, BoardPlayer owner) {
        this.card = card;
        this.owner = owner;
    }
}

public struct OnDeckEmptyDrawn : IEvent {
    public BoardPlayer owner;

    public OnDeckEmptyDrawn(BoardPlayer owner) {
        this.owner = owner;
    }
}

// Новий івент для повідомлення про те, що колода знову заповнена
public struct OnDeckRefilled : IEvent {
    public BoardPlayer owner;

    public OnDeckRefilled(BoardPlayer owner) {
        this.owner = owner;
    }
}