public class Deck : CardContainer {
    public Deck(int maxSize = DefaultSize) : base(maxSize) { }
    public Card Draw() {
        if (IsEmpty) return null;
        var card = cards[^1]; 
        Remove(card);  
        return card;
    }
}

public class DeckPresenter : UnitPresenter {
    private DeckView deckView;

    public Deck Deck { get; private set; }

    public DeckPresenter(Deck deckModel, DeckView deckView) : base (deckModel, deckView) {
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

public struct DiscardCardEvent : IEvent {
    public readonly Card card;
    public readonly Opponent owner;

    public DiscardCardEvent(Card card, Opponent owner) {
        this.card = card;
        this.owner = owner;
    }
}

public struct ExileCardEvent : IEvent {
    public Card card;
    public ExileCardEvent(Card card) {
        this.card = card;
    }
}
