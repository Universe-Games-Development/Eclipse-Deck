using System.Threading;
using UnityEngine;

public class PlayerController : MonoBehaviour{
    [SerializeField] public OperationManager operationManager;
    [SerializeField] public CardPlayModule cardPlayModule;
    
    [SerializeField] public BoardPlayer player;

    public HandPresenter handPresenter;

    private PlayerState currentState;

    [Header("Playing State Settings")]
    [SerializeField] public BoardInputManager boardInputManager;
    
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


    public void RetrieveCard(CardPresenter presenter) {
        Debug.Log("Need return card");
        handPresenter.UpdateCardPositions();
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
    private CardPresenter _cardPresenter;
    private CancellationTokenSource _cancellationTokenSource;
    private Vector3 lastBoardPosition;

    public PlayingState(CardPresenter presenter) {
        _cardPresenter = presenter;
    }

    public override void Enter() {
        base.Enter();

        _cancellationTokenSource = new CancellationTokenSource();
        controller.cardPlayModule.OnCardPlayCompleted += OnCardPlayCompleted;
        controller.cardPlayModule.StartCardPlay(_cardPresenter, controller.player, _cancellationTokenSource.Token);
    }

    public override void UpdateState() {
        base.UpdateState();
        if (controller.boardInputManager.TryGetCursorPosition(controller.boardMask, out Vector3 cursorPosition)) {
            lastBoardPosition = cursorPosition;
            controller.cursorIndicator.transform.position = lastBoardPosition;
        }
        HandleCardMovement();
    }

    private void HandleCardMovement() {
        
        var targetPosition = lastBoardPosition + controller.cardOffset;

        _cardPresenter.transform.position = Vector3.Lerp(
            _cardPresenter.transform.position,
            targetPosition,
            Time.deltaTime * controller.movementSpeed);
    }

    public override void Exit() {
        base.Exit();

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;

        controller.cardPlayModule.OnCardPlayCompleted -= OnCardPlayCompleted;
    }

    private void OnCardPlayCompleted(CardPresenter presenter, bool success) {
        if (presenter != _cardPresenter) return;

        if (success) {
            // Spend card - карта була успішно зіграна
            controller.handPresenter.Hand.Remove(_cardPresenter.Card);
            GameLogger.Log("Card successfully played and spent");
        } else {
            // Return card - карта не була зіграна, повертаємо в руку
            GameLogger.Log("Card play failed, returning to hand");
            controller.RetrieveCard(_cardPresenter);
        }
        controller.ChangeState(new IdleState());
    }
}
