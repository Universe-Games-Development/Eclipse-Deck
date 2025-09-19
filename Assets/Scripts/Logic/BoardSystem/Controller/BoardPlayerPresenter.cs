using Cysharp.Threading.Tasks;
using ModestTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BoardPlayerPresenter : UnitPresenter {
    [Inject] protected IEventBus<IEvent> _eventBus;
    [Inject] CommandManager _commandManager;
    [Inject] CardProvider _cardProvider;
    [Inject] ICardFactory<Card3DView> _cardFactory;

    public Direction FacingDirection;
    [SerializeField] private HealthCellView _healthDisplay;
    public Opponent Opponent { get; private set; }

    [SerializeField] public CharacterData Data;

    [Header("Debug")]
    public HumanTargetSelector Selector;

    [SerializeField] private CardHandView handView;
    [SerializeField] private DeckView deckView;

    public HandPresenter handPresenter;
    private DeckPresenter _deckPresenter;

    private void Awake() {
        if (Data != null) {
            Opponent = new Opponent(Data);
            Opponent.ChangeOwner(Opponent);
        }

        _deckPresenter = new(Opponent.Deck, deckView);
        handPresenter.Initialize(Opponent.Hand, handView);

        _eventBus.SubscribeTo<BattleStartedEvent>(StartBattleActions);
        _eventBus.SubscribeTo<BattleEndEventData>(EndBattleActions);
        _eventBus.SubscribeTo<TurnStartEvent>(TurnStartActions);

        Deck deck = Opponent.Deck;
        List<Card> _cards = GenerateRandomCards(40);
        deck.AddRange(_cards);
    }

    private void TurnStartActions(ref TurnStartEvent eventData) {
        if (eventData.StartingOpponent == this)
            _commandManager.EnqueueCommand(new DrawCardCommand(this));
    }

    private void EndBattleActions(ref BattleEndEventData eventData) {
        _deckPresenter.Deck.Clear();
        Opponent.Hand.Clear();
    }

    public void StartBattleActions(ref BattleStartedEvent eventData) {
        Deck deck = Opponent.Deck;
        List<Card> _cards = GenerateRandomCards(40);
        deck.AddRange(_cards);
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

        List<Card> cards = new();
        foreach (var cardEntry in collection.cardEntries) {
            for (int i = 0; i < cardEntry.Value; i++) {
                CardData cardData = cardEntry.Key;
                Card newCard = _cardFactory.CreateCard(cardData);
                if (newCard == null) continue;
                cards.Add(newCard);
            }
        }
        return cards;
    }

    /// <summary>
    /// Прив'язує об'єкт опонента до цього представлення на дошці
    /// </summary>
    public void BindPlayer(Opponent character) {
        Opponent = character;
        
        if (_healthDisplay != null) {
            _healthDisplay.Initialize();
            _healthDisplay.AssignOwner(Opponent);
        }

        InitializeCards();
    }

    /// <summary>
    /// Ініціалізує систему карт для гравця
    /// </summary>
    public void InitializeCards() {

        BattleStartedEvent battleStartedEvent = new BattleStartedEvent();
        StartBattleActions(ref battleStartedEvent);
    }

    public void DrawTestCards() {
        DrawCards(5);
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
    /// <summary>
    /// Очищає гравця з позиції за дошкою
    /// </summary>
    public void SelfClear() {

        if (_healthDisplay != null) {
            _healthDisplay.ClearOwner();
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawSphere(transform.position, 1f);
    }

    #region Unit presenter API
    public override UnitModel GetModel() {
        return Opponent;
    }
    #endregion

    

    public override string ToString() {
        return $"{gameObject.name}";
    }

    protected override void OnDestroy() {
        if (_eventBus != null) {
            _eventBus.UnsubscribeFrom<BattleStartedEvent>(StartBattleActions);
            _eventBus.UnsubscribeFrom<BattleEndEventData>(EndBattleActions);

            _eventBus.UnsubscribeFrom<TurnStartEvent>(TurnStartActions);
        }
        base.OnDestroy();
    }
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
        List<Card> drawnCards = opponentPresetner.DrawCards(_drawAmount);
        await UniTask.CompletedTask;
    }

    public override UniTask Undo() {
        throw new NotImplementedException();
    }
}

public struct DiscardCardEvent : IEvent {
    public Card card;
    public DiscardCardEvent(Card card, Opponent owner) {
        this.card = card;
    }
}

public struct ExileCardEvent : IEvent {
    public Card card;
    public ExileCardEvent(Card card) {
        this.card = card;
    }
}