using System;
using System.Collections.Generic;
using System.Linq;

public class Deck : CardContainer
{
    private readonly List<string> _cardOrder;

    public Deck(int maxSize = DefaultSize) : base(maxSize)
    {
        _cardOrder = new List<string>(maxSize);
    }

    public new IReadOnlyList<Card> Cards => _cardOrder.Select(id => _cards[id]).ToList();
    public IReadOnlyList<string> CardIds => _cardOrder;

    public Card this[int index] => PeekAt(index);

    public Card PeekAt(int position)
    {
        if (position < 0 || position >= _cardOrder.Count)
            return null;

        return _cards.GetValueOrDefault(_cardOrder[position]);
    }

    public Card Draw() => DrawFromPosition(_cardOrder.Count - 1);

    public Card DrawFromBottom() => DrawFromPosition(0);

    public Card DrawAt(int position) => DrawFromPosition(position);

    private Card DrawFromPosition(int position)
    {
        if (position < 0 || position >= _cardOrder.Count)
            return null;

        var cardId = _cardOrder[position];
        if (!_cards.TryGetValue(cardId, out var card))
            return null;

        _cards.Remove(cardId);
        _cardOrder.RemoveAt(position);

        if (card.CurrentContainer == this)
            card.SetContainer(null);

        RaiseCardRemoved(card);
        return card;
    }

    public List<Card> DrawMultiple(int count)
    {
        count = Math.Min(count, _cardOrder.Count);
        var drawnCards = new List<Card>(count);

        for (int i = 0; i < count; i++)
        {
            var card = DrawFromPosition(_cardOrder.Count - 1);
            if (card != null)
                drawnCards.Add(card);
        }

        return drawnCards;
    }

    public override void Shuffle()
    {
        if (IsEmpty) return;

        // Fisher-Yates shuffle
        var random = RandomService.SystemRandom;
        for (int i = _cardOrder.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (_cardOrder[i], _cardOrder[j]) = (_cardOrder[j], _cardOrder[i]);
        }

        RaiseChanged();
    }

    public void ShuffleRange(int startIndex, int count)
    {
        if (startIndex < 0 || startIndex >= _cardOrder.Count || count <= 1)
            return;

        int endIndex = Math.Min(startIndex + count, _cardOrder.Count);
        var random = RandomService.SystemRandom;

        for (int i = endIndex - 1; i > startIndex; i--)
        {
            int j = random.Next(startIndex, i + 1);
            (_cardOrder[i], _cardOrder[j]) = (_cardOrder[j], _cardOrder[i]);
        }

        RaiseChanged();
    }

    public bool AddToTop(Card card) => InsertAt(_cardOrder.Count, card);

    public bool AddToBottom(Card card) => InsertAt(0, card);

    public bool InsertAt(int position, Card card)
    {
        if (!CanAddCard(card))
            return false;

        PrepareCardForAdd(card);
        _cards[card.InstanceId] = card;

        position = Math.Clamp(position, 0, _cardOrder.Count);
        _cardOrder.Insert(position, card.InstanceId);

        RaiseCardAdded(card);
        return true;
    }

    public int AddRangeToTop(IEnumerable<Card> cards) => InsertRangeAt(_cardOrder.Count, cards);

    public int AddRangeToBottom(IEnumerable<Card> cards) => InsertRangeAt(0, cards);

    public int InsertRangeAt(int position, IEnumerable<Card> cards)
    {
        if (cards == null) return 0;

        var addedCards = new List<Card>();
        var insertPosition = Math.Clamp(position, 0, _cardOrder.Count);

        foreach (var card in cards)
        {
            if (IsFull) break;
            if (!CanAddCard(card)) continue;

            PrepareCardForAdd(card);
            _cards[card.InstanceId] = card;
            _cardOrder.Insert(insertPosition, card.InstanceId);
            addedCards.Add(card);

            // For top insertion, keep position at end
            if (position >= _cardOrder.Count - 1)
                insertPosition = _cardOrder.Count;
            else
                insertPosition++;
        }

        RaiseCardsAdded(addedCards);
        return addedCards.Count;
    }

    public override bool Add(Card card) => AddToTop(card);

    public override int AddRange(IEnumerable<Card> newCards) => AddRangeToTop(newCards);

    public override bool Remove(string cardInstanceId)
    {
        if (string.IsNullOrEmpty(cardInstanceId))
            return false;

        if (!_cards.TryGetValue(cardInstanceId, out var card))
            return false;

        _cards.Remove(cardInstanceId);
        _cardOrder.Remove(cardInstanceId);

        if (card.CurrentContainer == this)
            card.SetContainer(null);

        RaiseCardRemoved(card);
        return true;
    }

    public override void Clear()
    {
        if (IsEmpty) return;

        var removedCards = _cards.Values.ToList();

        foreach (var card in removedCards)
        {
            if (card.CurrentContainer == this)
                card.SetContainer(null);
        }

        _cards.Clear();
        _cardOrder.Clear();

        RaiseCardsRemoved(removedCards);
    }

    public override Card GetRandom()
    {
        if (IsEmpty) return null;
        var random = RandomService.SystemRandom;
        var randomId = _cardOrder[random.Next(_cardOrder.Count)];
        return _cards[randomId];
    }

    public Card PeekTop() => PeekAt(_cardOrder.Count - 1);
    
    public Card PeekBottom() => PeekAt(0);

    public IReadOnlyList<Card> PeekTopCards(int count)
    {
        count = Math.Min(count, _cardOrder.Count);
        return _cardOrder
            .Skip(_cardOrder.Count - count)
            .Select(id => _cards[id])
            .ToList();
    }

    public IReadOnlyList<Card> PeekBottomCards(int count)
    {
        count = Math.Min(count, _cardOrder.Count);
        return _cardOrder
            .Take(count)
            .Select(id => _cards[id])
            .ToList();
    }

    public int GetPosition(string cardInstanceId) => _cardOrder.IndexOf(cardInstanceId);

    public void MoveCard(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _cardOrder.Count ||
            toIndex < 0 || toIndex >= _cardOrder.Count || 
            fromIndex == toIndex)
            return;

        var cardId = _cardOrder[fromIndex];
        _cardOrder.RemoveAt(fromIndex);
        _cardOrder.Insert(toIndex, cardId);

        RaiseChanged();
    }

    public void MoveToTop(string cardInstanceId) => MoveToPosition(cardInstanceId, _cardOrder.Count - 1);

    public void MoveToBottom(string cardInstanceId) => MoveToPosition(cardInstanceId, 0);

    private void MoveToPosition(string cardInstanceId, int targetPosition)
    {
        var currentIndex = _cardOrder.IndexOf(cardInstanceId);
        if (currentIndex < 0 || currentIndex == targetPosition)
            return;

        _cardOrder.RemoveAt(currentIndex);

        if (currentIndex < targetPosition)
            targetPosition--;

        _cardOrder.Insert(targetPosition, cardInstanceId);
        RaiseChanged();
    }

    public void Reverse()
    {
        _cardOrder.Reverse();
        RaiseChanged();
    }

    public void Sort(Comparison<Card> comparison)
    {
        var sortedCards = _cardOrder
            .Select(id => _cards[id])
            .ToList();
            
        sortedCards.Sort(comparison);
        
        _cardOrder.Clear();
        _cardOrder.AddRange(sortedCards.Select(c => c.InstanceId));

        RaiseChanged();
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
