using System.Collections;
using System.Linq;
using UnityEngine;

public class CardPlayModule : MonoBehaviour {
    [Header("Dependencies")]
    [SerializeField] private HandPresenter _handPresenter;
    [SerializeField] private OperationManager _operationManager;
    [SerializeField] private BoardInputManager _boardInputManager;
    [SerializeField] private BoardPlayer _player;

    [Header("Visual Settings")]
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Transform _cursorIndicator;
    [SerializeField] private float _movementSpeed = 1f;
    [SerializeField] private Vector3 _cardOffset = new Vector3(0f, 1.2f, 0f);

    private bool _isPlaying = false;
    private CardPlayData _playData;

    private void Start() {
        _handPresenter.OnCardClicked += OnCardClicked;
        _operationManager.OnOperationStatus += HandleOperationStatus;
    }

    private void OnCardClicked(CardPresenter cardPresenter) {
        if (_isPlaying || cardPresenter == null) {
            GameLogger.LogWarning("Card click ignored - already playing or null card");
            return;
        }

        _playData = new(cardPresenter);
        _isPlaying = true;

        // Подаем только первую операцию
        SubmitNextOperation();
    }

    private void Update() {
        if (_isPlaying) {
            if (_boardInputManager.TryGetCursorPosition(_layerMask, out Vector3 cursorPosition)) {
                UpdateCardPosition(cursorPosition);
            }
           
            //Debug.Log(cursorPosition);
        }
    }

    private void UpdateCardPosition(Vector3 cursorPosition) {
        if (_playData?.View == null) return;

        _cursorIndicator.transform.position = cursorPosition;
        var targetPosition = cursorPosition + _cardOffset;
        _playData.View.transform.position = Vector3.Lerp(
            _playData.View.transform.position,
            targetPosition,
            Time.deltaTime * _movementSpeed);
    }

    private void HandleOperationStatus(GameOperation operation, OperationStatus status) {
        if (!_isPlaying || _playData == null) return;
        if (!_playData.Card.Operations.Contains(operation)) return;

        switch (status) {
            case OperationStatus.Success:
                _playData.IsStarted = true;
                StartCoroutine(WaitAndSubmitNextOperation());
                break;
            case OperationStatus.Canceled:
            case OperationStatus.Failed:
                HandleOperationCanceled(operation);
                break;
        }
    }

    private void HandleOperationCanceled(GameOperation operation) {
        if (_playData != null && !_playData.IsStarted) {
            RollbackCardPlay();
        } else {
            StartCoroutine(WaitAndSubmitNextOperation());
        }
    }

    private IEnumerator WaitAndSubmitNextOperation() {
        // Чекаємо завершення поточної операції
        yield return new WaitUntil(() => !_operationManager.IsRunning);

        // Додаткова затримка для стабільності
        yield return new WaitForSeconds(0.05f);

        SubmitNextOperation();
    }

    private void RollbackCardPlay() {
        _handPresenter.RetrieveCard(_playData.Presenter);

        GameLogger.LogDebug($"Card {_playData.Card.Data.Name} returned to hand");
        FinishCardPlay();
    }

    private void SubmitNextOperation() {
        if (_playData?.HasNextOperation() == true) {
            var nextOperation = _playData.GetNextOperation();
            nextOperation.Initiator = _player;
            _operationManager.Push(nextOperation);
        } else {
            FinishCardPlay();
        }
    }

    private void FinishCardPlay() {
        _playData = null;
        _isPlaying = false;
    }
}

public class CardPlayData {
    public Card Card;
    public CardPresenter Presenter;
    public Card3DView View;
    public bool IsStarted = false;
    public int CurrentOperationIndex = 0;
    public int CompletedOperations = 0;

    public CardPlayData(CardPresenter presenter) {
        Card = presenter.Card;
        View = presenter.View as Card3DView;
        Presenter = presenter;
    }

    public bool HasNextOperation() => CurrentOperationIndex < Card.Operations.Count;

    public GameOperation GetNextOperation() {
        if (HasNextOperation()) {
            return Card.Operations[CurrentOperationIndex++];
        }
        return null;
    }

    public bool IsLastOperation(GameOperation operation) {
        return Card.Operations.LastOrDefault() == operation;
    }
}

public struct CardPlayedEvent : IEvent {
    public Card PlayedCard { get; }
    public BoardPlayer PlayedBy { get; }
    public bool WasSuccessful { get; }

    public CardPlayedEvent(Card playedCard, BoardPlayer playedBy, bool wasSuccessful) {
        PlayedCard = playedCard;
        PlayedBy = playedBy;
        WasSuccessful = wasSuccessful;
    }
}