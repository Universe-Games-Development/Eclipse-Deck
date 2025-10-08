using System;
using Zenject;

public interface ICardFactory {
    Card CreateCard(CardData cardData);
}

public class CardFactory: ICardFactory {
    [Inject] private DiContainer _container;

    public Card CreateCard(CardData cardData) {
        Card card = cardData switch {
            CreatureCardData creatureData => CreateCreatureCard(creatureData),
            SpellCardData spellData => CreateSpellCard(spellData),
            _ => throw new ArgumentException($"Unsupported card data type: {cardData.GetType()}")
        };
        card.Id = $"Card_{Guid.NewGuid()}";
        return card;
    }

    private CreatureCard CreateCreatureCard(CreatureCardData creatureData) {
        Health health = new(creatureData.Health);
        Attack attack = new(creatureData.Attack);
        return _container.Instantiate<CreatureCard>(new object[] { creatureData, health, attack });
    }

    private SpellCard CreateSpellCard(SpellCardData spellData) {
        return _container.Instantiate<SpellCard>(new object[] { spellData });
    }
}



