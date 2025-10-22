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
    bool IsPlayingCard();
    void CancelCardPlay();
}

// Module can play only 1 card at a time
public class CardPlayService : ICardPlayService, IDisposable {
    private readonly IOperationExecutor _operationExecutor;
    private readonly IEventBus<IEvent> _eventBus;
    private readonly ITargetFiller _targetFiller;

    public event Action<Card, CardPlayResult> OnCardPlayFinished;
    public event Action<Card> OnCardActivated;

    private Card _currentCard = null;
    private List<OperationData> _operations;
    private readonly CancellationTokenSource _globalCancellationSource = new();


    public CardPlayService(
        IOperationExecutor operationExecutor,
        IEventBus<IEvent> eventBus,
        ITargetFiller targetFiller) {
        _operationExecutor = operationExecutor ?? throw new ArgumentNullException(nameof(operationExecutor));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _targetFiller = targetFiller;
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

            if (_operations.Count == 0) {
                var result = CardPlayResult.Failed("No operations defined for card");
                CompleteCardPlay(result);
                return result;
            }

            int completedOperations = 0;
            bool anyOperationStarted = false;

            for (int i = 0; i < _operations.Count && !token.IsCancellationRequested; i++) {

                TargetsFillResult fillResult = await _targetFiller.FillTargetsAsync(_operations[i], _currentCard, cancellationToken);
                TargetRegistry targetRegistry = fillResult.TargetRegistry;

                if (!fillResult.Status) {
                    if (anyOperationStarted) {
                        continue;
                    } else {
                        var cancelledResult = CardPlayResult.Cancelled();
                        CompleteCardPlay(cancelledResult);
                        return cancelledResult;
                    }
                }


                if (_operations[i] is SummonOperationData summon) {
                    Zone zone = targetRegistry.Get<Zone>(TargetKeys.MainTarget);
                    if (zone.IsFull()) {
                        bool isSacrificed = await HandleCardSummon(summon.SacrificeOperationData);
                        if (!isSacrificed)
                            continue;
                    }
                }

                targetRegistry.Add(TargetKeys.SourceCard, card);

                // Execute operation through executor
                OperationResult opResult = await _operationExecutor.ExecuteAsync(
                    _operations[i],
                    targetRegistry,
                    token);

                // If target filling was cancelled, skip this operation
                if (!opResult.IsSuccess) {
                    if (opResult.Cancelled) {
                        continue; // Skip to next operation
                    }
                    // Other errors - stop card play
                    var failedResult = CardPlayResult.Failed(opResult.Message);
                    CompleteCardPlay(failedResult);
                    return failedResult;
                }

                completedOperations++;

                // Notify card activation on first successful operation
                if (!anyOperationStarted) {
                    anyOperationStarted = true;
                    OnCardActivated?.Invoke(card);
                }
            }

            if (token.IsCancellationRequested) {
                var cancelledResult = CardPlayResult.Cancelled();
                CompleteCardPlay(cancelledResult);
                return cancelledResult;
            }

            var successResult = CardPlayResult.Success(completedOperations);
            CompleteCardPlay(successResult);
            return successResult;

        } catch (OperationCanceledException) {
            var cancelledResult = CardPlayResult.Cancelled();
            CompleteCardPlay(cancelledResult);
            return cancelledResult;
        } finally {
            Cleanup();
        }
    }

    private async UniTask<bool> HandleCardSummon(SacrificeOperationData sacrificeOperationData) {
        if (sacrificeOperationData == null) {
            Debug.LogWarning("Sacrifice Gained nulls");
            return false;
        }

        TargetsFillResult targetsFillResult = await _targetFiller.FillTargetsAsync(sacrificeOperationData, _currentCard, CancellationToken.None);
        if (!targetsFillResult.Status) {
            return false;
        }
        // Execute sacrifice operation
        OperationResult sacrificeResult = await _operationExecutor.ExecuteAsync(sacrificeOperationData, targetsFillResult.TargetRegistry, CancellationToken.None);
        return sacrificeResult.IsSuccess;
    }

    private void CompleteCardPlay(CardPlayResult result) {
        OnCardPlayFinished?.Invoke(_currentCard, result);
    }

    private void Cleanup() {
        _currentCard = null;
        _operations = null;
    }

    public bool IsPlayingCard() => _currentCard != null;

    public void CancelCardPlay() {
        if (IsPlayingCard()) {
            _globalCancellationSource.Cancel();
        }
    }

    public void Dispose() {
        _globalCancellationSource?.Cancel();
        _globalCancellationSource?.Dispose();
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
