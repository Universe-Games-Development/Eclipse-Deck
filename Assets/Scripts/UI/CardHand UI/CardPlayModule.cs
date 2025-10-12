using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public interface ICardPlayService {
    event Action<Card, CardPlayResult> OnCardPlayFinished;
    UniTask<CardPlayResult> PlayCardAsync(Card card, CancellationToken cancellationToken = default);
    bool IsPlayingCard(Card card);
    void CancelCardPlay(Card card);
}

public class CardPlayService : ICardPlayService {
    private readonly IOperationManager _operationManager;
    private readonly IOperationFactory _operationFactory;
    private readonly IEventBus<IEvent> _eventBus;
    private readonly Dictionary<string, CardPlaySession> _activeSessions = new();

    public event Action<Card, CardPlayResult> OnCardPlayFinished;

    public CardPlayService(
        IOperationManager operationManager,
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
        _activeSessions[card.InstanceId] = session;

        try {
            var result = await session.ExecuteAsync(cancellationToken);
            OnCardPlayFinished?.Invoke(card, result);
            return result;
        } finally {
            session.Dispose();
            _activeSessions.Remove(card.InstanceId);
        }
    }

    public bool IsPlayingCard(Card card) => card != null && _activeSessions.ContainsKey(card.InstanceId);

    public void CancelCardPlay(Card card) {
        if (card != null && _activeSessions.TryGetValue(card.InstanceId, out var session)) {
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
    private bool _isDisposed = false;

    // Константи для налаштувань
    private const int OPERATION_MANAGER_TIMEOUT_SECONDS = 5;
    private const int MAX_OPERATIONS_WARNING_THRESHOLD = 50; // Попередження якщо занадто багато операцій

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

        if (_operations.Count > MAX_OPERATIONS_WARNING_THRESHOLD) {
            Debug.LogWarning($"Card {card.Data?.Name} has {_operations.Count} operations, which might be too many");
        }
    }

    public async UniTask<CardPlayResult> ExecuteAsync(CancellationToken externalToken) {
        if (_isDisposed) throw new ObjectDisposedException(nameof(CardPlaySession));

        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            externalToken, _cancellationTokenSource.Token);

        var token = linkedTokenSource.Token;

        try {
            _eventBus.Raise(new CardPlaySessionStartedEvent(_card, _operations.Count));

            var validationResult = ValidateOperations();
            if (!validationResult.IsValid) {
                return FinishSession(CardPlayResult.Failed(validationResult.ErrorMessage));
            }

            while (HasNextOperation() && !token.IsCancellationRequested) {
                var operationResult = await ExecuteNextOperationAsync(token);

                RaiseOperationResultEvent(_currentOperationIndex - 1, operationResult);

                if (!operationResult.IsSuccess) {
                    return FinishSession(CardPlayResult.Failed(operationResult.Message));
                }
            }

            if (token.IsCancellationRequested) {
                return FinishSession(CardPlayResult.Cancelled());
            }

            return FinishSession(CardPlayResult.Success(_currentOperationIndex));

        } catch (OperationCanceledException) {
            return FinishSession(CardPlayResult.Cancelled());
        } catch (Exception ex) {
            Debug.LogError($"Card play failed with exception: {ex}");
            return FinishSession(CardPlayResult.Failed(ex.Message));
        }
    }

    private ValidationResult ValidateOperations() {
        if (_operations.Count == 0) {
            return ValidationResult.Invalid($"Card {_card.Data?.Name} has no operations");
        }


        return ValidationResult.Valid();
    }

    private bool HasNextOperation() => _currentOperationIndex < _operations.Count;

    private async UniTask<OperationResult> ExecuteNextOperationAsync(CancellationToken token) {
        var operationData = _operations[_currentOperationIndex];
        _currentOperationIndex++;

        try {
            await WaitForOperationManagerAsync(token);

            
            var operation = _operationFactory.Create(operationData, _card);
            return await ExecuteOperationAsync(operation, token);

        } catch (OperationCanceledException) {
            return OperationResult.Failure("Operation cancelled");
        } catch (TimeoutException) {
            return OperationResult.Failure($"Operation manager timeout after {OPERATION_MANAGER_TIMEOUT_SECONDS}s");
        } catch (Exception ex) {
            Debug.LogError($"Failed to execute operation at index {_currentOperationIndex - 1}: {ex}");
            return OperationResult.Failure(ex.Message);
        }
    }


