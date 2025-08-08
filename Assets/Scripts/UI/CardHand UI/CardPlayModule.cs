using UnityEngine;
using Zenject;

public class CardPlayModule : MonoBehaviour {
    [SerializeField] HandPresenter handPresenter;
    [SerializeField] OperationManager operationManager;

    [SerializeField] BoardInputManager boardInputManager;
    [SerializeField] private Transform _testObject;

    private bool isPlaying = false;
    private CardPlayData playData;

    private void Start() {
        handPresenter.OnCardClicked += OnCardClicked;

        operationManager.OnOperationEnd += HandleOperationEnd;
    }

    private void Update() {
        if (isPlaying) {
            if (boardInputManager.TryGetBoardCursorPosition(out Vector3 cursorPosition))
            if (playData.View != null) {
                    _testObject.transform.position = cursorPosition;
                    Vector3 cardOffset = new Vector3(0, 1.3f, 0);
                    playData.View.transform.position = cursorPosition + cardOffset;
            }
            //Debug.Log(cursorPosition);
        }
    }

    private void HandleOperationEnd(GameOperation operation, OperationStatus status) {
        // if we not active
        if (!isPlaying || playData == null) return;

        // if its not ours
        if (!playData.Card.Operations.Contains(operation)) {
            return;
        }

        switch (status) {
            case OperationStatus.Success:
                playData.IsStarted = true;
                break;
            case OperationStatus.Canceled:
                HandleOperationCanceled(operation);
                break;
            case OperationStatus.Failed:
                Debug.Log("Failed operation: " + operation);
                break;
        }

        if (operation == playData.EndOperation) {
            FinishCardPlay();
            // soon add card played event
        }
    }

    private void HandleOperationCanceled(GameOperation operation) {
        if (!playData.IsStarted) {
            var presenter = playData.Presenter; // зберігаємо перед очищенням
            operationManager.PopDefinedRange(playData.Card.Operations);
            isPlaying = false;
            playData = null;
            handPresenter.RetrieveCard(presenter); // використовуємо збережене значення
        }
    }

    private void OnCardClicked(CardPresenter cardPresenter) {
        if (isPlaying) {
            return;
        }

        handPresenter.Hand.Remove(cardPresenter.Card);

        playData = new(cardPresenter);
        isPlaying = true;
        playData.EndOperation = playData.Card.Operations[0];
        operationManager.PushRange(playData.Card.Operations);
    }

    private void FinishCardPlay() {
        playData = null;
        isPlaying = false;
    }
}

public class CardPlayData {
    public Card Card;
    public CardPresenter Presenter;
    public Card3DView View;
    public bool IsStarted = false;
    public CardPlayData(CardPresenter presenter) {
        Card = presenter.Card; ;
        View = presenter.View as Card3DView;
        Presenter = presenter;
    }

    public GameOperation EndOperation { get; internal set; }
}

public struct CardPlayedEvent : IEvent {
    public Card playedCard;
    // think how to collect data "by who played"
}
