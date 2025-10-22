using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;


public class CardContainer : UnitModel {
    protected readonly Dictionary<string, Card> _cards;
    private readonly int _maxSize;
    protected const int DefaultSize = 127;
    [Inject] protected IRandomService RandomService;

    public CardContainer(int maxSize = DefaultSize) {
        _maxSize = maxSize;
        _cards = new Dictionary<string, Card>(maxSize);
    }

    public int Count => _cards.Count;
    public bool IsEmpty => _cards.Count == 0;
    public bool IsFull => _cards.Count >= _maxSize;

    public IReadOnlyCollection<Card> Cards => _cards.Values;

    // Events with better naming
    public event Action<Card> CardAdded;
    public event Action<IReadOnlyList<Card>> CardsAdded;
    public event Action<Card> CardRemoved;
    public event Action<IReadOnlyList<Card>> CardsRemoved;
    public event Action<CardContainer> Changed;

    protected virtual bool CanAddCard(Card card) {
        if (card == null || string.IsNullOrEmpty(card.InstanceId))
            return false;

        return !IsFull && !_cards.ContainsKey(card.InstanceId);
    }

    protected virtual void PrepareCardForAdd(Card card) {
        card.CurrentContainer?.Remove(card);
        card.SetContainer(this);
    }

    protected void RaiseCardAdded(Card card) {
        CardAdded?.Invoke(card);
        Changed?.Invoke(this);
    }

    protected void RaiseChanged() {
        Changed?.Invoke(this);
    }

    protected void RaiseCardsAdded(IReadOnlyList<Card> cards) {
        if (cards.Count == 0) return;

        if (cards.Count == 1)
            CardAdded?.Invoke(cards[0]);
        else
            CardsAdded?.Invoke(cards);

        Changed?.Invoke(this);
    }

    protected void RaiseCardRemoved(Card card) {
        CardRemoved?.Invoke(card);
        Changed?.Invoke(this);
    }

    protected void RaiseCardsRemoved(IReadOnlyList<Card> cards) {
        if (cards.Count == 0) return;

        if (cards.Count == 1)
            CardRemoved?.Invoke(cards[0]);
        else
            CardsRemoved?.Invoke(cards);

        Changed?.Invoke(this);
    }

    public virtual bool Add(Card card) {
        if (!CanAddCard(card))
            return false;

        PrepareCardForAdd(card);
        _cards[card.InstanceId] = card;
        RaiseCardAdded(card);

        return true;
    }

    public virtual int AddRange(IEnumerable<Card> newCards) {
        if (newCards == null)
            return 0;

        var addedCards = new List<Card>();

        foreach (var card in newCards) {
            if (IsFull) break;
            if (!CanAddCard(card)) continue;

            PrepareCardForAdd(card);
            _cards[card.InstanceId] = card;
            addedCards.Add(card);
        }

        RaiseCardsAdded(addedCards);
        return addedCards.Count;
    }

    public virtual bool Remove(Card card) {
        return card != null && Remove(card.InstanceId);
    }

    public virtual bool Remove(string cardInstanceId) {
        if (string.IsNullOrEmpty(cardInstanceId))
            return false;

        if (!_cards.TryGetValue(cardInstanceId, out var card))
            return false;

        _cards.Remove(cardInstanceId);

        if (card.CurrentContainer == this)
            card.SetContainer(null);

        RaiseCardRemoved(card);
        return true;
    }

    public virtual int RemoveRange(IEnumerable<Card> cardsToRemove) {
        if (cardsToRemove == null)
            return 0;

        return RemoveCardsByIds(cardsToRemove.Where(c => c != null).Select(c => c.InstanceId));
    }

    public virtual int RemoveRange(IEnumerable<string> cardInstanceIds) {
        return cardInstanceIds == null ? 0 : RemoveCardsByIds(cardInstanceIds);
    }

    private int RemoveCardsByIds(IEnumerable<string> ids) {
        var removedCards = new List<Card>();

        foreach (var id in ids) {
            if (string.IsNullOrEmpty(id)) continue;

            if (_cards.TryGetValue(id, out var card) && _cards.Remove(id)) {
                if (card.CurrentContainer == this)
                    card.SetContainer(null);

                removedCards.Add(card);
            }
        }

        RaiseCardsRemoved(removedCards);
        return removedCards.Count;
    }

    public virtual void Clear() {
        if (IsEmpty) return;

        var removedCards = _cards.Values.ToList();

        foreach (var card in removedCards) {
            if (card.CurrentContainer == this)
                card.SetContainer(null);
        }

        _cards.Clear();
        RaiseCardsRemoved(removedCards);
    }

    public bool Contains(Card card) => card != null && _cards.ContainsKey(card.InstanceId);

    public bool Contains(string cardInstanceId) => !string.IsNullOrEmpty(cardInstanceId) && _cards.ContainsKey(cardInstanceId);

    public bool TryGetCard(string cardInstanceId, out Card card) => _cards.TryGetValue(cardInstanceId, out card);

    public Card GetCard(string cardInstanceId) => _cards.GetValueOrDefault(cardInstanceId);

    public virtual void Shuffle() {
        Changed?.Invoke(this);
    }

    public virtual Card GetRandom() {
        if (IsEmpty) return null;

        var random = RandomService.SystemRandom;
        var index = random.Next(_cards.Count);
        return _cards.Values.ElementAt(index);
    }

    public List<Card> GetCards(IEnumerable<string> instanceIds) {
        return instanceIds
            .Where(id => _cards.ContainsKey(id))
            .Select(id => _cards[id])
            .ToList();
    }

    public bool ContainsAll(IEnumerable<string> instanceIds) => instanceIds.All(_cards.ContainsKey);

    public bool ContainsAny(IEnumerable<string> instanceIds) => instanceIds.Any(_cards.ContainsKey);
}
