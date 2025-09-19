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