using System.Collections.Generic;
using Zenject;

public interface ICardFactory {
    Card CreateCard(CardData cardData);
    List<Card> CreateCardsFromCollection(CardCollection collection);
}

public class CardFactory : ICardFactory {
    private readonly DiContainer _container;

    public CardFactory(DiContainer container) {
        _container = container;
    }

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
        Card card = cardData switch {
            CreatureCardData creatureData => _container.Instantiate<CreatureCard>(new object[] { creatureData }),
            SpellCardData spellData => _container.Instantiate<SpellCard>(new object[] { spellData }),
            _ => null
        };

        return card;
    }
}
