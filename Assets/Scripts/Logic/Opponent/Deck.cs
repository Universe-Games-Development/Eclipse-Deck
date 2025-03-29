using System.Collections.Generic;
using UnityEngine;

public class Deck {
    private Stack<Card> cards = new();
    private GameEventBus eventBus;
    private Opponent owner;
    private CardFactory cardFactory;
    private int emptyDrawAttempts = 0;
    private bool wasDeckEmpty = false;

    public Deck(Opponent owner, GameEventBus eventBus) {
        this.owner = owner;
        this.eventBus = eventBus;
        cardFactory = new CardFactory(owner, eventBus);
    }

    public void Initialize(CardCollection collection) {
        List<Card> collectionCards = cardFactory.CreateCardsFromCollection(collection);
        foreach (var card in collectionCards) {
            cards.Push(card);
            card.ChangeState(CardState.InDeck);
        }
        ShuffleDeck();
        // Скидаємо лічильник спроб та прапорець порожньої колоди при ініціалізації
        emptyDrawAttempts = 0;
        wasDeckEmpty = false;
    }

    public Card DrawCard() {
        if (cards.Count > 0) {
            Card drawnCard = cards.Pop();
            eventBus.Raise(new OnCardDrawn(drawnCard, owner));
            return drawnCard;
        } else {
            wasDeckEmpty = true;
            emptyDrawAttempts++;
            // Calculate damage (2^(n-1))
            int damage = 1 << (emptyDrawAttempts - 1);

            eventBus.Raise(new OnDeckEmptyDrawn(owner, emptyDrawAttempts, damage));

            return null;
        }
    }

    public void ShuffleDeck() {
        List<Card> tempCards = new(cards);
        cards.Clear();
        ShuffleList(tempCards);
        foreach (var card in tempCards) {
            cards.Push(card);
        }

        CheckDeckRefilled();
    }

    private void ShuffleList(List<Card> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public void AddCard(Card card) {
        cards.Push(card);

        CheckDeckRefilled();
    }

    private void CheckDeckRefilled() {
        if (wasDeckEmpty && cards.Count > 0) {
            // Deck was empty but refilled
            ResetEmptyDrawAttempts();
            eventBus.Raise(new OnDeckRefilled(owner));
        }
    }

    public void ResetEmptyDrawAttempts() {
        emptyDrawAttempts = 0;
        wasDeckEmpty = false;
    }

    public void ClearDeck() {
        cards.Clear();
        emptyDrawAttempts = 0;
        wasDeckEmpty = false;
    }

    public int GetCount() {
        return cards.Count;
    }

    public int GetEmptyDrawAttempts() {
        return emptyDrawAttempts;
    }
}

public class CardFactory {
    public GameEventBus eventBus;
    private Opponent owner;
    public CardFactory(Opponent owner, GameEventBus eventBus) {
        this.eventBus = eventBus;
        this.owner = owner;
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
        return cardData switch {
            CreatureCardData creatureData => new CreatureCard(creatureData, owner),
            SpellCardData spellData => new SpellCard(spellData, owner),
            SupportCardData supportData => new SupportCard(supportData, owner),
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
    public int attemptCount;
    public int damage;

    public OnDeckEmptyDrawn(Opponent owner, int attemptCount, int damage) {
        this.owner = owner;
        this.attemptCount = attemptCount;
        this.damage = damage;
    }
}

// Новий івент для повідомлення про те, що колода знову заповнена
public struct OnDeckRefilled : IEvent {
    public Opponent owner;

    public OnDeckRefilled(Opponent owner) {
        this.owner = owner;
    }
}