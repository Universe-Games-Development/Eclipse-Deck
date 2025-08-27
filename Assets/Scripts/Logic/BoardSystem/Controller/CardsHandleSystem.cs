using Cysharp.Threading.Tasks;
using ModestTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CardsHandleSystem : MonoBehaviour {
    [SerializeField] private CardHandView handView;
    [SerializeField] private DeckView deckView;
    [SerializeField] private DeckView discardDeckView;
    public HandPresenter HandPresenter;

    private DeckPresenter _deckPresenter;
    private DeckPresenter _discardeckPresenter; // not used now
    [SerializeField] public BoardPlayer BoardPlayer;
    public CardSpendable CardSpendable { get; private set; }

    [Inject] GameEventBus _eventBus;
    [Inject] CommandManager _commandManager;
    [Inject] CardProvider _cardProvider;
    [Inject] DiContainer diContainer;
    CardFactory _cardFactory;

    private void Awake() {
        CardFactory cardFactory = new(diContainer);
        Deck deckModel = new();
        CardHand handModel = new();
        _cardFactory = new(diContainer);

        _deckPresenter = new(deckModel, deckView);

        HandPresenter.Initialize(handModel, handView);
    }

    public void Initialize(BoardPlayer boardPlayer) { 
        BoardPlayer = boardPlayer;
        CardSpendable = new CardSpendable(BoardPlayer.Mana, BoardPlayer.Health, _eventBus);

        CharacterData data = BoardPlayer.Character.Data; // soon opponent data will define deck and cards

        // Move to the global game manager
        _eventBus.SubscribeTo<BattleStartedEvent>(StartBattleActions);
        _eventBus.SubscribeTo<BattleEndEventData>(EndBattleActions);
    }

    public void StartBattleActions(ref BattleStartedEvent eventData) {
        _eventBus.UnsubscribeFrom<BattleStartedEvent>(StartBattleActions);
        _eventBus.UnsubscribeFrom<BattleEndEventData>(EndBattleActions);

        _eventBus.SubscribeTo<TurnStartEvent>(TurnStartActions);

        Deck deck = _deckPresenter.Deck;
        List<Card> _cards = GenerateRandomCards(40);
        deck.AddRange(_cards);
    }

    protected virtual void TurnStartActions(ref TurnStartEvent eventData) {
        if (eventData.StartingOpponent == BoardPlayer)
            _commandManager.EnqueueCommand(new DrawCardCommand(this));
    }

    private void EndBattleActions(ref BattleEndEventData eventData) {
        _eventBus.UnsubscribeFrom<TurnStartEvent>(TurnStartActions);
        _deckPresenter.Deck.Clear();
        HandPresenter.ClearHand();
    }
    

    public List<Card> GenerateRandomCards(int cardAmount) {
        CardCollection collection = new();
        List<CardData> _unclokedCards = _cardProvider.GetRandomUnlockedCards(cardAmount);
        if (_unclokedCards.IsEmpty()) return new List<Card>();
        
        for (int i = 0; i < cardAmount; i++) {
            var randomIndex = UnityEngine.Random.Range(0, _unclokedCards.Count);
            var randomCard = _unclokedCards[randomIndex];
            collection.AddCardToCollection(randomCard);
        }

        return _cardFactory.CreateCardsFromCollection(collection); ;
    }

    public List<Card> DrawCards(int drawAmount) {
        List<Card> drawnCards = new();

        while (drawAmount > 0) {
            Card card = _deckPresenter.Deck.Draw();
            if (card == null) {
                _eventBus.Raise(new OnDeckEmptyDrawn(BoardPlayer));
                
                return drawnCards;
            }

            if (!HandPresenter.Hand.Add(card)) {
                
                _discardeckPresenter.Deck.Add(card);
                _eventBus.Raise(new DiscardCardEvent(card, BoardPlayer));
            }
            drawnCards.Add(card);
            _eventBus.Raise(new OnCardDrawn(card, BoardPlayer));
            drawAmount--;
        }
        return drawnCards;
    }

    private void OnDestroy() {
        if (_eventBus != null) {
            _eventBus.UnsubscribeFrom<BattleStartedEvent>(StartBattleActions);
            _eventBus.UnsubscribeFrom<BattleEndEventData>(EndBattleActions);

            _eventBus.UnsubscribeFrom<TurnStartEvent>(TurnStartActions);
        }
    }
}

public class DrawCardCommand : Command {
    private CardsHandleSystem _cardsPlaySystem;
    private int _drawAmount;
    private List<Card> drawnCards;
    public DrawCardCommand(CardsHandleSystem cardsPlaySystem, int drawAmount = 1) {
        _cardsPlaySystem = cardsPlaySystem;
        _drawAmount = drawAmount;
    }

    public async override UniTask Execute() {
        List<Card> drawnCards = _cardsPlaySystem.DrawCards(_drawAmount);
        await UniTask.CompletedTask;
    }

    public override UniTask Undo() {
        throw new NotImplementedException();
    }
}

public struct DiscardCardEvent : IEvent {
    public Card card;
    public DiscardCardEvent(Card card, BoardPlayer owner) {
        this.card = card;
    }
}

public struct ExileCardEvent : IEvent {
    public Card card;
    public ExileCardEvent(Card card) {
        this.card = card;
    }
}