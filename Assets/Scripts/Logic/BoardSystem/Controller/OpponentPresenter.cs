using System;
using UnityEngine;
using Zenject;


// Please dont watch this class under the influence of alcohol
// EnemyPresenter Will be ITargetSelectionService
public class OpponentPresenter : UnitPresenter, IDisposable {
    
    [Inject] public IOperationManager operationManager;
    [Inject] public IEventBus<IEvent> eventBus;
    [Inject] protected readonly IPresenterFactory presenterFactory;
    [Inject] public IUnitRegistry unitRegistry;

    public Opponent Opponent { get; private set; }
    public OpponentView OpponentView;

    public OpponentPresenter(Opponent opponent, OpponentView opponentView) : base(opponent, opponentView) {
        Opponent = opponent;
        OpponentView = opponentView;
    }

    public virtual void Initialize() {
        HandPresenter handPresenter = presenterFactory.CreateUnitPresenter<HandPresenter>(OpponentView.HandDisplay, Opponent.Hand);
        DeckPresenter deckPresenter = presenterFactory.CreateUnitPresenter<DeckPresenter>(OpponentView.DeckDisplay, Opponent.Deck);
        unitRegistry.Register(handPresenter);
        unitRegistry.Register(deckPresenter);
        Opponent.FillDeckWithRandomCards(20);
    }

    
    public void DrawCards(int drawAmount) {
        for (int i = 0; i < drawAmount; i++) {
            Opponent.DrawCard();
        }
    }

    public void PlayCard(string cardId) {
        if (!Opponent.Hand.TryGetCardById(cardId, out Card card)) {
            Debug.Log("Failed to find card: " + cardId);
            return;
        }
        Opponent.PlayCard(card);
    }

    public virtual void Dispose() {
    }
}

public class PlayerPresenter : OpponentPresenter {
    public PlayerView PlayerView;
    
    //public event Action OnEndTurnClicked;

    private readonly ITargetSelectionService selectionService;

    private PlayerState currentState;
    private PlayerState previousState;

    public PlayerPresenter(Opponent opponent, PlayerView playerView, ITargetSelectionService selectionService) : base(opponent, playerView) {
        PlayerView = playerView;

        this.selectionService = selectionService;
    }

    public override void Initialize() {
        base.Initialize();
        
        Opponent.OnCardPlayStarted += OnCardPlayStarted;
        Opponent.OnCardPlayFinished += OnCardPlayFinished;
        SwitchState(new IdleState()); // will react on turns soon to change states
    }


    private void OnCardPlayStarted(Card card) {
        SwitchState(new TargetingState());
    }

    private void OnCardPlayFinished(Card card, CardPlayResult result) {
        PlayerView.UpdateHandCardsOrder();
        ReturnToPreviousState();
    }

    public void SwitchState(PlayerState newState) {
        if (newState == null) {
            Debug.LogError("Attempting to switch to null state");
            return;
        }

        if (currentState?.GetType() == newState.GetType()) {
            return;
        }
        previousState = currentState ?? new PassiveState();
        currentState?.Exit();

        currentState = newState;
        currentState.Initialize(this, PlayerView, selectionService);
        currentState.Enter();

        Debug.Log($"State changed to: {currentState.GetType().Name}");
    }

    public void ReturnToPreviousState() {
        if (previousState != null) {
            SwitchState(previousState);
        } else {
            SwitchState(new IdleState());
        }
    }

    public override void Dispose() {
        base.Dispose();
        currentState?.Exit();

        Opponent.OnCardPlayStarted -= OnCardPlayStarted;
        Opponent.OnCardPlayFinished -= OnCardPlayFinished;
    }
}


public abstract class PlayerState : State {
    protected PlayerPresenter Presenter { get; private set; }
    protected PlayerView PlayerView { get; private set; }
    protected ITargetSelectionService SelectionService { get; private set; }

    public void Initialize(
    PlayerPresenter presenter,
    PlayerView playerView,
    ITargetSelectionService selectionService) {
        Presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        PlayerView = playerView ?? throw new ArgumentNullException(nameof(playerView));
        SelectionService = selectionService ?? throw new ArgumentNullException(nameof(selectionService));
    }
}

public class PassiveState : PlayerState {
    // Гравець не може нічого робити (не його хід)
    public override void Enter() {
        base.Enter();
        Debug.Log("Player is in passive state (not their turn)");
    }

    public override void Exit() {
        base.Exit();
    }
}

public class IdleState : PlayerState {
    public override void Enter() {
        base.Enter();
        PlayerView.OnCardClicked += HandleCardSelection;
        PlayerView.SetInteractableHand(true);
    }

    private void HandleCardSelection(string cardId) {
        if (cardId == null) return;
        Presenter.PlayCard(cardId);
    }

    public override void Exit() {
        base.Exit();
        PlayerView.OnCardClicked -= HandleCardSelection;
    }
}

public class TargetingState : PlayerState {
    public override void Enter() {
        base.Enter();
        PlayerView.SetInteractableHand(false);
        //Debug.Log("Player is busy selecting target");
    }

    public override void Exit() {
        base.Exit();
    }
}
