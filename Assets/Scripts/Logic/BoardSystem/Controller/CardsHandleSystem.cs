using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CardsHandleSystem : MonoBehaviour {
    [SerializeField] private CardHandView handView;
    [SerializeField] private DeckView deckView;
    [SerializeField] private DeckView discardDeckView;

    public HandPresenter HandPresenter { get; private set; }
    private DeckPresenter _deckPresenter;
    private DeckPresenter _discardeckPresenter; // not used now
    public Opponent Player { get; private set; }
    public CardSpendable CardSpendable { get; private set; }
    public Opponent CurrentPlayer { get; internal set; }

    [Inject] GameEventBus _eventBus;
    [Inject] CommandManager _commandManager;
    [Inject] CardProvider _cardProvider;

    public void Initialize(BaseOpponentPresenter presenter) {
        if (handView == null || deckView == null || discardDeckView == null) {
            Debug.LogWarning($"{gameObject} is not set");
            return;
        } 
        Player = presenter.OpponentModel;
        CardSpendable = new CardSpendable(Player, Player.Mana, Player.Health, _eventBus);

        OpponentData data = Player.Data; // soon opponent data will define deck and cards
        Deck deckModel = new Deck();
        CardHand handModel = new CardHand();

        _deckPresenter = new DeckPresenter(deckModel, deckView);
        HandPresenter = new HandPresenter(handModel, handView);
        HandPresenter.OnCardSelected += PlayCard;

        _eventBus.SubscribeTo<BattleStartedEvent>(StartBattleActions);
        _eventBus.SubscribeTo<BattleEndEventData>(EndBattleActions);
    }

    private void StartBattleActions(ref BattleStartedEvent eventData) {
        _eventBus.UnsubscribeFrom<BattleStartedEvent>(StartBattleActions);
        _eventBus.UnsubscribeFrom<BattleEndEventData>(EndBattleActions);

        _eventBus.SubscribeTo<OnTurnStart>(TurnStartActions);

        _commandManager.EnqueueCommands(new List<Command> {
            new InitDeckCommand(_deckPresenter, 40, _cardProvider).SetPriority(11),
            new DrawCardCommand(this, 3),
        });
    }

    protected virtual void TurnStartActions(ref OnTurnStart eventData) {
        if (eventData.StartingOpponent == Player)
            _commandManager.EnqueueCommand(new DrawCardCommand(this));
    }

    private void PlayCard(Card card) {
        PlayCardCommand playCardCommand = new PlayCardCommand(this, card);
        _commandManager.EnqueueCommand(playCardCommand);
    }

    private void EndBattleActions(ref BattleEndEventData eventData) {
        _eventBus.UnsubscribeFrom<OnTurnStart>(TurnStartActions);
        _deckPresenter.Deck.ClearDeck();
        HandPresenter.ClearHand();
    }

    public async UniTask<List<Card>> DrawCards(int drawAmount) {
        List<Card> drawnCards = new();

        while (drawAmount > 0) {
            Card card = _deckPresenter.Deck.DrawCard();
            if (card == null) {
                _eventBus.Raise(new OnDeckEmptyDrawn(Player));
                
                await UniTask.CompletedTask;
                return drawnCards;
            }

            if (!HandPresenter.CardHand.AddCard(card)) {
                
                _discardeckPresenter.Deck.AddCard(card);
                _eventBus.Raise(new DiscardCardEvent(card, Player));
            }
            drawnCards.Add(card);
            _eventBus.Raise(new OnCardDrawn(card, Player));
            drawAmount--;
        }
        return drawnCards;
    }

    private void OnDestroy() {
        if (_eventBus != null) {
            _eventBus.UnsubscribeFrom<BattleStartedEvent>(StartBattleActions);
            _eventBus.UnsubscribeFrom<BattleEndEventData>(EndBattleActions);

            _eventBus.UnsubscribeFrom<OnTurnStart>(TurnStartActions);
        }
    }
}

public class PlayCardCommand : Command {
    private Card _card;
    private bool _isPlayed = false;
    private CardsHandleSystem _cardsPlaySystem;
    private Opponent _cardPlayer;
    private CardHand _cardHand;
    private CardSpendable _cardSpendable;
    private ResourceData _resourceData;

    public PlayCardCommand(CardsHandleSystem cardsPlaySystem, Card card) {
        _cardsPlaySystem = cardsPlaySystem;
        _card = card;
        _cardPlayer = cardsPlaySystem.Player;
        _cardHand = cardsPlaySystem.HandPresenter.CardHand;
        _cardSpendable = cardsPlaySystem.CardSpendable;
    }

    public async override UniTask Execute() {
        _cardHand.RemoveCard(_card);
        List<GameOperation> cardPlayOperations = _card.GetCardPlayOperations();

        // We need to check if any operation is possible first
        if (!IsAnyOperationPossible(cardPlayOperations)) {
            _cardHand.AddCard(_card);
            return;
        }

        try {
            
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
            if (operation == null || !operation.IsPossible()) {
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
            if (operation != null && operation.IsPossible()) {
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

public class InitDeckCommand : Command {
    private DeckPresenter _deckPresenter;
    private CardCollection _cardCollection;
    private CardProvider _cardProvider;
    private int _cardAmount;

    public InitDeckCommand(DeckPresenter deckPresenter, int amount, CardProvider cardProvider) {
        _deckPresenter = deckPresenter;
        _cardAmount = amount;
        _cardProvider = cardProvider;
    }

    public async override UniTask Execute() {
        Deck deck = _deckPresenter.Deck;
        _cardCollection = await GenerateRandomCollection();
        deck.Initialize(_cardCollection);
        await UniTask.CompletedTask;
    }

    public async UniTask<CardCollection> GenerateRandomCollection() {
        CardCollection collection = new();
        List<CardData> _unclokedCards = await _cardProvider.GetRandomUnlockedCards(_cardAmount);
        foreach (var card in _unclokedCards) {
            collection.AddCardToCollection(card);
        }
        return collection;
    }

    public async override UniTask Undo() {
        _deckPresenter.Deck.ClearDeck();
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
        List<Card> drawnCards = await _cardsPlaySystem.DrawCards(_drawAmount);
        await UniTask.CompletedTask;
    }

    public override UniTask Undo() {
        throw new NotImplementedException();
    }
}
