using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public interface ICardPlayService {
    UniTask<CardPlayResult> PlayCardAsync(Card card, CancellationToken cancellationToken = default);
    bool IsPlayingCard(Card card);
    void CancelCardPlay(Card card);
}

public class CardPlayService : ICardPlayService {
    private readonly OperationManager _operationManager;
    private readonly IOperationFactory _operationFactory;
    private readonly IEventBus<IEvent> _eventBus;
    private readonly Dictionary<string, CardPlaySession> _activeSessions = new();

    public CardPlayService(
        OperationManager operationManager,
        IOperationFactory operationFactory,
        IEventBus<IEvent> eventBus) {

        _operationManager = operationManager ?? throw new ArgumentNullException(nameof(operationManager));
        _operationFactory = operationFactory ?? throw new ArgumentNullException(nameof(operationFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public async UniTask<CardPlayResult> PlayCardAsync(Card card, CancellationToken cancellationToken = default) {
        if (card == null) throw new ArgumentNullException(nameof(card));

        if (IsPlayingCard(card)) {
            return CardPlayResult.Failed("Card is already being played");
        }

        var session = new CardPlaySession(card, _operationFactory, _operationManager, _eventBus);
        _activeSessions[card.Id] = session;

        try {
            var result = await session.ExecuteAsync(cancellationToken);
            return result;
        } finally {
            _activeSessions.Remove(card.Id);
        }
    }

    public bool IsPlayingCard(Card card) {
        return card != null && _activeSessions.ContainsKey(card.Id);
    }

    public void CancelCardPlay(Card card) {
        if (card != null && _activeSessions.TryGetValue(card.Id, out var session)) {
            session.Cancel();
        }
    }
}

public class CardPlaySession : IDisposable {
    private readonly Card _card;
    private readonly IOperationFactory _operationFactory;
    private readonly IOperationManager _operationManager;
    private readonly IEventBus<IEvent> _eventBus;
    private readonly List<OperationData> _operations;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private int _currentOperationIndex = 0;
    private int _completedOperations = 0;

    private bool _isDisposed = false;
    public CardPlaySession(
        Card card,
        IOperationFactory operationFactory,
        IOperationManager operationManager,
        IEventBus<IEvent> eventBus) {

        _card = card ?? throw new ArgumentNullException(nameof(card));
        _operationFactory = operationFactory ?? throw new ArgumentNullException(nameof(operationFactory));
        _operationManager = operationManager ?? throw new ArgumentNullException(nameof(operationManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        _operations = card.GetOperationData()?.ToList() ?? new List<OperationData>();
    }

    public async UniTask<CardPlayResult> ExecuteAsync(CancellationToken externalToken) {
        if (_isDisposed) throw new ObjectDisposedException(nameof(CardPlaySession));

        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            externalToken, _cancellationTokenSource.Token);

        var token = linkedTokenSource.Token;

        try {
            _eventBus.Raise(new CardPlayStartedEvent(_card));

            if (!ValidateOperations()) {
                var failedResult = CardPlayResult.Failed("Invalid operations for card");
                _eventBus.Raise(new CardPlayStatusEvent(_card, failedResult));
                return failedResult;
            }

            while (HasNextOperation() && !token.IsCancellationRequested) {
                var operationResult = await ExecuteNextOperationAsync(token);

                if (!operationResult.IsSuccess) {
                    var failedResult = CardPlayResult.Failed(operationResult.ErrorMessage);
                    _eventBus.Raise(new CardPlayStatusEvent(_card, failedResult));
                    return failedResult;
                }

                _completedOperations++;
            }

            var successResult = CardPlayResult.Success(_completedOperations);
            _eventBus.Raise(new CardPlayStatusEvent(_card, successResult));
            return successResult;

        } catch (OperationCanceledException) {
            var cancelledResult = CardPlayResult.Cancelled();
            _eventBus.Raise(new CardPlayStatusEvent(_card, cancelledResult));
            return cancelledResult;
        } catch (Exception ex) {
            Debug.LogError($"Card play failed with exception: {ex}");
            var failedResult = CardPlayResult.Failed(ex.Message);
            _eventBus.Raise(new CardPlayStatusEvent(_card, failedResult));
            return failedResult;
        }
    }

    private bool ValidateOperations() {
        if (_operations.Count == 0) {
            Debug.LogWarning($"Card {_card.Data?.Name} has no operations");
            return false;
        }

        // Перевіряємо чи всі операції можна створити
        return _operations.All(op => _operationFactory.CanCreate(op.GetType()));
    }

    private bool HasNextOperation() {
        return _currentOperationIndex < _operations.Count;
    }

    private async UniTask<OperationResult> ExecuteNextOperationAsync(CancellationToken token) {
        var operationData = _operations[_currentOperationIndex];
        var currentIndex = _currentOperationIndex;
        _currentOperationIndex++;

        try {
            await WaitForOperationManager(token);

            var operation = _operationFactory.Create(operationData);
            operation.SetSource(_card);

            return await ExecuteOperationWithCompletionSource(operation, currentIndex, token);
        } catch (OperationCanceledException) {
            return OperationResult.Failed("Operation canceled");
        } catch (TimeoutException) {
            return OperationResult.Failed("Operation manager timeout");
        } catch (Exception ex) {
            Debug.LogError($"Failed to execute operation: {ex}");
            return OperationResult.Failed(ex.Message);
        }
    }


    private async UniTask WaitForOperationManager(CancellationToken token) {
        await UniTask.WaitUntil(
            () => !_operationManager.IsRunning,
            cancellationToken: token,
            timing: PlayerLoopTiming.Update
        ).Timeout(TimeSpan.FromSeconds(5));
    }

    private async UniTask<OperationResult> ExecuteOperationWithCompletionSource(
        GameOperation operation, int currentIndex, CancellationToken token) {
        var tcs = new UniTaskCompletionSource<OperationResult>();
        using var subscription = new OperationStatusSubscription(_operationManager, operation, tcs);

        _operationManager.Push(operation);

        return await tcs.Task.AttachExternalCancellation(token); ;
    }


    public void Cancel() {
        _cancellationTokenSource.Cancel();
    }

    public void Dispose() {
        _cancellationTokenSource?.Dispose();
    }

    // Допоміжний клас для управління підпискою
    private class OperationStatusSubscription : IDisposable {
        private readonly IOperationManager _manager;
        private readonly GameOperation _operation;
        private readonly UniTaskCompletionSource<OperationResult> _tcs;
        private Action<GameOperation, OperationStatus> _handler;

        public OperationStatusSubscription(
            IOperationManager manager,
            GameOperation operation,
            UniTaskCompletionSource<OperationResult> tcs) {
            _manager = manager;
            _operation = operation;
            _tcs = tcs;

            _handler = (op, status) => {
                if (op != _operation) return;

                switch (status) {
                    case OperationStatus.Success:
                        _tcs.TrySetResult(OperationResult.Success());
                        break;
                    case OperationStatus.Failed:
                        _tcs.TrySetResult(OperationResult.Failed("Operation failed"));
                        break;
                    case OperationStatus.Cancelled:
                        _tcs.TrySetCanceled();
                        break;
                    case OperationStatus.ThrownException:
                        _tcs.TrySetResult(OperationResult.Failed("Operation threw exception"));
                        break;
                }
            };

            _manager.OnOperationStatus += _handler;
        }

        public void Dispose() {
            if (_handler != null) {
                _manager.OnOperationStatus -= _handler;
                _handler = null;
            }
        }
    }
}

#region Results and Events

public struct CardPlayResult {
    public bool IsSuccess { get; }
    public bool IsCancelled { get; }
    public string ErrorMessage { get; }
    public int CompletedOperations { get; }

    private CardPlayResult(bool isSuccess, bool isCancelled, string errorMessage, int completedOperations) {
        IsSuccess = isSuccess;
        IsCancelled = isCancelled;
        ErrorMessage = errorMessage;
        CompletedOperations = completedOperations;
    }

    public static CardPlayResult Success(int completedOperations) =>
        new(true, false, null, completedOperations);

    public static CardPlayResult Failed(string errorMessage) =>
        new(false, false, errorMessage, 0);

    public static CardPlayResult Cancelled() =>
        new(false, true, "Cancelled", 0);
}

public struct OperationResult {
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }

    private OperationResult(bool isSuccess, string errorMessage) {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static OperationResult Success() => new(true, null);
    public static OperationResult Failed(string errorMessage) => new(false, errorMessage);
}

// Events
public struct CardPlayStartedEvent : IEvent {
    public Card Card { get; }
    public CardPlayStartedEvent(Card card) => Card = card;
}

public struct CardPlayStatusEvent : IEvent {
    public Card Card { get; }
    public CardPlayResult playResult;

    public CardPlayStatusEvent(Card card, CardPlayResult result) {
        Card = card;
        playResult = result;
    }
}

public struct CardOperationCompletedEvent : IEvent {
    public Card Card { get; }
    public int OperationIndex { get; }
    public int TotalOperations { get; }

    public CardOperationCompletedEvent(Card card, int operationIndex, int totalOperations) {
        Card = card;
        OperationIndex = operationIndex;
        TotalOperations = totalOperations;
    }
}
#endregion
