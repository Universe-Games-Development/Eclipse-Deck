using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public interface ICardPlayService {
    event Action<Card, CardPlayResult> OnCardPlayFinished;
    event Action<Card> OnCardActivated;

    UniTask<CardPlayResult> PlayCardAsync(Card card, CancellationToken cancellationToken = default);
    bool IsPlayingCard(Card card);
    void CancelCardPlay(Card card);
}

public class CardPlayService : ICardPlayService {
    private readonly IOperationManager _operationManager;
    private readonly IOperationFactory _operationFactory;
    private readonly ITargetFiller _targetFiller;
    private readonly IEventBus<IEvent> _eventBus;
    private readonly Dictionary<string, CardPlaySession> _activeSessions = new();

    public event Action<Card, CardPlayResult> OnCardPlayFinished;
    public event Action<Card> OnCardActivated;

    public CardPlayService(
        IOperationManager operationManager,
        IOperationFactory operationFactory,
        ITargetFiller targetFiller,
        IEventBus<IEvent> eventBus) {
        _operationManager = operationManager ?? throw new ArgumentNullException(nameof(operationManager));
        _operationFactory = operationFactory ?? throw new ArgumentNullException(nameof(operationFactory));
        _targetFiller = targetFiller ?? throw new ArgumentNullException(nameof(targetFiller));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public async UniTask<CardPlayResult> PlayCardAsync(Card card, CancellationToken cancellationToken = default) {
        if (card == null) throw new ArgumentNullException(nameof(card));

        if (IsPlayingCard(card)) {
            return CardPlayResult.Failed("Card is already being played");
        }

        var session = new CardPlaySession(
            card,
            _operationFactory,
            _operationManager,
            _targetFiller,
            _eventBus
        );

        _activeSessions[card.InstanceId] = session;

        // 🔥 Підписуємось на активацію
        session.OnCardActivated += () => OnCardActivated?.Invoke(card);

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
    private readonly ITargetFiller _targetFiller;
    private readonly IEventBus<IEvent> _eventBus;
    private readonly List<OperationData> _operations;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private int _currentOperationIndex = 0;
    private bool _isDisposed = false;
    private bool _isActivated = false;

    private const int MAX_OPERATIONS_WARNING_THRESHOLD = 50;

    public event Action OnCardActivated;

    public CardPlaySession(
        Card card,
        IOperationFactory operationFactory,
        IOperationManager operationManager,
        ITargetFiller targetFiller,
        IEventBus<IEvent> eventBus) {
        _card = card ?? throw new ArgumentNullException(nameof(card));
        _operationFactory = operationFactory ?? throw new ArgumentNullException(nameof(operationFactory));
        _operationManager = operationManager ?? throw new ArgumentNullException(nameof(operationManager));
        _targetFiller = targetFiller ?? throw new ArgumentNullException(nameof(targetFiller));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        _operations = card.GetOperationData()?.ToList() ?? new List<OperationData>();

        if (_operations.Count > MAX_OPERATIONS_WARNING_THRESHOLD) {
            Debug.LogWarning($"Card {card.Data?.Name} has {_operations.Count} operations");
        }
    }

    public async UniTask<CardPlayResult> ExecuteAsync(CancellationToken externalToken) {
        if (_isDisposed) throw new ObjectDisposedException(nameof(CardPlaySession));

        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            externalToken, _cancellationTokenSource.Token);
        var token = linkedTokenSource.Token;

        try {
            _eventBus.Raise(new CardPlaySessionStartedEvent(_card, _operations.Count));

            // Валідація
            var validationResult = ValidateOperations();
            if (!validationResult.IsValid) {
                return FinishSession(CardPlayResult.Failed(validationResult.ErrorMessage));
            }

            // Виконуємо операції по черзі
            while (HasNextOperation() && !token.IsCancellationRequested) {
                var operationResult = await ProcessNextOperationAsync(token);
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
        }
    }

    /// <summary>
    /// Обробляє наступну операцію:
    /// 1. Створює операцію
    /// 2. Заповнює цілі (TargetFiller)
    /// 3. Активує карту (ПЕРЕД ПЕРШОЮ ОПЕРАЦІЄЮ)
    /// 4. Відправляє в OperationManager
    /// </summary>
    private async UniTask<OperationResult> ProcessNextOperationAsync(CancellationToken token) {
        var operationData = _operations[_currentOperationIndex];
        var isFirstOperation = _currentOperationIndex == 0;
        _currentOperationIndex++;

        try {
            // 1️⃣ Створюємо операцію
            var operation = _operationFactory.Create(operationData, _card);
            if (operation == null) {
                return OperationResult.Failure("Failed to create operation");
            }

            // 2️⃣ Заповнюємо цілі
            var targetResult = await FillOperationTargetsAsync(operation, token);
            if (!targetResult.IsSuccess) {
                return OperationResult.Failure($"Target filling failed: {targetResult.Message}");
            }

            // 3️⃣ АКТИВАЦІЯ КАРТИ (тільки перед першою операцією!)
            if (isFirstOperation) {
                ActivateCard();
            }

            // 4️⃣ Відправляємо в OperationManager
            return await PushOperationToManagerAsync(operation, token);

        } catch (OperationCanceledException) {
            return OperationResult.Failure("Operation cancelled");
        }
    }

    /// <summary>
    /// Заповнює цілі для операції через TargetFiller
    /// </summary>
    private async UniTask<OperationResult> FillOperationTargetsAsync(
        GameOperation operation,
        CancellationToken token) {

        var targets = operation.GetTargets();

        // Перевіряємо чи можна взагалі заповнити цілі
        if (!_targetFiller.CanFillTargets(targets, _card.OwnerId)) {
            return OperationResult.Failure("Cannot fill targets - no valid targets available");
        }

        // Запитуємо заповнення цілей
        var request = new TargetOperationRequest(
            targets,
            operation.IsMandatory,
            operation.Source
        );

        var result = await _targetFiller.FillTargetsAsync(request, token);

        if (result == null) {
            return OperationResult.Failure("Target filling was cancelled");
        }

        // Встановлюємо заповнені цілі
        operation.SetTargets(result.FilledTargets);

        if (!operation.IsReady()) {
            return OperationResult.Failure("Operation is not ready after target filling");
        }

        return OperationResult.Success();
    }

    /// <summary>
    /// Відправляє готову операцію в OperationManager
    /// </summary>
    private async UniTask<OperationResult> PushOperationToManagerAsync(
        GameOperation operation,
        CancellationToken token) {

        var tcs = new UniTaskCompletionSource<OperationResult>();

        using var subscription = new OperationCompletionHandler(_operationManager, operation, tcs);

        // OperationManager отримує ГОТОВУ операцію з заповненими цілями
        // Він тільки валідує і виконує
        _operationManager.Push(operation);

        return await tcs.Task.AttachExternalCancellation(token);
    }

    /// <summary>
    /// Активує карту - викликається ОДИН раз перед першою операцією
    /// ТУТ витрачаються ресурси (мана) і карта йде з руки
    /// </summary>
    private void ActivateCard() {
        if (_isActivated) return;

        _isActivated = true;

        Debug.Log($"💳 CARD ACTIVATED: {_card.Data?.Name} (Owner: {_card.OwnerId})");

        // Викликаємо події
        _eventBus.Raise(new CardActivatedEvent(_card));
        OnCardActivated?.Invoke();
    }

    private ValidationResult ValidateOperations() {
        if (_operations.Count == 0) {
            return ValidationResult.Invalid($"Card {_card.Data?.Name} has no operations");
        }
        return ValidationResult.Valid();
    }

    private bool HasNextOperation() => _currentOperationIndex < _operations.Count;

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
        OnCardActivated = null;
        _isDisposed = true;
        _cancellationTokenSource?.Dispose();
    }

    #region Helper Classes

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

    #endregion
}
