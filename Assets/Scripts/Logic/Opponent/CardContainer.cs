using System;
using System.Collections.Generic;
using System.Linq;


public class CardContainer : UnitModel {
    protected readonly Dictionary<string, Card> _cards = new Dictionary<string, Card>();
    protected readonly int _maxSize;
    protected const int DefaultSize = 127;

    public CardContainer(int maxSize = DefaultSize) {
        _maxSize = maxSize;
    }

    public int Count => _cards.Count;
    public bool IsEmpty => _cards.Count == 0;
    public bool IsFull => Count >= _maxSize;

    public IReadOnlyCollection<Card> Cards => _cards.Values;
    public IReadOnlyList<Card> CardsList => _cards.Values.ToList().AsReadOnly();

    public Action<Card> OnCardAdded;
    public Action<IReadOnlyList<Card>> OnCardsAdded;
    public Action<Card> OnCardRemoved;
    public Action<IReadOnlyList<Card>> OnCardsRemoved;
    public Action<CardContainer> OnChanged;

    protected virtual bool CanAddCard(Card card) {
        return card != null && !IsFull && !string.IsNullOrEmpty(card.InstanceId) && !_cards.ContainsKey(card.InstanceId);
    }

    protected virtual void PrepareCardForAdd(Card card) {
        card.CurrentContainer?.Remove(card);
        card.SetContainer(this);
    }

    protected virtual void NotifyChanges(Card singleCard = null, IReadOnlyList<Card> multipleCards = null) {
        if (singleCard != null) {
            OnCardAdded?.Invoke(singleCard);
        } else if (multipleCards != null && multipleCards.Count > 0) {
            if (multipleCards.Count == 1)
                OnCardAdded?.Invoke(multipleCards[0]);
            else
                OnCardsAdded?.Invoke(multipleCards);
        }

        OnChanged?.Invoke(this);
    }

    protected virtual void NotifyRemovals(Card singleCard = null, IReadOnlyList<Card> multipleCards = null) {
        if (singleCard != null) {
            OnCardRemoved?.Invoke(singleCard);
        } else if (multipleCards != null && multipleCards.Count > 0) {
            if (multipleCards.Count == 1)
                OnCardRemoved?.Invoke(multipleCards[0]);
            else
                OnCardsRemoved?.Invoke(multipleCards);
        }

        OnChanged?.Invoke(this);
    }

    public virtual bool Add(Card card) {
        if (!CanAddCard(card))
            return false;

        PrepareCardForAdd(card);
        _cards[card.InstanceId] = card;

        NotifyChanges(singleCard: card);
        return true;
    }

    public virtual bool AddRange(IEnumerable<Card> newCards) {
        if (newCards == null)
            return false;

        var addedCards = new List<Card>();
        foreach (var card in newCards) {
            if (IsFull) break;
            if (!CanAddCard(card)) continue;

            PrepareCardForAdd(card);
            _cards[card.InstanceId] = card;
            addedCards.Add(card);
        }

        if (addedCards.Count == 0)
            return false;

        NotifyChanges(multipleCards: addedCards);
        return true;
    }

    public virtual bool Remove(Card card) {
        return card != null && Remove(card.InstanceId);
    }

    public virtual bool Remove(string cardInstanceId) {
        if (string.IsNullOrEmpty(cardInstanceId) || !_cards.TryGetValue(cardInstanceId, out var card))
            return false;

        _cards.Remove(cardInstanceId);

        if (card.CurrentContainer == this)
            card.SetContainer(null);

        NotifyRemovals(singleCard: card);
        return true;
    }

    public virtual bool RemoveRange(IEnumerable<Card> cardsToRemove) {
        if (cardsToRemove == null)
            return false;

        var removedCards = RemoveCardsByIds(cardsToRemove.Select(c => c?.InstanceId));
        return removedCards.Count > 0;
    }

    public virtual bool RemoveRange(IEnumerable<string> cardInstanceIds) {
        if (cardInstanceIds == null)
            return false;

        var removedCards = RemoveCardsByIds(cardInstanceIds);
        return removedCards.Count > 0;
    }

    private List<Card> RemoveCardsByIds(IEnumerable<string> ids) {
        var removedCards = new List<Card>();
        foreach (var id in ids) {
            if (string.IsNullOrEmpty(id) || !_cards.TryGetValue(id, out var card))
                continue;

            if (_cards.Remove(id)) {
                if (card.CurrentContainer == this)
                    card.SetContainer(null);
                removedCards.Add(card);
            }
        }

        if (removedCards.Count > 0)
            NotifyRemovals(multipleCards: removedCards);

        return removedCards;
    }

    public virtual void Clear() {
        if (IsEmpty)
            return;

        var removedCards = _cards.Values.ToList();

        foreach (var card in removedCards) {
            if (card.CurrentContainer == this)
                card.SetContainer(null);
        }

        _cards.Clear();
        NotifyRemovals(multipleCards: removedCards);
    }

    public virtual bool Contains(Card card) {
        return card != null && _cards.ContainsKey(card.InstanceId);
    }

    public virtual bool Contains(string cardInstanceId) {
        return !string.IsNullOrEmpty(cardInstanceId) && _cards.ContainsKey(cardInstanceId);
    }

    public bool TryGetCardById(string cardInstanceId, out Card card) {
        return _cards.TryGetValue(cardInstanceId, out card);
    }

    public Card GetCardById(string cardInstanceId) {
        return _cards.TryGetValue(cardInstanceId, out var card) ? card : null;
    }

    public virtual void Shuffle() {
        // Базовий контейнер не має порядку, тому просто сповіщаємо про зміни
        OnChanged?.Invoke(this);
    }

    public virtual Card GetRandom() {
        if (IsEmpty)
            return null;

        var random = new Random();
        var values = _cards.Values.ToList();
        return values[random.Next(values.Count)];
    }

    public List<Card> GetCardsByIds(IEnumerable<string> instanceIds) {
        var result = new List<Card>();
        foreach (var id in instanceIds) {
            if (_cards.TryGetValue(id, out var card))
                result.Add(card);
        }
        return result;
    }

    public bool ContainsAll(IEnumerable<string> instanceIds) {
        return instanceIds.All(id => _cards.ContainsKey(id));
    }

    public bool ContainsAny(IEnumerable<string> instanceIds) {
        return instanceIds.Any(id => _cards.ContainsKey(id));
    }
}
