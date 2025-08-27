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

        ChangeState(new IdleState());
    }

    private void HandleSelectionStart(ITargetRequirement requirement) {
        ChangeState(new PlayingState());
    }

    private void Update() {
        currentState.UpdateState();
    }

    private void OnDestroy() {
        cardPlayModule.OnCardPlayCompleted -= OnCardPlayCompleted;
    }

    public void ChangeState(PlayerState newState) {
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
        handPresenter.UpdateCardPositions();
    }

    private void OnCardPlayCompleted(CardPresenter presenter, bool success) {
        if (!handPresenter.Contains(presenter)) return;

        if (success) {
            // Spend card - карта була успішно зіграна
            handPresenter.RemoveCard(presenter);
            GameLogger.Log("Card successfully played and spent");
        } else {
            // Return card - карта не була зіграна, повертаємо в руку
            GameLogger.Log("Card play failed, returning to hand");
            RetrieveCard(presenter);
        }
        ChangeState(new IdleState());
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
        handPresenter = controller.handPresenter;
        handPresenter.OnCardClicked += OnCardClicked;
        handPresenter.OnCardHovered += OnCardHovered;
    }

    private void OnCardClicked(CardPresenter presenter) {
        Debug.Log($"Card clicked: {presenter.Card.Data.Name}");
        controller.cardPlayModule.StartCardPlay(presenter, controller.player, CancellationToken.None);
    }

    private void OnCardHovered(CardPresenter presenter, bool isHovered) {
        //Debug.Log($"Card hovered: {presenter.Card.Data.Name}");
    }

    public override void Exit() {
        base.Exit();
        handPresenter.OnCardClicked -= OnCardClicked;
        handPresenter.OnCardHovered -= OnCardHovered;
    }
}

public class PlayingState : PlayerState {
    private CancellationTokenSource _debounceCts;
    private readonly TimeSpan _debounceTime = TimeSpan.FromMilliseconds(500);

    public override void Enter() {
        base.Enter();
        controller.operationManager.OnQueueEmpty += HandleQueueEmpty;
        _debounceCts = new CancellationTokenSource();
    }

    private async void HandleQueueEmpty() {
        // Скасовуємо попередню debounce операцію
        _debounceCts.Cancel();
        _debounceCts.Dispose();
        _debounceCts = new CancellationTokenSource();

        try {
            await UniTask.Delay(_debounceTime, cancellationToken: _debounceCts.Token);

            if (controller.operationManager.IsQueueEmpty()) {
                controller.ChangeState(new IdleState());
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