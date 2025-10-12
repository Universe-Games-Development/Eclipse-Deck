using System.Collections.Generic;

public class DeckPresenter : UnitPresenter {
    private DeckView deckView;

    public Deck Deck { get; private set; }

    public DeckPresenter(Deck deckModel, DeckView deckView) : base (deckModel, deckView) {
        Deck = deckModel;
        this.deckView = deckView;
    }
}
