using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading;
using UnityEngine;

public class CardPlayModule : MonoBehaviour {
    [Header("Dependencies")]
    [SerializeField] private OperationManager _operationManager;

    private bool _isPlaying = false;
    private CardPlayData _playData;
    private CancellationToken _currentCancellationToken;

    public event System.Action<CardPresenter, bool> OnCardPlayCompleted;
    public bool IsPlaying => _isPlaying;
    private CancellationTokenSource _internalTokenSource;

    private void Awake() {
        if (_operationManager == null) {
            _operationManager = FindFirstObjectByType<OperationManager>();
        }
    }

    public void StartCardPlay(CardPresenter cardPresenter, BoardPlayer initiator, CancellationToken externalToken = default) {
        if (_isPlaying || cardPresenter == null) {
            OnCardPlayCompleted?.Invoke(cardPresenter, false);
            return;
        }

        _playData = new CardPlayData(cardPresenter, initiator);
        _isPlaying = true;

        _internalTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        _currentCancellationToken = _internalTokenSource.Token;

        _operationManager.OnOperationStatus += HandleOperationStatus;
        WaitAndSubmitNextOperation(_currentCancellationToken).Forget();
    }

    public void CancelCardPlay() {
        if (!_isPlaying) return;
        FinishCardPlay();
    }

    private async UniTask WaitAndSubmitNextOperation(CancellationToken cancellationToken) {
        try {
            await UniTask.WaitUntil(() => !_operationManager.IsRunning,
                cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested) {
                FinishCardPlay();
                return;
            }

            if (_playData?.HasNextOperation() == true) {
                var nextOperation = _playData.GetNextOperation();
                nextOperation.Initiator = _playData.Initiator;

                if (!cancellationToken.IsCancellationRequested) {
                    _operationManager.Push(nextOperation);
                }
            } else {
                FinishCardPlay();
            }
        } catch (System.OperationCanceledException) {
            FinishCardPlay();
        }
    }

    private void HandleOperationStatus(GameOperation operation, OperationStatus status) {
        if (!_isPlaying || !_playData.Card.Operations.Contains(operation)) return;

        switch (status) {
            case OperationStatus.Success:
                _playData.IsStarted = true;
                _playData.CompletedOperations++;
                WaitAndSubmitNextOperation(_currentCancellationToken).Forget();
                break;

            case OperationStatus.Canceled:
            case OperationStatus.Failed:
                if (!_playData.IsStarted) {
                    // Перша операція не вдалася - карта не зіграна
                    FinishCardPlay();
                } else {
                    WaitAndSubmitNextOperation(_currentCancellationToken).Forget();
                }
                break;
        }
    }

    private void FinishCardPlay() {
        if (!_isPlaying) return;

        _isPlaying = false;
        _internalTokenSource?.Cancel();
        _internalTokenSource?.Dispose();
        _internalTokenSource = null;

        if (_operationManager != null) {
            _operationManager.OnOperationStatus -= HandleOperationStatus;
        }
        _operationManager.OnOperationStatus -= HandleOperationStatus;

        GameLogger.Log($"Card play finished. Success: {_playData.IsStarted}, Completed: {_playData.CompletedOperations} / {_playData.Card.Operations.Count}");
        
        OnCardPlayCompleted?.Invoke(_playData.Presenter, _playData.IsStarted);
        _playData = null;
    }
}

public class CardPlayData {
    public Card Card;
    public CardPresenter Presenter;
    public Card3DView View;
    public bool IsStarted = false;
    public int CurrentOperationIndex = 0;
    public int CompletedOperations = 0;
    public BoardPlayer Initiator;

    public CardPlayData(CardPresenter presenter, BoardPlayer initiator) {
        Card = presenter.Card;
        View = presenter.View as Card3DView;
        Presenter = presenter;
        Initiator = initiator;
    }

    public bool HasNextOperation() => CurrentOperationIndex < Card.Operations.Count;

    public GameOperation GetNextOperation() {
        if (HasNextOperation() && Card != null && Card.Operations != null) {
            return Card.Operations[CurrentOperationIndex++];
        }
        return null;
    }

    public bool IsLastOperation(GameOperation operation) {
        return Card?.Operations?.LastOrDefault() == operation;
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