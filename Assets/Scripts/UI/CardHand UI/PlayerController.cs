using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerController : MonoBehaviour{
    [SerializeField] public OperationManager operationManager;
    [SerializeField] public CardPlayModule cardPlayModule;
    
    [SerializeField] public BoardPlayer player;

    public HandPresenter handPresenter;
    

    private PlayerState currentState;

    private void Start() {
        handPresenter ??= GetComponent<HandPresenter>();
        if (handPresenter == null) {
            Debug.LogError("HandPresenter is not assigned to PlayerController.");
            return;
        }

        cardPlayModule.OnCardPlayCompleted += OnCardPlayCompleted;
        player.Selector.OnSelectionStarted += HandleSelectionStart;

        SwitchState(new IdleState());
    }

    private void HandleSelectionStart(TargetSelectionRequest request) {
        SwitchState(new PlayingState());
    }

    private void Update() {
        currentState.UpdateState();
    }

    private void OnDestroy() {
        cardPlayModule.OnCardPlayCompleted -= OnCardPlayCompleted;
    }

    public void SwitchState(PlayerState newState) {
        if (currentState != null && currentState.GetType() == newState.GetType()) {
            return;
        }

        currentState?.Exit();
        currentState = newState;
        currentState.controller = this;
        currentState.Enter();

        Debug.Log($"State changed to: {currentState.GetType().Name}");
    }


    public void RetrieveCard(CardPresenter presenter) {
        handPresenter.UpdateCardsOrder();
    }

    private void OnCardPlayCompleted(CardPresenter presenter, bool success) {
        if (!handPresenter.Contains(presenter.Card)) return;

        if (success) {
            // Spend card - карта була успішно зіграна
            handPresenter.RemoveCard(presenter.Card);
            GameLogger.Log("Card successfully played and spent");
        } else {
            // Return card - карта не була зіграна, повертаємо в руку
            GameLogger.Log("Card play failed, returning to hand");
            RetrieveCard(presenter);
        }
        SwitchState(new IdleState());
    }
}


public abstract class State {
    public virtual void Enter() { }
    public virtual void UpdateState() { }
    public virtual void Exit() { }
}

public class PlayerState : State {
    public PlayerController controller;
}

public class PassiveState : PlayerState {

}

public class IdleState : PlayerState {
    private HandPresenter handPresenter;
    public override void Enter() {
        base.Enter();
        if (controller?.handPresenter == null) {
            Debug.LogError("HandPresenter is not available");
            return;
        }

        handPresenter = controller.handPresenter;
        handPresenter.OnCardClicked += OnCardClicked;
        handPresenter.OnCardHovered += OnCardHovered;
        handPresenter.SetInteractable(true);
    }

    private void OnCardClicked(CardPresenter presenter) {
        //Debug.Log($"Card clicked: {presenter.Card.Data.Name}");
        handPresenter.SetInteractable(false);
        controller.SwitchState(new PlayingState(presenter));
    }

    private void OnCardHovered(CardPresenter presenter, bool isHovered) {
        if (isHovered) {
            handPresenter.SetHoveredCard(presenter);
        } else {
            handPresenter.ClearHoveredCard();
        }
    }

    public override void Exit() {
        base.Exit();
        handPresenter.OnCardClicked -= OnCardClicked;
        handPresenter.OnCardHovered -= OnCardHovered;
    }
}

public class PlayingState : PlayerState {
    private CancellationTokenSource _debounceCts;
    private CardPresenter presenter;
    private readonly TimeSpan _debounceTime = TimeSpan.FromMilliseconds(500);

    public PlayingState() {
    }

    public PlayingState(CardPresenter presenter) {
        this.presenter = presenter;
    }

    public override void Enter() {
        base.Enter();
        controller.operationManager.OnQueueEmpty += HandleQueueEmpty;
        _debounceCts = new CancellationTokenSource();
        controller.handPresenter.SetInteractable(false);
        if (presenter != null) {
            controller.cardPlayModule.StartCardPlay(presenter, CancellationToken.None);
        }
    }

    private async void HandleQueueEmpty() {
        // Скасовуємо попередню debounce операцію
        _debounceCts.Cancel();
        _debounceCts.Dispose();
        _debounceCts = new CancellationTokenSource();

        try {
            await UniTask.Delay(_debounceTime, cancellationToken: _debounceCts.Token);

            if (controller.operationManager.IsQueueEmpty()) {
                controller.SwitchState(new IdleState());
            }
        } catch (OperationCanceledException) {
            // Ігноруємо скасування - це нормально для debounce
        }
    }

    public override void Exit() {
        base.Exit();
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;
        controller.operationManager.OnQueueEmpty -= HandleQueueEmpty;
    }
}