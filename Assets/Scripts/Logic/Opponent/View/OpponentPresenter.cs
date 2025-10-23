using System;
using UnityEngine;
using Zenject;

public abstract class OpponentPresenter : UnitPresenter, IDisposable {
    [Inject] protected IEventBus<IEvent> eventBus;

    public Opponent Opponent { get; private set; }
    public OpponentView OpponentView;

    public OpponentPresenter(Opponent opponent, OpponentView opponentView) : base(opponent, opponentView) {
        Opponent = opponent;
        OpponentView = opponentView;

        Opponent.FillDeck();
    }

    public void DrawCards(int drawAmount) {
        for (int i = 0; i < drawAmount; i++) {
            Opponent.DrawCard();
        }
    }

    public void PlayCard(string cardId) {
        if (!Opponent.Hand.TryGetCard(cardId, out Card card)) {
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

    private PlayerState currentState;
    private PlayerState previousState;

    public PlayerPresenter(Opponent opponent, PlayerView playerView) : base(opponent, playerView) {
        PlayerView = playerView;

        SwitchState(new IdleState());
        Opponent.OnCardPlayStarted += OnCardPlayStarted;
        Opponent.OnCardPlayFinished += OnCardPlayFinished;
        playerView.OnCardDrawRequest += DrawCard;
        playerView.OnCardTestRemoveRequest += RemoveCard;
    }

    private void RemoveCard() {
        Opponent.DiscardCard();
    }

    private void DrawCard() {
        Opponent.DrawCard();
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
        currentState.Initialize(this, PlayerView);
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

    public void Initialize(
    PlayerPresenter presenter,
    PlayerView playerView) {
        Presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        PlayerView = playerView ?? throw new ArgumentNullException(nameof(playerView));
    }
}

// Гравець не може нічого робити (не його хід)
public class PassiveState : PlayerState {
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
