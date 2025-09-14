using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour {
    [SerializeField] public OperationManager operationManager;
    [SerializeField] public BoardPlayer player;
    [SerializeField] public HandPresenter handPresenter;

    [Inject] public ICardPlayService cardPlayService;
    [Inject] public IEventBus<IEvent> eventBus;

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

        if (cardPlayService == null) {
            Debug.LogError("CardPlayModule is not assigned to PlayerController.");
            return false;
        }

        return true;
    }

    private void SubscribeToEvents() {
        operationManager.OnOperationStatus += HandleOperationStatus;
        player.Selector.OnSelectionStarted += HandleSelectionStart;
        eventBus.SubscribeTo<CardPlayStatusEvent>(HandleCardPlayCompleted);
    }

    private void HandleCardPlayCompleted(ref CardPlayStatusEvent eventData) {
        Card spentCard = eventData.Card;
        if (!handPresenter.Contains(spentCard))
            return;

        if (eventData.playResult.IsSuccess && false) {
            player.SpendMana(spentCard.Cost.Current);
            handPresenter.RemoveCard(spentCard);
            GameLogger.Log("Card successfully played and spent");
        } else {
            GameLogger.Log("Card play failed, returning to hand");
            handPresenter.UpdateCardsOrder();
        }
    }

    private void HandleOperationStatus(GameOperation operation, OperationStatus status) {
        switch (status) {
            case OperationStatus.Start:
                if (operation.Source is Card card && cardPlayService.IsPlayingCard(card)) {
                    player.SpendMana(card.Cost.Current);
                    handPresenter.RemoveCard(card);
                }
                break;
        }
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

        Debug.Log($"State changed to: {currentState.GetType().Name}");
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

        controller.SwitchState(new PlayingState(presenter.Card));
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
    private readonly Card _card;
    private readonly TimeSpan _debounceTime = TimeSpan.FromMilliseconds(500);
    private bool _isExiting = false;

    public PlayingState(Card card = null) {
        _card = card;
    }

    public override void Enter() {
        base.Enter();

        if (controller?.operationManager == null || controller?.cardPlayService == null) {
            Debug.LogError("Required components are missing in PlayingState");
            return;
        }

        controller.operationManager.OnQueueEmpty += TryExitPlaying;

        _debounceCts = new CancellationTokenSource();

        if (_card != null) {
            controller.cardPlayService.PlayCardAsync(_card, CancellationToken.None);
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
    }
}