    private async UniTask WaitForOperationManagerAsync(CancellationToken token) {
        // Чекаємо поки менеджер операцій не звільниться
        await UniTask.WaitUntil(
            () => !_operationManager.IsRunning,
            cancellationToken: token,
            timing: PlayerLoopTiming.Update
        ).Timeout(TimeSpan.FromSeconds(OPERATION_MANAGER_TIMEOUT_SECONDS));
    }

    private async UniTask<OperationResult> ExecuteOperationAsync(GameOperation operation, CancellationToken token) {
        var tcs = new UniTaskCompletionSource<OperationResult>();

        using var subscription = new OperationCompletionHandler(_operationManager, operation, tcs);
        _operationManager.Push(operation);

        return await tcs.Task.AttachExternalCancellation(token);
    }

    private void RaiseOperationResultEvent(int operationIndex, OperationResult result) {
        _eventBus.Raise(new CardOperationResultEvent(
            _card,
            operationIndex,
            _operations.Count,
            result
        ));
    }

    private CardPlayResult FinishSession(CardPlayResult result) {
        _eventBus.Raise(new CardPlaySessionEndedEvent(_card, result));
        return result;
    }

    public void Cancel() => _cancellationTokenSource.Cancel();

    public void Dispose() {
        if (_isDisposed) return;

        _isDisposed = true;
        _cancellationTokenSource?.Dispose();
    }

    // Допоміжна структура для валідації
    private readonly struct ValidationResult {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        private ValidationResult(bool isValid, string errorMessage = null) {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Valid() => new(true);
        public static ValidationResult Invalid(string message) => new(false, message);
    }

    // Спрощена підписка на завершення операції
    private sealed class OperationCompletionHandler : IDisposable {
        private readonly IOperationManager _manager;
        private readonly GameOperation _targetOperation;
        private readonly UniTaskCompletionSource<OperationResult> _completionSource;
        private Action<GameOperation, OperationResult> _handler;

        public OperationCompletionHandler(
            IOperationManager manager,
            GameOperation operation,
            UniTaskCompletionSource<OperationResult> completionSource) {
            _manager = manager;
            _targetOperation = operation;
            _completionSource = completionSource;

            _handler = OnOperationCompleted;
            _manager.OnOperationEnd += _handler;
        }

        private void OnOperationCompleted(GameOperation operation, OperationResult result) {
            if (operation == _targetOperation) {
                _completionSource.TrySetResult(result);
            }
        }

        public void Dispose() {
            if (_handler != null) {
                _manager.OnOperationEnd -= _handler;
                _handler = null;
            }
        }
    }
}

#region Results and Events

public readonly struct CardPlayResult {
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

    public bool IsFailed => !IsSuccess && !IsCancelled;
}

public readonly struct CardOperationResultEvent : IEvent {
    public Card Card { get; }
    public int OperationIndex { get; }
    public int TotalOperations { get; }
    public OperationResult Result { get; }
    public DateTime CompletedTime { get; }

    public CardOperationResultEvent(Card card, int operationIndex, int totalOperations, OperationResult result) {
        Card = card;
        OperationIndex = operationIndex;
        TotalOperations = totalOperations;
        Result = result;
        CompletedTime = DateTime.UtcNow; // Краще використовувати UTC
    }

    public bool IsLastOperation => OperationIndex == TotalOperations - 1;
    public float Progress => TotalOperations > 0 ? (float)(OperationIndex + 1) / TotalOperations : 0f;
}

public readonly struct CardPlaySessionStartedEvent : IEvent {
    public Card Card { get; }
    public int TotalOperations { get; }
    public DateTime StartTime { get; }

    public CardPlaySessionStartedEvent(Card card, int totalOperations) {
        Card = card;
        TotalOperations = totalOperations;
        StartTime = DateTime.UtcNow;
    }
}

public readonly struct CardPlaySessionEndedEvent : IEvent {
    public Card Card { get; }
    public CardPlayResult FinalResult { get; }
    public DateTime EndTime { get; }

    public CardPlaySessionEndedEvent(Card card, CardPlayResult finalResult) {
        Card = card;
        FinalResult = finalResult;
        EndTime = DateTime.UtcNow;
    }

    public bool WasSuccessful => FinalResult.IsSuccess;
    public bool WasCancelled => FinalResult.IsCancelled;
    public bool WasFailed => FinalResult.IsFailed;
}

#endregion