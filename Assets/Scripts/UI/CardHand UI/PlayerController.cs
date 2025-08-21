using Cysharp.Threading.Tasks;
using System.Collections;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine;

public class PlayerController : MonoBehaviour{
    [SerializeField] public OperationManager operationManager;
    [SerializeField] public BoardPlayer player;

    public HandPresenter handPresenter;

    private PlayerState currentState;

    [SerializeField] public BoardInputManager boardInputManager;
    [Header("Visual Settings")]
    [SerializeField] public LayerMask boardMask;
    [SerializeField] public Transform cursorIndicator;
    [SerializeField] public float movementSpeed = 1f;
    [SerializeField] public Vector3 cardOffset = new Vector3(0f, 1.2f, 0f);

    private void Start() {
        handPresenter ??= GetComponent<HandPresenter>();
        if (handPresenter == null) {
            Debug.LogError("HandPresenter is not assigned to PlayerController.");
            return;
        }
        ChangeState(new IdleState());
    }

    private void Update() {
        currentState.UpdateState();
    }

    public void ChangeState(PlayerState newState) {
        if (currentState != null && currentState.GetType() == newState.GetType()) {
            return; // Не менять состояние, если оно уже такое же
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

}

public class IdleState : PlayerState {
    private HandPresenter handPresenter;
    public override void Enter() {
        base.Enter();
        handPresenter = controller.handPresenter;
        handPresenter.OnCardClicked += OnCardClicked;
        handPresenter.OnCardHovered += OnCardHovered;
        // Здесь можно добавить логику, которая выполняется при входе в состояние Playing
    }

    private void OnCardClicked(CardPresenter presenter) {
        Debug.Log($"Card clicked: {presenter.Card.Data.Name}");
        controller.ChangeState(new PlayingState(presenter));
    }

    private void OnCardHovered(CardPresenter presenter, bool isHovered) {
        //Debug.Log($"Card hovered: {presenter.Card.Data.Name}");
    }

    public override void Exit() {
        base.Exit();
        handPresenter.OnCardClicked -= OnCardClicked;
        handPresenter.OnCardHovered -= OnCardHovered;
        // Здесь можно добавить логику, которая выполняется при выходе из состояния Playing
    }
}

public class PlayingState : PlayerState {
    private CardPlayData _playData;
    private CardPresenter cardPresenter;
    private CancellationTokenSource _cancellationTokenSource;

    public PlayingState(CardPresenter presenter) {
        this.cardPresenter = presenter;
    }

    public override void Enter() {
        base.Enter();
        _playData = new(cardPresenter);
        _cancellationTokenSource = new CancellationTokenSource();
        controller.operationManager.OnOperationStatus += HandleOperationStatus;

        WaitAndSubmitNextOperation(_cancellationTokenSource.Token).Forget();
    }

    public override void UpdateState() {
        base.UpdateState();
        HandleCardMovement();
    }

    private void HandleCardMovement() {
        if (controller.boardInputManager.TryGetCursorPosition(controller.boardMask, out Vector3 cursorPosition)) {
            controller.cursorIndicator.transform.position = cursorPosition;
            var targetPosition = cursorPosition + controller.cardOffset;
            _playData.View.transform.position = Vector3.Lerp(
                _playData.View.transform.position,
                targetPosition,
                Time.deltaTime * controller.movementSpeed);
        }
    }

    public override void Exit() {
        base.Exit();

        // Скасовуємо всі поточні задачі
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        controller.operationManager.OnOperationStatus -= HandleOperationStatus;

        if (_playData.IsStarted) {
            // Spend card 
        } else {
            // Return card
        }

        _playData = null;
    }

    private async UniTask WaitAndSubmitNextOperation(CancellationToken cancellationToken) {
        await UniTask.WaitUntil(() => !controller.operationManager.IsRunning,
            cancellationToken: cancellationToken);

        if (cancellationToken.IsCancellationRequested) return;

        if (_playData?.HasNextOperation() == true) {
            var nextOperation = _playData.GetNextOperation();
            nextOperation.Initiator = controller.player;

            if (!cancellationToken.IsCancellationRequested)
            controller.operationManager.Push(nextOperation);
        } else {
            controller.ChangeState(new IdleState());
        }
    }


    private void HandleOperationStatus(GameOperation operation, OperationStatus status) {
        if (_cancellationTokenSource == null) return;

        if (!_playData.Card.Operations.Contains(operation)) return;

        switch (status) {
            case OperationStatus.Success:
                _playData.IsStarted = true;
                WaitAndSubmitNextOperation(_cancellationTokenSource.Token).Forget();
                break;
            case OperationStatus.Canceled:
            case OperationStatus.Failed:
                if (!_playData.IsStarted) {
                    controller.ChangeState(new IdleState());
                } else {
                    WaitAndSubmitNextOperation(_cancellationTokenSource.Token).Forget();
                }
                break;
        }
    }
}