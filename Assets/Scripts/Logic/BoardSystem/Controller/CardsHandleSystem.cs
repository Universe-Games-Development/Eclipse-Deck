using Cysharp.Threading.Tasks;
using ModestTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CardsHandleSystem : MonoBehaviour {
    [SerializeField] private CardHandUIView handUIView;
    [SerializeField] private DeckView deckView;
    [SerializeField] private DeckView discardDeckView;

    public HandPresenter HandPresenter { get; private set; }
    private DeckPresenter _deckPresenter;
    private DeckPresenter _discardeckPresenter; // not used now
    public BoardPlayer BoardPlayer { get; private set; }
    public CardSpendable CardSpendable { get; private set; }

    [Inject] GameEventBus _eventBus;
    [Inject] CommandManager _commandManager;
    [Inject] CardProvider _cardProvider;
    [Inject] DiContainer diContainer;

    public void Initialize(BoardPlayer boardPlayer) { 
        BoardPlayer = boardPlayer;
        CardSpendable = new CardSpendable(BoardPlayer.Mana, BoardPlayer.Health, _eventBus);

        OpponentData data = BoardPlayer.Info.Data; // soon opponent data will define deck and cards
        CardFactory cardFactory = new(diContainer);
        Deck deckModel = new(cardFactory);
        CardHand handModel = new();

        _deckPresenter = new(deckModel, deckView);
        HandPresenter = new(handModel, handUIView);
        HandPresenter.CardHand.OnCardSelected += PlayCard;

        _eventBus.SubscribeTo<BattleStartedEvent>(StartBattleActions);
        _eventBus.SubscribeTo<BattleEndEventData>(EndBattleActions);
    }

    public void StartBattleActions(ref BattleStartedEvent eventData) {
        _eventBus.UnsubscribeFrom<BattleStartedEvent>(StartBattleActions);
        _eventBus.UnsubscribeFrom<BattleEndEventData>(EndBattleActions);

        _eventBus.SubscribeTo<TurnStartEvent>(TurnStartActions);

        Deck deck = _deckPresenter.Deck;
        CardCollection _cardCollection = GenerateRandomCollection(40);
        deck.Initialize(_cardCollection);

        DrawCards(6);

        //_commandManager.EnqueueCommands(new List<Command> {
        //    new DrawCardCommand(this, 3),
        //});
    }

    protected virtual void TurnStartActions(ref TurnStartEvent eventData) {
        if (eventData.StartingOpponent == BoardPlayer)
            _commandManager.EnqueueCommand(new DrawCardCommand(this));
    }

    private void PlayCard(Card card) {
        PlayCardCommand playCardCommand = new(this, card);
        _commandManager.EnqueueCommand(playCardCommand);
    }

    private void EndBattleActions(ref BattleEndEventData eventData) {
        _eventBus.UnsubscribeFrom<TurnStartEvent>(TurnStartActions);
        _deckPresenter.Deck.ClearDeck();
        HandPresenter.ClearHand();
    }


    public CardCollection GenerateRandomCollection(int cardAmount) {
        CardCollection collection = new();
        List<CardData> _unclokedCards = _cardProvider.GetRandomUnlockedCards(cardAmount);
        if (_unclokedCards.IsEmpty()) return collection;

        for (int i = 0; i < cardAmount; i++) {
            var randomIndex = UnityEngine.Random.Range(0, _unclokedCards.Count);
            var randomCard = _unclokedCards[randomIndex];
            collection.AddCardToCollection(randomCard);
        }

        return collection;
    }

    public List<Card> DrawCards(int drawAmount) {
        List<Card> drawnCards = new();

        while (drawAmount > 0) {
            Card card = _deckPresenter.Deck.DrawCard();
            if (card == null) {
                _eventBus.Raise(new OnDeckEmptyDrawn(BoardPlayer));
                
                return drawnCards;
            }

            if (!HandPresenter.CardHand.AddCard(card)) {
                
                _discardeckPresenter.Deck.AddCard(card);
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

public class PlayCardCommand : Command {
    private Card _card;
    private bool _isPlayed = false;
    private CardsHandleSystem _cardsPlaySystem;
    private BoardPlayer _cardPlayer;
    private CardHand _cardHand;
    private CardSpendable _cardSpendable;
    private ResourceData _resourceData;

    public PlayCardCommand(CardsHandleSystem cardsPlaySystem, Card card) {
        _cardsPlaySystem = cardsPlaySystem;
        _card = card;
        _cardPlayer = cardsPlaySystem.BoardPlayer;
        _cardHand = cardsPlaySystem.HandPresenter.CardHand;
        _cardSpendable = cardsPlaySystem.CardSpendable;
    }

    public async override UniTask Execute() {
        try {
            _cardHand.RemoveCard(_card);
            List<GameOperation> cardPlayOperations = _card.GetCardPlayOperations();

            // We need to check if any operation is possible first
            if (!IsAnyOperationPossible(cardPlayOperations)) {
                _cardHand.AddCard(_card);
                return;
            }
            // If no operation was successfully executed, return card back else spend resources (players spend health so it always can be spent)
            if (!await PerformCardOperations(cardPlayOperations)) {
                _cardHand.AddCard(_card);
            } else {
                _isPlayed = true;
                _resourceData = _cardSpendable.TrySpend(_card.Cost.CurrentValue);
            }
        } catch (Exception ex) {
            // Make sure card is returned to hand if any exception occurs
            _cardHand.AddCard(_card);
            _isPlayed = false;
            Debug.LogError($"Error executing PlayCardCommand: {ex.Message}");
        }
    }

    private async UniTask<bool> PerformCardOperations(List<GameOperation> cardPlayOperations) {
        bool isAnyOperationPerformed = false;

        foreach (var operation in cardPlayOperations) {
            if (operation == null || !operation.AreTargetsFilled()) {
                continue;
            }

            try {
                bool requirementsFilled = await operation.FillRequirements(_cardPlayer);

                if (requirementsFilled) {
                    operation.PerformOperation();
                    isAnyOperationPerformed = true;
                }
            } catch (Exception ex) {
                Debug.LogWarning($"Failed to perform operation for card {_card}: {ex.Message}");
                // Continue to next operation if one fails
            }
        }

        return isAnyOperationPerformed;
    }
    private bool IsAnyOperationPossible(List<GameOperation> cardPlayOperations) {
        if (cardPlayOperations == null || cardPlayOperations.Count == 0) {
            return false;
        }

        foreach (var operation in cardPlayOperations) {
            if (operation != null && operation.AreTargetsFilled()) {
                return true;
            }
        }
        return false;
    }

    public override async UniTask Undo() {
        if (_card != null && _isPlayed) {
            try {
                _cardHand.AddCard(_card);

                if (_resourceData != null) {
                    _cardsPlaySystem.CardSpendable.TryRefund(_resourceData);
                }
            } catch (Exception ex) {
                Debug.LogError($"Error undoing PlayCardCommand: {ex.Message}");
            }
        }
        await UniTask.CompletedTask;
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