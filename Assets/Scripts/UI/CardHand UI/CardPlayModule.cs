using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class CardPlayModule : MonoBehaviour {
    [Header("Dependencies")]
    [SerializeField] private OperationManager _operationManager;

    private CardPlayData _playData;
    private CancellationToken _currentCancellationToken;

    public event System.Action<CardPresenter, bool> OnCardPlayCompleted;
    public event System.Action<CardPresenter> OnCardPlayStarted;

    private CancellationTokenSource _internalTokenSource;

    private void Awake() {
        if (_operationManager == null) {
            _operationManager = FindFirstObjectByType<OperationManager>();
        }
    }

    public void StartCardPlay(CardPresenter cardPresenter, CancellationToken externalToken = default) {
        if (IsPlaying() || cardPresenter == null) {
            OnCardPlayCompleted?.Invoke(cardPresenter, false);
            return;
        }

        _playData = new CardPlayData(cardPresenter);
        OnCardPlayStarted?.Invoke(cardPresenter);

        _internalTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        _currentCancellationToken = _internalTokenSource.Token;

        _operationManager.OnOperationStatus += HandleOperationStatus;
        WaitAndSubmitNextOperation(_currentCancellationToken).Forget();
    }

    public void CancelCardPlay() {
        if (!IsPlaying()) return;
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
                nextOperation.Initiator = _playData.CardPresenter;

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
        List<GameOperation> operations = _playData.CardPresenter.Card.Operations;
        if (!IsPlaying() || !operations.Contains(operation)) return;

        switch (status) {
            case OperationStatus.Success:
                _playData.IsStarted = true;
                _playData.CompletedOperations++;
                WaitAndSubmitNextOperation(_currentCancellationToken).Forget();
                break;

            case OperationStatus.Cancelled:
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
        if (!IsPlaying()) return;

        _internalTokenSource?.Cancel();
        _internalTokenSource?.Dispose();
        _internalTokenSource = null;

        if (_operationManager != null) {
            _operationManager.OnOperationStatus -= HandleOperationStatus;
        }
        _operationManager.OnOperationStatus -= HandleOperationStatus;

        GameLogger.Log($"Card play finished: {_playData.IsStarted}, Completed: {_playData.CompletedOperations} / {_playData.Operations.Count}", LogLevel.Debug, LogCategory.CardModule);
        
        OnCardPlayCompleted?.Invoke(_playData.CardPresenter, _playData.IsStarted);
        _playData = null;
    }

    public bool IsPlaying() {
        return _playData != null;
    }

    public CardPlayData GetCurrentPlayData() {
        return _playData;
    }
}

public class CardPlayData {
    public CardPresenter CardPresenter;
    public bool IsStarted = false;
    public int CurrentOperationIndex = 0;
    public int CompletedOperations = 0;
    public List<GameOperation> Operations;

    public CardPlayData(CardPresenter presenter) {
        CardPresenter = presenter;
        Operations = presenter.Card.Operations;
    }

    public bool HasNextOperation() => CurrentOperationIndex < Operations.Count;

    public GameOperation GetNextOperation() {
        if (HasNextOperation() && CardPresenter != null && Operations != null) {
            return Operations[CurrentOperationIndex++];
        }
        return null;
    }

    public bool IsLastOperation(GameOperation operation) {
        return Operations?.LastOrDefault() == operation;
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