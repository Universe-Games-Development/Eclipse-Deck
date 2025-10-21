using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public interface ICardPlayService {
    event Action<Card, CardPlayResult> OnCardPlayFinished;
    event Action<Card> OnCardActivated;

    UniTask<CardPlayResult> PlayCardAsync(Card card, CancellationToken cancellationToken = default);
    bool IsPlayingCard();
    void CancelCardPlay();
}

// Module can play only 1 card at a time
public class CardPlayService : ICardPlayService, IDisposable {
    private readonly IOperationManager _operationManager;
    private readonly IOperationFactory _operationFactory;
    private readonly ITargetFiller _targetFiller;
    private readonly IEventBus<IEvent> _eventBus;

    public event Action<Card, CardPlayResult> OnCardPlayFinished;
    public event Action<Card> OnCardActivated;

    private Card _currentCard = null;
    private List<OperationData> _operations;
    private int _currentOperationIndex = 0;
    private UniTaskCompletionSource _completionSource;
    private GameOperation _targetOperation;
    private readonly CancellationTokenSource _globalCancellationSource = new();

    public CardPlayService(
        IOperationManager operationManager,
        IOperationFactory operationFactory,
        ITargetFiller targetFiller,
        IEventBus<IEvent> eventBus) {
        _operationManager = operationManager ?? throw new ArgumentNullException(nameof(operationManager));
        _operationFactory = operationFactory ?? throw new ArgumentNullException(nameof(operationFactory));
        _targetFiller = targetFiller ?? throw new ArgumentNullException(nameof(targetFiller));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        _operationManager.OnQueueEmpty += OnManagerEmpty;
    }

    public async UniTask<CardPlayResult> PlayCardAsync(Card card, CancellationToken cancellationToken = default) {
        if (card == null) throw new ArgumentNullException(nameof(card));

        if (IsPlayingCard()) {
            var errorResult = CardPlayResult.Failed("Another card is already being played");
            OnCardPlayFinished?.Invoke(card, errorResult);
            return errorResult;
        }

        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _globalCancellationSource.Token);
        var token = combinedTokenSource.Token;

        try {
            _currentCard = card;
            _operations = card.GetOperationData()?.ToList() ?? new List<OperationData>();
            _currentOperationIndex = 0;

            if (_operations.Count == 0) {
                var result = CardPlayResult.Failed("No operations defined for card");
                CompleteCardPlay(result);
                return result;
            }

            bool firstOperationCompleted = false;

            for (int i = 0; i < _operations.Count && !token.IsCancellationRequested; i++) {
                _currentOperationIndex = i;

                // Fill targets for current operation
                TargetsFillResult fillResult = await _targetFiller.FillTargetsAsync(_operations[i], card, token);
                TargetRegistry targetRegistry = fillResult.TargetRegistry;

                // If player refused to fill targets, skip this operation
                if (!fillResult.Status) {
                    continue;
                }

                targetRegistry.Add(TargetKeys.SourceCard, card);
                _completionSource = new UniTaskCompletionSource();

                _targetOperation = _operations[i].CreateOperation(_operationFactory, targetRegistry);
                _operationManager.Push(_targetOperation);

                // Wait for operation completion
                await _completionSource.Task;

                // Mark card as activated after first successful operation execution
                if (!firstOperationCompleted) {
                    firstOperationCompleted = true;
                    OnCardActivated?.Invoke(card);
                }
            }

            if (token.IsCancellationRequested) {
                var cancelledResult = CardPlayResult.Failed("Card play was cancelled");
                CompleteCardPlay(cancelledResult);
                return cancelledResult;
            }

            var successResult = CardPlayResult.Success(_operations.Count);
            CompleteCardPlay(successResult);
            return successResult;
        } catch (OperationCanceledException) {
            var cancelledResult = CardPlayResult.Failed("Card play was cancelled");
            CompleteCardPlay(cancelledResult);
            return cancelledResult;
        } finally {
            Cleanup();
        }
    }

    private void OnManagerEmpty() {
        _completionSource?.TrySetResult();
    }

    private void CompleteCardPlay(CardPlayResult result) {
        OnCardPlayFinished?.Invoke(_currentCard, result);
    }

    private void Cleanup() {
        _currentCard = null;
        _operations = null;
        _currentOperationIndex = 0;
        _targetOperation = null;
        _completionSource = null;
    }

    public bool IsPlayingCard() => _currentCard != null;

    public void CancelCardPlay() {
        if (IsPlayingCard()) {
            _globalCancellationSource.Cancel();
            _completionSource?.TrySetCanceled();
        }
    }

    public void Dispose() {
        _operationManager.OnQueueEmpty -= OnManagerEmpty;
        _globalCancellationSource?.Cancel();
        _globalCancellationSource?.Dispose();
        _completionSource?.TrySetCanceled();
    }
}

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
