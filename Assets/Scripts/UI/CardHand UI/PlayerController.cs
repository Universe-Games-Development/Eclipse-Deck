using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] public OperationManager operationManager;
    [SerializeField] public CardPlayModule cardPlayModule;
    [SerializeField] public BoardPlayer player;
    [SerializeField] public HandPresenter handPresenter;

    private PlayerState currentState;

    private void Start() {
        if (!ValidateComponents()) return;

        SubscribeToEvents();
        SwitchState(new IdleState());
    }

    private bool ValidateComponents() {
        handPresenter ??= GetComponent<HandPresenter>();

        if (handPresenter == null) {
            Debug.LogError("HandPresenter is not assigned to PlayerController.");
            return false;
        }

        if (operationManager == null) {
            Debug.LogError("OperationManager is not assigned to PlayerController.");
            return false;
        }

        if (cardPlayModule == null) {
            Debug.LogError("CardPlayModule is not assigned to PlayerController.");
            return false;
        }

        return true;
    }

    private void SubscribeToEvents() {
        cardPlayModule.OnCardPlayCompleted += OnCardPlayCompleted;
        player.Selector.OnSelectionStarted += HandleSelectionStart;
    }

    private void HandleSelectionStart(TargetSelectionRequest request) {
        SwitchState(new PlayingState());
    }

    private void Update() {
        currentState?.UpdateState();
    }

    private void OnDestroy() {
        UnsubscribeFromEvents();
        currentState?.Exit();
    }

    private void UnsubscribeFromEvents() {
        if (cardPlayModule != null)
            cardPlayModule.OnCardPlayCompleted -= OnCardPlayCompleted;

        if (player?.Selector != null)
            player.Selector.OnSelectionStarted -= HandleSelectionStart;
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
        currentState.controller = this;
        currentState.Enter();

        //Debug.Log($"State changed to: {currentState.GetType().Name}");
    }

    private void OnCardPlayCompleted(CardPresenter presenter, bool success) {
        if (presenter?.Card == null || handPresenter == null)
            return;

        Card spentCard = presenter.Card;
        if (!handPresenter.Contains(spentCard))
            return;

        if (success) {
            handPresenter.RemoveCard(spentCard);
            player.SpendMana(spentCard.Cost.Current);
            GameLogger.Log("Card successfully played and spent");
        } else {
            GameLogger.Log("Card play failed, returning to hand");
            handPresenter.UpdateCardOrder(presenter);
        }
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
    // Implementation here
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
    }

    private void OnCardClicked(CardPresenter presenter) {
        if (presenter?.Card == null) return;

        controller.SwitchState(new PlayingState(presenter));
    }

    private void OnCardHovered(CardPresenter presenter, bool isHovered) {
        if (presenter == null) return;

        if (isHovered) {
            handPresenter.SetHoveredCard(presenter);
        } else {
            handPresenter.ClearHoveredCard();
        }
    }

    public override void Exit() {
        base.Exit();

        if (handPresenter != null) {
            handPresenter.OnCardClicked -= OnCardClicked;
            handPresenter.OnCardHovered -= OnCardHovered;
        }
    }
}

public class PlayingState : PlayerState {
    private CancellationTokenSource _debounceCts;
    private readonly CardPresenter presenter;
    private readonly TimeSpan _debounceTime = TimeSpan.FromMilliseconds(500);
    private bool _isExiting = false;

    public PlayingState(CardPresenter presenter = null) {
        this.presenter = presenter;
    }

    public override void Enter() {
        base.Enter();

        if (controller?.operationManager == null || controller?.cardPlayModule == null) {
            Debug.LogError("Required components are missing in PlayingState");
            return;
        }

        controller.operationManager.OnQueueEmpty += TryExitPlaying;
        controller.cardPlayModule.OnCardPlayCompleted += OnCardPlayCompleted;

        _debounceCts = new CancellationTokenSource();

        if (presenter != null) {
            controller.cardPlayModule.StartCardPlay(presenter, CancellationToken.None);
        }
    }

    private void OnCardPlayCompleted(CardPresenter presenter, bool success) {
        TryExitPlaying();
    }

    private async void TryExitPlaying() {
        if (_isExiting) return; // Запобігаємо race condition

        // Скасовуємо попередню операцію
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();

        if (_isExiting) return; // Перевіряємо ще раз після dispose

        _debounceCts = new CancellationTokenSource();

        try {
            await UniTask.Delay(_debounceTime, cancellationToken: _debounceCts.Token);

            if (!_isExiting && controller?.operationManager?.IsQueueEmpty() == true) {
                controller.SwitchState(new IdleState());
            }
        } catch (OperationCanceledException) {
            // Нормальне скасування для debounce
        }
    }

    public override void Exit() {
        base.Exit();

        _isExiting = true;

        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;

        if (controller?.operationManager != null)
            controller.operationManager.OnQueueEmpty -= TryExitPlaying;

        if (controller?.cardPlayModule != null)
            controller.cardPlayModule.OnCardPlayCompleted -= OnCardPlayCompleted;
    }
}