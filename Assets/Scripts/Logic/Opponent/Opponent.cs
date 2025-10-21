using System;
using System.Collections.Generic;
using UnityEngine;

public class Opponent : UnitModel, IHealthable, IManaSystem, IDisposable {
    public override string OwnerId {
        get { return InstanceId; }
    }

    public Action<Card, CardPlayResult> OnCardPlayFinished;
    public Action<Card> OnCardPlayStarted;

    public Health Health { get; private set; }
    public Mana Mana { get; private set; }
    public OpponentData Data { get; private set; }

    public CardSpendable CardSpendable { get; private set; }
    public Deck Deck { get; private set; }
    public CardHand Hand { get; private set; }

    private ICardPlayService cardPlayService;
    private IEventBus<IEvent> eventBus;

    private IDeckBuilder _deckBuilder;
    public Opponent(OpponentData data, Deck deck, CardHand hand, ICardPlayService cardPlayService, IEventBus<IEvent> eventBus, IDeckBuilder deckBuilder) {
        Data = data;
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        Hand = hand ?? throw new ArgumentNullException(nameof(hand));
        _deckBuilder = deckBuilder;

        UnitName = Data.Name;
        InstanceId = $"{Data.Name}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        
        Health = new Health(data.Health);
        Mana = new Mana(data.Mana);
        CardSpendable = new CardSpendable(Mana, Health);

        Deck.ChangeOwner(InstanceId);
        Hand.ChangeOwner(InstanceId);

        this.cardPlayService = cardPlayService;
        this.eventBus = eventBus;
        cardPlayService.OnCardPlayFinished += HandleCardPlayFinished;
        cardPlayService.OnCardActivated += HandleActivatedCard;
    }

    private void HandleActivatedCard(Card card) {
        if (!Hand.Contains(card)) return;

        Hand.Remove(card);
    }

    public void FillDeck() {
        List<Card> cards = _deckBuilder.BuildDeck(Data.DeckConfig);
        Deck.AddRange(cards);
    }

    private void HandleCardPlayFinished(Card card, CardPlayResult result) {
        OnCardPlayFinished?.Invoke(card, result);
    }

    public void SpendMana(int currentValue) {
        int was = Mana.Current;
        Mana.Subtract(currentValue);
        //Debug.Log($"Mana: {Mana.Current} / {Mana.Max}");
    }

    public void PlayCard(Card card) {
        if (card == null) return;
        OnCardPlayStarted?.Invoke(card);

        cardPlayService.PlayCardAsync(card);
    }

    #region IHealthable
    public bool IsDead => Health.IsDead;

    public int CurrentHealth => Health.Current;

    public float BaseValue => Health.BaseMaximum;

    public void TakeDamage(int damage) {
        Health.TakeDamage(damage);
    }
    #endregion

    public void DrawCard() {
        Card card = Deck.Draw();
        if (card == null) {
            Debug.Log($"{UnitName} Draw from Empty deck");
            TakeDamage(1);
            return;
        }
        Hand.Add(card);
    }

    public void DrawCards(int amount) {
        for (int i = amount - 1; i >= 0; i--) {
            DrawCard();
        }
    }

    public void DiscardCard() {
        Card card = Hand.GetRandom();
        Hand.Remove(card);
    }

    public void RestoreMana() {
        Mana.RestoreMana();
    }

    public void EndTurn() {
        eventBus.Raise(new TurnEndEvent(this));
    }

    public void Dispose() {
        cardPlayService.OnCardPlayFinished -= HandleCardPlayFinished;
    }
}

public interface IDeckBuilder {
    List<Card> BuildDeck(DeckConfiguration config);
    List<Card> BuildDeckFromEntries(List<CardEntry> entries);
    List<Card> GenerateRandomDeck(int count);
}

public class DeckBuilder : IDeckBuilder {
    private readonly ICardFactory _cardFactory;
    private readonly ILogger _logger;
    private readonly CardProvider _cardProvider;

    public DeckBuilder(ICardFactory cardFactory, CardProvider cardProvider, ILogger logger = null) {
        _cardFactory = cardFactory ?? throw new ArgumentNullException(nameof(cardFactory));
        _logger = logger;
        _cardProvider = cardProvider;
    }

    public List<Card> BuildDeck(DeckConfiguration config) {
        if (config == null) {
            _logger?.LogWarning("DeckConfiguration is null");
            return new List<Card>();
        }

        if (config.UseRandomGeneration) {
            return GenerateRandomDeck(config.RandomCardCount);
        }

        return BuildDeckFromEntries(config.Cards);
    }

    public List<Card> BuildDeckFromEntries(List<CardEntry> entries) {
        if (entries == null || entries.Count == 0) {
            _logger?.LogWarning("Card entries list is empty");
            return new List<Card>();
        }

        var cards = new List<Card>();

        foreach (var entry in entries) {
            if (entry.CardData == null) {
                _logger?.LogWarning("CardData is null in entry");
                continue;
            }

            for (int i = 0; i < entry.Quantity; i++) {
                var card = _cardFactory.CreateCard(entry.CardData);
                if (card != null) {
                    cards.Add(card);
                } else {
                    _logger?.LogError($"Failed to create card: {entry.CardData.name}");
                }
            }
        }

        _logger?.LogInfo($"Built deck with {cards.Count} cards");
        return cards;
    }

    public List<Card> GenerateRandomDeck(int count) {
        List<CardData> cardPool = _cardProvider.GetUnlockedCards();
        if (cardPool == null || cardPool.Count == 0) {
            _logger?.LogWarning("Card pool is empty for random generation");
            return new List<Card>();
        }

        var cards = new List<Card>();
        var random = new System.Random();

        for (int i = 0; i < count; i++) {
            var randomData = cardPool[random.Next(cardPool.Count)];
            var card = _cardFactory.CreateCard(randomData);

            if (card != null) {
                cards.Add(card);
            }
        }

        _logger?.LogInfo($"Generated random deck with {cards.Count} cards");
        return cards;
    }
}
