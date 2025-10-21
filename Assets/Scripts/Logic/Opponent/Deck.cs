using System;
using System.Collections.Generic;
using System.Linq;
public class Deck : CardContainer {
    private readonly List<string> _cardOrder = new List<string>();

    public Deck(int maxSize = DefaultSize) : base(maxSize) { }

    public new IReadOnlyList<Card> Cards => _cardOrder.Select(id => _cards[id]).ToList().AsReadOnly();
    public IReadOnlyList<string> CardIds => _cardOrder.AsReadOnly();

    public Card PeekAt(int position) {
        if (position < 0 || position >= _cardOrder.Count)
            return null;

        var cardId = _cardOrder[position];
        return _cards.TryGetValue(cardId, out var card) ? card : null;
    }

    public Card Draw() => DrawFromPosition(_cardOrder.Count - 1);

    public Card DrawFromBottom() => DrawFromPosition(0);

    public Card DrawAt(int position) => DrawFromPosition(position);

    private Card DrawFromPosition(int position) {
        if (position < 0 || position >= _cardOrder.Count)
            return null;

        var cardId = _cardOrder[position];
        if (!_cards.TryGetValue(cardId, out var card))
            return null;

        RemoveCardFromDeck(cardId);
        return card;
    }

    public override void Shuffle() {
        if (IsEmpty) return;

        var random = new Random();
        for (int i = _cardOrder.Count - 1; i > 0; i--) {
            int randomIndex = random.Next(i + 1);
            (_cardOrder[i], _cardOrder[randomIndex]) = (_cardOrder[randomIndex], _cardOrder[i]);
        }

        OnChanged?.Invoke(this);
    }

    public void ShuffleRange(int startIndex, int count) {
        if (startIndex < 0 || startIndex >= _cardOrder.Count || count <= 0)
            return;

        int endIndex = Math.Min(startIndex + count, _cardOrder.Count);
        var random = new Random();

        for (int i = endIndex - 1; i > startIndex; i--) {
            int randomIndex = random.Next(startIndex, i + 1);
            (_cardOrder[i], _cardOrder[randomIndex]) = (_cardOrder[randomIndex], _cardOrder[i]);
        }

        OnChanged?.Invoke(this);
    }

    public bool AddToTop(Card card) => AddToPosition(card, _cardOrder.Count);

    public bool AddToBottom(Card card) => AddToPosition(card, 0);

    public void InsertAt(int position, Card card) => AddToPosition(card, position);

    private bool AddToPosition(Card card, int position) {
        if (!CanAddCard(card))
            return false;

        PrepareCardForAdd(card);
        _cards[card.InstanceId] = card;

        position = Math.Clamp(position, 0, _cardOrder.Count);
        _cardOrder.Insert(position, card.InstanceId);

        NotifyChanges(singleCard: card);
        return true;
    }

    public bool AddRangeToTop(IEnumerable<Card> cards) => AddRangeToPosition(cards, _cardOrder.Count);

    public bool AddRangeToBottom(IEnumerable<Card> cards) => AddRangeToPosition(cards, 0);

    private bool AddRangeToPosition(IEnumerable<Card> cards, int position) {
        if (cards == null) return false;

        var addedCards = new List<Card>();
        foreach (var card in cards) {
            if (IsFull) break;
            if (!CanAddCard(card)) continue;

            PrepareCardForAdd(card);
            _cards[card.InstanceId] = card;

            position = Math.Clamp(position, 0, _cardOrder.Count);
            _cardOrder.Insert(position, card.InstanceId);
            addedCards.Add(card);

            // Для додавання зверху збільшуємо позицію
            if (position == _cardOrder.Count)
                position = _cardOrder.Count;
        }

        bool isAdded = addedCards.Count > 0;
        if (isAdded) {
            NotifyChanges(multipleCards: addedCards);
        }
        return isAdded;
       
    }

    public override bool Add(Card card) => AddToTop(card);

    public override bool AddRange(IEnumerable<Card> newCards) => AddRangeToTop(newCards);

    private void RemoveCardFromDeck(string cardInstanceId) {
        if (_cards.Remove(cardInstanceId)) {
            _cardOrder.Remove(cardInstanceId);
        }
    }

    public override bool Remove(string cardInstanceId) {
        if (string.IsNullOrEmpty(cardInstanceId) || !_cards.TryGetValue(cardInstanceId, out var card))
            return false;

        RemoveCardFromDeck(cardInstanceId);

        if (card.CurrentContainer == this)
            card.SetContainer(null);

        NotifyRemovals(singleCard: card);
        return true;
    }

    public override void Clear() {
        if (IsEmpty) return;

        var removedCards = _cards.Values.ToList();

        foreach (var card in removedCards) {
            if (card.CurrentContainer == this)
                card.SetContainer(null);
        }

        _cards.Clear();
        _cardOrder.Clear();

        NotifyRemovals(multipleCards: removedCards);
    }

    public override Card GetRandom() {
        if (IsEmpty) return null;

        var random = new Random();
        var randomId = _cardOrder[random.Next(_cardOrder.Count)];
        return _cards[randomId];
    }

    public Card PeekTop() => PeekAt(_cardOrder.Count - 1);
    public Card PeekBottom() => PeekAt(0);

    public List<Card> PeekTopCards(int count) {
        count = Math.Min(count, _cardOrder.Count);
        return _cardOrder.TakeLast(count).Select(id => _cards[id]).ToList();
    }

    public List<Card> PeekBottomCards(int count) {
        count = Math.Min(count, _cardOrder.Count);
        return _cardOrder.Take(count).Select(id => _cards[id]).ToList();
    }

    public int GetCardPosition(string cardInstanceId) => _cardOrder.IndexOf(cardInstanceId);

    public void MoveCard(int fromIndex, int toIndex) {
        if (fromIndex < 0 || fromIndex >= _cardOrder.Count ||
            toIndex < 0 || toIndex >= _cardOrder.Count || fromIndex == toIndex)
            return;

        var cardId = _cardOrder[fromIndex];
        _cardOrder.RemoveAt(fromIndex);
        _cardOrder.Insert(toIndex, cardId);

        OnChanged?.Invoke(this);
    }

    public void MoveCardToTop(string cardInstanceId) => MoveCardToPosition(cardInstanceId, _cardOrder.Count - 1);

    public void MoveCardToBottom(string cardInstanceId) => MoveCardToPosition(cardInstanceId, 0);

    private void MoveCardToPosition(string cardInstanceId, int targetPosition) {
        var currentIndex = _cardOrder.IndexOf(cardInstanceId);
        if (currentIndex >= 0 && currentIndex != targetPosition) {
            _cardOrder.RemoveAt(currentIndex);

            // Коригуємо цільову позицію якщо видалили елемент перед нею
            if (currentIndex < targetPosition)
                targetPosition--;

            _cardOrder.Insert(targetPosition, cardInstanceId);
            OnChanged?.Invoke(this);
        }
    }

    public void ReverseOrder() {
        _cardOrder.Reverse();
        OnChanged?.Invoke(this);
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
