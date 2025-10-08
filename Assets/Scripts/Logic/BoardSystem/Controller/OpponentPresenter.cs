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
        DeckPresenter = presenterFactory.CreateUnitPresenter<DeckPresenter>(Opponent.Deck, OpponentView.DeckDisplay);
        DeckPresenter.FillDeckWithRandomCards(20);
    }

    #endregion


    #region Card Management
    
    public bool DrawCards(int drawAmount) {
        if (drawAmount == 0) return true;

        List<Card> drawnCards = DeckPresenter.DrawCards(drawAmount);

        foreach(var card in drawnCards) {
            HandPresenter.AddCard(card);
        }
        
        return drawnCards.Count > 0;
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
    }
}

public class PlayerPresenter : OpponentPresenter {
    [Inject] public ICardPlayService cardPlayService;
    [Inject] ITargetFiller targetFiller;

    private PlayerState currentState;

    public PlayerView PlayerView;
    public PlayerSelectorService SelectorService;

    public PlayerPresenter(Opponent opponent, PlayerView playerView) : base(opponent, playerView) {
        PlayerView = playerView;
    }

    public override void Initialize() {
        base.Initialize();
        SelectorService = presenterFactory.CreatePresenter<PlayerSelectorService>(PlayerView.SelectionDisplay);
        SelectorService.OnSelectionStarted += OnSelectionStarted;
        SelectorService.OnSelectionCompleted += OnSelectionCompleted;
        SelectorService.OnSelectionCancelled += OnSelectionCancelled;
        cardPlayService.OnCardPlayFinished += OnCardPlayFinished;

        targetFiller.RegisterSelector(Opponent.Id, SelectorService);
        
        SwitchState(new IdleState());
    }

    private void OnCardPlayFinished(Card card, CardPlayResult result) {
        if (result.IsSuccess && HandPresenter.Contains(card)) {
            HandPresenter.RemoveCard(card);
        } else {
            HandPresenter.SetInteractiveCard(card, true);
        }
        
        HandPresenter.UpdateCardsOrder();
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
        targetFiller.UnregisterSelector(Opponent.Id);
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
        handPresenter.OnHandCardClicked += OnCardClicked;
    }

    private void OnCardClicked(CardPresenter card) {
        if (card == null) return;
        handPresenter.SetInteractiveCard(card.Card, false);
        Presenter.cardPlayService.PlayCardAsync(card.Card).Forget();
    }


    public override void Exit() {
        base.Exit();

        if (handPresenter != null) {
            handPresenter.OnHandCardClicked -= OnCardClicked;
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
        SelectorPresenter = Presenter.SelectorService;


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
        handPresenter.SetInteractable(true);
    }
}