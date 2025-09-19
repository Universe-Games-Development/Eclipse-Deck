using Cysharp.Threading.Tasks;
using ModestTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BoardPlayerPresenter : UnitPresenter {
    #region Injected Dependencies
    [Inject] protected IEventBus<IEvent> _eventBus;
    [Inject] private CommandManager _commandManager;
    [Inject] private CardProvider _cardProvider;
    [Inject] private ICardFactory<Card3DView> _cardFactory;
    #endregion

    #region Serialized Fields
    [Header("Core Data")]
    [SerializeField] private CharacterData Data;
    [SerializeField] private HealthCellView _healthDisplay;

    [Header("Card Views")]
    [SerializeField] private CardHandView handView;
    [SerializeField] private DeckView deckView;

    [Header("Debug")]
    public HumanTargetSelector Selector;
    #endregion

    #region Runtime State
    public Direction FacingDirection;
    public Opponent Opponent { get; private set; }

    public HandPresenter handPresenter;
    private DeckPresenter _deckPresenter;
    #endregion

    #region Unity Lifecycle
    private void Awake() {
        InitializeOpponent();
        InitializePresenters();
        SubscribeToEvents();
        FillDeckWithRandomCards(40);
    }

    protected override void OnDestroy() {
        if (_eventBus != null) {
            _eventBus.UnsubscribeFrom<BattleStartedEvent>(StartBattleActions);
            _eventBus.UnsubscribeFrom<BattleEndEventData>(EndBattleActions);
            _eventBus.UnsubscribeFrom<TurnStartEvent>(TurnStartActions);
        }
        base.OnDestroy();
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawSphere(transform.position, 1f);
    }
    #endregion

    #region Initialization
    private void InitializeOpponent() {
        if (Data != null) {
            Opponent = new Opponent(Data);
            Opponent.ChangeOwner(Opponent);
        }
    }

    private void InitializePresenters() {
        _deckPresenter = new DeckPresenter(Opponent.Deck, deckView);
        handPresenter.Initialize(Opponent.Hand, handView);
    }

    private void SubscribeToEvents() {
        _eventBus.SubscribeTo<BattleStartedEvent>(StartBattleActions);
        _eventBus.SubscribeTo<BattleEndEventData>(EndBattleActions);
        _eventBus.SubscribeTo<TurnStartEvent>(TurnStartActions);
    }
    #endregion

    #region Event Handlers
    private void TurnStartActions(ref TurnStartEvent eventData) {
        if (eventData.StartingOpponent == this) {
            _commandManager.EnqueueCommand(new DrawCardCommand(this));
        }
    }

    private void EndBattleActions(ref BattleEndEventData eventData) {
        _deckPresenter.Deck.Clear();
        Opponent.Hand.Clear();
    }

    public void StartBattleActions(ref BattleStartedEvent eventData) {
        FillDeckWithRandomCards(40);
    }
    #endregion

    #region Card Management
    private void FillDeckWithRandomCards(int amount) {
        Deck deck = Opponent.Deck;
        var cards = GenerateRandomCards(amount);
        deck.AddRange(cards);
    }

    public List<Card> GenerateRandomCards(int amount) {
        CardCollection collection = new();
        List<CardData> unlockedCards = _cardProvider.GetRandomUnlockedCards(amount);

        if (unlockedCards.IsEmpty())
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

    public List<Card> DrawCards(int drawAmount) {
        List<Card> drawnCards = new();

        while (drawAmount > 0) {
            Card card = _deckPresenter.Deck.Draw();
            if (card == null) {
                _eventBus.Raise(new OnDeckEmptyDrawn(Opponent));
                return drawnCards;
            }

            if (!Opponent.Hand.Add(card)) {
                _eventBus.Raise(new DiscardCardEvent(card, Opponent));
            }

            drawnCards.Add(card);
            _eventBus.Raise(new OnCardDrawn(card, Opponent));
            drawAmount--;
        }
        return drawnCards;
    }
    #endregion

    #region API
    public void BindPlayer(Opponent character) {
        Opponent = character;

        if (_healthDisplay != null) {
            _healthDisplay.Initialize();
            _healthDisplay.AssignOwner(Opponent);
        }

        InitializeCards();
    }

    public void InitializeCards() {
        var battleStartedEvent = new BattleStartedEvent();
        StartBattleActions(ref battleStartedEvent);
    }

    public void DrawTestCards() {
        DrawCards(5);
    }

    public void SelfClear() {
        _healthDisplay?.ClearOwner();
    }

    public override UnitModel GetModel() => Opponent;

    public override string ToString() => gameObject.name;
    #endregion
}


public class DrawCardCommand : Command {
    private BoardPlayerPresenter opponentPresetner;
    private int _drawAmount;
    private List<Card> drawnCards;
    public DrawCardCommand(BoardPlayerPresenter boardPlayer, int drawAmount = 1) {
        opponentPresetner = boardPlayer;
        _drawAmount = drawAmount;
    }

    public async override UniTask Execute() {
        opponentPresetner.DrawCards(_drawAmount);
        await UniTask.CompletedTask;
    }

    public override UniTask Undo() {
        throw new NotImplementedException();
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