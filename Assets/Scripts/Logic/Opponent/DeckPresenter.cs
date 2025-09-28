using ModestTree;
using System.Collections.Generic;
using Unity.VisualScripting;
using Zenject;

public class DeckPresenter : UnitPresenter {
    private DeckView deckView;
    [Inject] private CardProvider _cardProvider;
    [Inject] private ICardFactory _cardFactory;

    public Deck Deck { get; private set; }

    public DeckPresenter(Deck deckModel, DeckView deckView) : base (deckModel, deckView) {
        Deck = deckModel;
        this.deckView = deckView;
    }

    public void FillDeckWithRandomCards(int amount) {
        Deck deck = Deck;
        var cards = GenerateRandomCards(amount);
        deck.AddRange(cards);
    }

    public List<Card> GenerateRandomCards(int amount) {
        CardCollection collection = new();
        List<CardData> unlockedCards = _cardProvider.GetRandomUnlockedCards(amount);

        if (unlockedCards.IsEmpty())
            return new List<Card>();

        // випадковий набір карт
        for (int i = 0; i < amount; i++) {
            var randomIndex = UnityEngine.Random.Range(0, unlockedCards.Count);
            var randomCard = unlockedCards[randomIndex];
            collection.AddCardToCollection(randomCard);
        }

        // створення інстансів
        List<Card> cards = new();
        foreach (var entry in collection.cardEntries) {
            for (int i = 0; i < entry.Value; i++) {
                Card newCard = _cardFactory.CreateCard(entry.Key);
                if (newCard != null)
                    cards.Add(newCard);
            }
        }
        return cards;
    }

    public List<Card> DrawCards(int drawAmount) {
        return Deck.DrawCards(drawAmount);
    }
}
