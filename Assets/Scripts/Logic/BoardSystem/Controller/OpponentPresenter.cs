using Cysharp.Threading.Tasks;
using ModestTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


// Please dont watch this class under the influence of alcohol
public class OpponentPresenter : UnitPresenter, IDisposable {
    
    [Inject] public IOperationManager operationManager;
    [Inject] public IEventBus<IEvent> eventBus;
    [Inject] private CardProvider _cardProvider;
    [Inject] private ICardFactory _cardFactory;

    [Inject] public readonly IPresenterFactory presenterFactory;
    [Inject] public IUnitRegistry unitRegistry;

    public Opponent Opponent { get; private set; }
    public OpponentView OpponentView;

    public HandPresenter HandPresenter;
    public DeckPresenter DeckPresenter;

    public OpponentPresenter(Opponent opponent, OpponentView opponentView) : base(opponent, opponentView) {
        Opponent = opponent;
        OpponentView = opponentView;
    }

    #region Initialization

    public virtual void Initialize() {
        HandPresenter = presenterFactory.CreateUnitPresenter<HandPresenter>(Opponent.Hand, OpponentView.HandDisplay);
        unitRegistry.Register(HandPresenter);
        FillDeckWithRandomCards(20);
        SubscribeToEvents();
    }

    private void SubscribeToEvents() {
        eventBus.SubscribeTo<BattleStartedEvent>(StartBattleActions);
    }

    private void UnSubscribeEvents() {
        if (eventBus != null) {
            eventBus.UnsubscribeFrom<BattleStartedEvent>(StartBattleActions);
        }
    }
    #endregion

    public void StartBattleActions(ref BattleStartedEvent eventData) {
        FillDeckWithRandomCards(40);
    }

    #region Card Management
    public void FillDeckWithRandomCards(int amount) {
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
            Card card = DeckPresenter.Deck.Draw();
            if (card == null) {
                eventBus.Raise(new OnDeckEmptyDrawn(Opponent));
                return drawnCards;
            }

            if (!Opponent.Hand.Add(card)) {
                eventBus.Raise(new DiscardCardEvent(card, Opponent));
            }

            drawnCards.Add(card);
            eventBus.Raise(new OnCardDrawn(card, Opponent));
            drawAmount--;
        }
        return drawnCards;
    }
    #endregion

    #region API
    public void DrawTestCards() {
        DrawCards(5);
    }

    public override string ToString() {
        if (Opponent != null) {
            return Opponent.Data.Name;
        }
        return "Opponent";
    }


    #endregion

    public virtual void Dispose() {
        UnSubscribeEvents();
    }
}

public class PlayerPresenter : OpponentPresenter {
    [Inject] public ICardPlayService cardPlayService;
    [Inject] ITargetFiller targetFiller;

    private PlayerState currentState;

    public PlayerView PlayerView;
    public PlayerSelectorService SelectorPresenter;

    public PlayerPresenter(Opponent opponent, PlayerView playerView) : base(opponent, playerView) {
        PlayerView = playerView;
    }

    public override void Initialize() {
        base.Initialize();
        SelectorPresenter = presenterFactory.CreatePresenter<PlayerSelectorService>(PlayerView.SelectionDisplay);
        SelectorPresenter.OnSelectionStarted += OnSelectionStarted;
        SelectorPresenter.OnSelectionCompleted += OnSelectionCompleted;
        SelectorPresenter.OnSelectionCancelled += OnSelectionCancelled;

        ITargetSelectionService selectionService = SelectorPresenter;
        targetFiller.RegisterSelector(Opponent.Id, selectionService);
        
        SwitchState(new IdleState());
    }

    private void OnSelectionStarted(TargetSelectionRequest request) {
        SwitchState(new BusyState(request));
    }

    private void OnSelectionCompleted(TargetSelectionRequest request, UnitModel target) {
        SwitchState(new IdleState());
    }

    private void OnSelectionCancelled(TargetSelectionRequest request) {
        SwitchState(new IdleState());
    }

    public void SwitchState(PlayerState newState) {
        if (newState == null) {
            Debug.LogError("Attempting to switch to null state");
            return;
        }

        if (currentState?.GetType() == newState.GetType()) {
            return;
        }

        currentState?.Exit();
        currentState = newState;
        currentState.Presenter = this;
        currentState.Enter();

        Debug.Log($"State changed to: {currentState.GetType().Name}");
    }

    public override void Dispose() {
        base.Dispose();
        targetFiller.UnRegisterSelector(Opponent.Id);
    }
}


public class PlayerState : State {
    public PlayerPresenter Presenter;
}

public class PassiveState : PlayerState {
    // Implementation here
}

public class IdleState : PlayerState {
    HandPresenter handPresenter;

    public override void Enter() {
        base.Enter();
        handPresenter = Presenter.HandPresenter;
        handPresenter.SetInteractable(true);
        handPresenter.OnCardClicked += OnCardClicked;
    }

    private void OnCardClicked(CardPresenter card) {
        if (card == null) return;
        Presenter.cardPlayService.PlayCardAsync(card.Card).Forget();
    }


    public override void Exit() {
        base.Exit();

        if (handPresenter != null) {
            handPresenter.OnCardClicked -= OnCardClicked;
        }
    }
}

public class BusyState : PlayerState {
    private readonly TargetSelectionRequest _request;
    
    private HandPresenter handPresenter;
    private PlayerSelectorService SelectorPresenter;
    private ICardPlayService cardPlayService;

   
    public BusyState(TargetSelectionRequest request) {
        _request = request;
    }

    public override void Enter() {
        base.Enter();

        handPresenter = Presenter.HandPresenter;
        cardPlayService = Presenter.cardPlayService;
        SelectorPresenter = Presenter.SelectorPresenter;


        handPresenter.SetInteractable(false);
        if (_request.Source is Card card && cardPlayService.IsPlayingCard(card)) {
            CardPresenter cardPresenter = Presenter.unitRegistry.GetPresenter<CardPresenter>(card);
        }

        SelectorPresenter.OnSelectionCompleted += HandleFinishedSelection;
        SelectorPresenter.OnSelectionCancelled += HandleCanceledSelection;
    }

    private void HandleCanceledSelection(TargetSelectionRequest request) {
        Presenter.SwitchState(new IdleState());
    }

    private void HandleFinishedSelection(TargetSelectionRequest request, UnitModel model) {
        Presenter.SwitchState(new IdleState());
    }

    public override void Exit() {
        base.Exit();
        handPresenter.UpdateCardsOrder();
        handPresenter.SetInteractable(true);
    }
}