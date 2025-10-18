using System;
using System.Collections.Generic;
using System.Linq;

public class Deck : CardContainer {
   
    private readonly List<Card> orderedCards = new List<Card>();

    public Deck(int maxSize = DefaultSize) : base(maxSize) { }

    public new IReadOnlyList<Card> Cards => orderedCards.AsReadOnly();

    public Card PeekAt(int position) {
        if (position < 0 || position >= orderedCards.Count)
            return null;
        return orderedCards[position];
    }

    public Card Draw() {
        if (IsEmpty) return null;

        var card = orderedCards[^1]; // Остання карта
        Remove(card);
        return card;
    }

    public Card DrawFromBottom() {
        if (IsEmpty) return null;

        var card = orderedCards[0];
        Remove(card);
        return card;
    }

    public override void Shuffle() {
        if (IsEmpty) return;

        // Алгоритм Fisher-Yates shuffle
        var random = new Random();
        for (int i = orderedCards.Count - 1; i > 0; i--) {
            int randomIndex = random.Next(i + 1);
            (orderedCards[i], orderedCards[randomIndex]) = (orderedCards[randomIndex], orderedCards[i]);
        }
        base.Shuffle();
    }

    public void AddToTop(Card card) {
        if (Add(card)) {
            orderedCards.Add(card); 
        }
    }

    public void AddToBottom(Card card) {
        if (Add(card)) {
            orderedCards.Insert(0, card);
        }
    }

    public void InsertAt(int position, Card card) {
        if (position < 0 || position > orderedCards.Count)
            throw new ArgumentOutOfRangeException(nameof(position));

        if (Add(card)) {
            orderedCards.Insert(position, card);
        }
    }

    public override bool Add(Card card) {
        if (base.Add(card)) {
            orderedCards.Add(card);
            return true;
        }
        return false;
    }

    public override bool Remove(Card card) {
        if (base.Remove(card)) {
            orderedCards.Remove(card);
            return true;
        }
        return false;
    }

    public override void Clear() {
        base.Clear();
        orderedCards.Clear();
    }

    public override Card GetRandom() {
        if (IsEmpty) return null;
        var random = new Random();
        return orderedCards[random.Next(orderedCards.Count)];
    }

    public Card PeekTop() => IsEmpty ? null : orderedCards[^1];
    public Card PeekBottom() => IsEmpty ? null : orderedCards[0];

    public List<Card> PeekTopCards(int count) {
        count = Math.Min(count, orderedCards.Count);
        return orderedCards.TakeLast(count).ToList();
    }
    public int GetCardPosition(string cardInstanceId) {
        for (int i = 0; i < orderedCards.Count; i++) {
            if (orderedCards[i].InstanceId == cardInstanceId)
                return i;
        }
        return -1;
    }

    public void MoveCard(int fromIndex, int toIndex) {
        if (fromIndex < 0 || fromIndex >= orderedCards.Count ||
            toIndex < 0 || toIndex >= orderedCards.Count)
            return;

        var card = orderedCards[fromIndex];
        orderedCards.RemoveAt(fromIndex);
        orderedCards.Insert(toIndex, card);
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
