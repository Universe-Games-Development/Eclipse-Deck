using System;
using System.Collections.Generic;

public class CardContainer {
    protected readonly List<Card> cards = new List<Card>();
    private readonly int maxSize;
    protected const int DefaultSize = 127;

    public CardContainer(int maxSize = DefaultSize) {
        this.maxSize = maxSize;
    }

    public int Count => cards.Count;
    public bool IsEmpty => cards.Count == 0;
    public IReadOnlyList<Card> Cards => cards.AsReadOnly();

    public event Action<Card> OnCardAdded;
    public event Action<Card> OnCardRemoved;
    public event Action<CardContainer> OnChanged;

    public virtual bool Add(Card card) {
        if (card == null || Count >= maxSize)
            return false;

        card.CurrentContainer?.Remove(card);
        cards.Add(card);
        card.SetContainer(this);

        OnCardAdded?.Invoke(card);
        OnChanged?.Invoke(this);
        return true;
    }

    public virtual bool AddRange(IEnumerable<Card> newCards) {
        if (newCards == null)
            return false;
        foreach (var card in newCards) {
            if (!Add(card) && Count < maxSize)
                return false; // Stop on first failure
        }
        OnChanged?.Invoke(this);
        return true;
    }

    public virtual bool Remove(Card card) {
        if (card == null || !cards.Remove(card))
            return false;

        if (card.CurrentContainer == this)
            card.SetContainer(null);

        OnCardRemoved?.Invoke(card);
        OnChanged?.Invoke(this);
        return true;
    }

    public virtual void Clear() {
        var cardsCopy = new List<Card>(cards);
        cards.Clear();

        foreach (var card in cardsCopy) {
            if (card != null && card.CurrentContainer == this)
                card.SetContainer(null);
        }

        OnChanged?.Invoke(this);
    }

    public virtual bool Contains(Card card) => card != null && cards.Contains(card);

    public virtual void Shuffle() {
        // Реалізація Shuffle
        OnChanged?.Invoke(this);
    }

    public virtual Card GetRandom() {
        // Реалізація GetRandomCard
        return null;
    }
}