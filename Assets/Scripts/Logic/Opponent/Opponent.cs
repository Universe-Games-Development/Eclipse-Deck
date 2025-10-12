using System;
using System.Collections.Generic;
using Zenject;

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

    //Deck Generation
    [Inject] private CardProvider _cardProvider;
    [Inject] private ICardFactory _cardFactory;

    public Opponent(OpponentData data, Deck deck, CardHand hand, ICardPlayService cardPlayService, IEventBus<IEvent> eventBus) {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        Hand = hand ?? throw new ArgumentNullException(nameof(hand));

        InstanceId = $"{Data.Name}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

        Health = new Health(data.Health);
        Mana = new Mana(data.Mana);
        CardSpendable = new CardSpendable(Mana, Health);

        
        Deck.ChangeOwner(InstanceId);
        Hand.ChangeOwner(InstanceId);

        this.cardPlayService = cardPlayService;
        this.eventBus = eventBus;
        cardPlayService.OnCardPlayFinished += HandleCardPlayFinished;
    }

    private void HandleCardPlayFinished(Card card, CardPlayResult result) {
        if (result.IsSuccess && Hand.Contains(card)) {
            Hand.Remove(card);
        }

        OnCardPlayFinished?.Invoke(card, result);
    }

    public void SpendMana(int currentValue) {
        int was = Mana.Current;
        Mana.Subtract(currentValue);
        //Debug.Log($"Mana: {Mana.Current} / {Mana.Max}");
    }

    public void PlayCard(Card card) {
        if (card == null) return;
        cardPlayService.PlayCardAsync(card);
        OnCardPlayStarted?.Invoke(card);
    }

    #region IHealthable
    public bool IsDead => Health.IsDead;

    public int CurrentHealth => Health.Current;

    public void TakeDamage(int damage) {
        Health.TakeDamage(damage);
    }
    #endregion

    public void DrawCard() {
        Card card = Deck.Draw();
        if (card == null) {
            // Handle empty deck logic
            TakeDamage(1);
            return;
        }
        Hand.Add(card);
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

    public void FillDeckWithRandomCards(int amount) {
        var cards = GenerateRandomCards(amount);
        Deck.AddRange(cards);
    }

    public List<Card> GenerateRandomCards(int amount) {
        CardCollection collection = new();
        List<CardData> unlockedCards = _cardProvider.GetRandomUnlockedCards(amount);

        if (unlockedCards.Count == 0)
            return new List<Card>();

        // випадковий набір карт
        for (int i = 0; i < amount; i++) {
            var randomIndex = UnityEngine.Random.Range(0, unlockedCards.Count);
            var randomCard = unlockedCards[randomIndex];
            collection.AddCardToCollection(randomCard);
        }

        // створення інстансів
        List<Card> cards = new();
        foreach (var entry in collection.cardEntries) {
            for (int i = 0; i < entry.Value; i++) {
                Card newCard = _cardFactory.CreateCard(entry.Key);
                if (newCard != null)
                    cards.Add(newCard);
            }
        }
        return cards;
    }
}
