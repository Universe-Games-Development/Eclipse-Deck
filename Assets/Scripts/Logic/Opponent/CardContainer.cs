using System;
using System.Collections.Generic;
using System.Linq;

public class CardContainer : UnitModel {
    private readonly Dictionary<string, Card> cards = new Dictionary<string, Card>();
    private readonly int maxSize;
    protected const int DefaultSize = 127;

    public CardContainer(int maxSize = DefaultSize) {
        this.maxSize = maxSize;
    }

    public int Count => cards.Count;
    public bool IsEmpty => cards.Count == 0;

    public IReadOnlyCollection<Card> Cards => cards.Values;

    public IReadOnlyList<Card> CardsList => cards.Values.ToList().AsReadOnly();

    public event Action<Card> OnCardAdded;
    public event Action<Card> OnCardRemoved;
    public event Action<CardContainer> OnChanged;

    public virtual bool Add(Card card) {
        if (card == null || Count >= maxSize || string.IsNullOrEmpty(card.InstanceId))
            return false;

        // Видаляємо з попереднього контейнера
        card.CurrentContainer?.Remove(card);

        // Додаємо в словник
        cards[card.InstanceId] = card;
        card.SetContainer(this);

        OnCardAdded?.Invoke(card);
        OnChanged?.Invoke(this);
        return true;
    }

    public virtual bool AddRange(IEnumerable<Card> newCards) {
        if (newCards == null)
            return false;

        bool anyAdded = false;
        foreach (var card in newCards) {
            if (Add(card)) {
                anyAdded = true;
            }
            if (Count >= maxSize) break;
        }

        if (anyAdded) {
            OnChanged?.Invoke(this);
        }
        return anyAdded;
    }

    public virtual bool Remove(Card card) {
        if (card == null || !Contains(card))
            return false;

        return Remove(card.InstanceId);
    }

    public virtual bool Remove(string cardInstanceId) {
        if (string.IsNullOrEmpty(cardInstanceId) || !cards.TryGetValue(cardInstanceId, out var card))
            return false;

        cards.Remove(cardInstanceId);

        if (card.CurrentContainer == this)
            card.SetContainer(null);

        OnCardRemoved?.Invoke(card);
        OnChanged?.Invoke(this);
        return true;
    }

    public virtual void Clear() {
        var cardsCopy = new List<Card>(cards.Values);
        cards.Clear();

        foreach (var card in cardsCopy) {
            if (card != null && card.CurrentContainer == this)
                card.SetContainer(null);
        }

        OnChanged?.Invoke(this);
    }

    public virtual bool Contains(Card card) {
        return card != null && Contains(card.InstanceId);
    }

    public virtual bool Contains(string cardInstanceId) {
        return !string.IsNullOrEmpty(cardInstanceId) && cards.ContainsKey(cardInstanceId);
    }

    // ✅ ШВИДКИЙ пошук по ID
    public bool TryGetCardById(string cardInstanceId, out Card card) {
        return cards.TryGetValue(cardInstanceId, out card);
    }

    public Card GetCardById(string cardInstanceId) {
        return cards.TryGetValue(cardInstanceId, out var card) ? card : null;
    }

    public bool IsFull() => Count >= maxSize;

    public virtual void Shuffle() {
        // Для словника треба перетворити на список, перемішати, і зберегти порядок?
        // Але якщо порядок не важливий, то можна просто викликати подію
        OnChanged?.Invoke(this);
    }

    public virtual Card GetRandom() {
        if (IsEmpty) return null;

        var random = new Random();
        var values = cards.Values.ToList();
        return values[random.Next(values.Count)];
    }

    //public IEnumerable<Card> GetCardsByTemplateId(string templateId) {
    //    return cards.Values.Where(card => card.TemplateId == templateId);
    //}

    //public int CountByTemplateId(string templateId) {
    //    return cards.Values.Count(card => card.TemplateId == templateId);
    //}

    //public bool HasCardWithTemplateId(string templateId) {
    //    return cards.Values.Any(card => card.TemplateId == templateId);
    //}
}