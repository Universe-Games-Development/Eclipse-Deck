using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;


public class OperationTargetsFiller : MonoBehaviour, ITargetFiller {

    [SerializeField] private int tryAttempts = 3;
    [SerializeField] private float operationTimeoutSeconds = 30f;

    [Inject] private readonly ITargetValidator targetValidator;
    [Inject] private readonly ILogger logger;
    [Inject] private readonly IOpponentRegistry opponentRegistry;

    private readonly Dictionary<string, ITargetSelectionService> registeredSelectors = new();
    private readonly CancellationTokenSource globalCancellationSource = new();
    private ITargetSelectionService fallbackSelector;

    private void Awake() {
        fallbackSelector = new HumanTargetSelector(); // soon be randomSelector
    }

    private void OnDestroy() {
        globalCancellationSource?.Cancel();
        globalCancellationSource?.Dispose();
    }

    public bool CanFillTargets(List<TypedTargetBase> targets) {
        return targets?.Any() == true && targetValidator.CanValidateAllTargets(targets);
    }

    public async UniTask<TargetOperationResult> FillTargetsAsync(TargetOperationRequest request, CancellationToken cancellationToken = default) {

        if (!IsValidRequest(request)) {
            return TargetOperationResult.Failure("Invalid request parameters");
        }

        using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(operationTimeoutSeconds));
        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, globalCancellationSource.Token, timeoutSource.Token);

        logger.LogInfo($"Starting target operation: {request.Targets.Count} targets, mandatory: {request.IsMandatory}", LogCategory.TargetsFiller);

        try {
            var result = await ProcessTargetsAsync(request, combinedTokenSource.Token);
            logger.LogInfo($"Target operation completed: {result.Status}", LogCategory.TargetsFiller);
            return result;
        } catch (OperationCanceledException) {
            logger.LogInfo("Target operation cancelled", LogCategory.TargetsFiller);
            return TargetOperationResult.Cancelled();
        }
    }

    private async UniTask<TargetOperationResult> ProcessTargetsAsync(TargetOperationRequest request, CancellationToken cancellationToken) {
        var startTime = DateTime.UtcNow;
        var filledTargets = new Dictionary<string, object>();

        for (int i = 0; i < request.Targets.Count; i++) {
            var target = request.Targets[i];
            var selector = GetSelectorForTarget(target, request.Source.OwnerId);

            var result = await TryFillTargetAsync(selector, target, request, cancellationToken);

            if (result.success && result.unit != null) {
                filledTargets[target.Key] = result.unit;
            } else if (request.IsMandatory && result.unit == null) {
                return TargetOperationResult.Failure($"Failed to fill mandatory target: {target.Key}");
            }
        }

        var duration = DateTime.UtcNow - startTime;

        if (filledTargets.Count == request.Targets.Count) {
            return TargetOperationResult.Success(filledTargets, duration);
        } else if (filledTargets.Count > 0) {
            return TargetOperationResult.PartialSuccess(filledTargets, duration);
        } else {
            return TargetOperationResult.Failure("No targets were filled");
        }
    }

    private async UniTask<(bool success, object unit)> TryFillTargetAsync(
        ITargetSelectionService selector,
        TypedTargetBase target,
        TargetOperationRequest request,
        CancellationToken cancellationToken) {

        for (int attempt = 0; attempt < tryAttempts; attempt++) {
            try {
                var selectedUnit = await selector.SelectTargetAsync(
                    new TargetSelectionRequest(request.Source, target),
                    cancellationToken);

                if (selectedUnit == null) {
                    return request.IsMandatory ? (false, null) : (true, null);
                }

                var validationResult = target.IsValid(selectedUnit, new ValidationContext(request.Source.OwnerId));
                if (validationResult.IsValid) {
                    return (true, selectedUnit);
                }

                logger.LogWarning($"Invalid target {selectedUnit} for {target.Key}: {validationResult.ErrorMessage}", LogCategory.TargetsFiller);

                if (!request.IsMandatory && attempt == tryAttempts - 1) {
                    return (true, null); // Skip optional target after max retries
                }

            } catch (OperationCanceledException) {
                throw;
            }
        }

        return (false, null);
    }

    private ITargetSelectionService GetSelectorForTarget(TypedTargetBase typeTarget, string initiatorId) {
        var selectorType = typeTarget.GetTargetSelector();

        return selectorType switch {
            TargetSelector.Initiator => registeredSelectors.GetValueOrDefault(initiatorId, fallbackSelector),
            TargetSelector.Opponent => GetOpponentSelector(initiatorId),
            _ => fallbackSelector
        };
    }

    private ITargetSelectionService GetOpponentSelector(string initiatorId) {
        var opponent = opponentRegistry.GetAgainstOpponentId(initiatorId);
        return registeredSelectors.GetValueOrDefault(opponent?.Id, fallbackSelector);
    }

    private bool IsValidRequest(TargetOperationRequest request) {
        if (request?.Targets?.Any() != true) {
            logger.LogWarning("Cannot process empty target request", LogCategory.TargetsFiller);
            return false;
        }

        if (request.Source == null) {
            logger.LogWarning("Request requires source but none provided", LogCategory.TargetsFiller);
            return false;
        }

        return true;
    }

    public void RegisterSelector(string opponentId, ITargetSelectionService selectionService) {
        if (registeredSelectors.ContainsKey(opponentId)) {
            logger.LogWarning($"Selector already registered for player {opponentId}", LogCategory.TargetsFiller);
            return;
        }
        registeredSelectors[opponentId] = selectionService;
    }

    public void UnRegisterSelector(string opponentId) {
        if (!registeredSelectors.Remove(opponentId)) {
            logger.LogWarning($"No selector found to unregister for player {opponentId}", LogCategory.TargetsFiller);
        }
    }
}

// === Simplified Data Objects ===

public class TargetOperationRequest {
    public TargetOperationRequest(List<TypedTargetBase> namedTargets, bool isMandatory, UnitModel source) {
        Targets = namedTargets ?? throw new ArgumentNullException(nameof(namedTargets));
        IsMandatory = isMandatory;
        Source = source ?? throw new ArgumentNullException(nameof(source));
        OperationId = Guid.NewGuid().ToString();
    }

    public List<TypedTargetBase> Targets { get; }
    public bool IsMandatory { get; }
    public UnitModel Source { get; }
    public string OperationId { get; }
}

public class TargetOperationResult {
    public OperationStatus Status { get; private set; }
    public IReadOnlyDictionary<string, object> FilledTargets { get; private set; }
    public string ErrorMessage { get; private set; }
    public TimeSpan Duration { get; private set; }

    private TargetOperationResult(OperationStatus status, Dictionary<string, object> targets = null, string error = null, TimeSpan duration = default) {
        Status = status;
        FilledTargets = targets ?? new Dictionary<string, object>();
        ErrorMessage = error;
        Duration = duration;
    }

    public static TargetOperationResult Success(Dictionary<string, object> targets, TimeSpan duration) =>
        new(OperationStatus.Success, targets, duration: duration);

    public static TargetOperationResult Failure(string error) =>
        new(OperationStatus.Failed, error: error);

    public static TargetOperationResult Cancelled() =>
        new(OperationStatus.Cancelled);

    public static TargetOperationResult PartialSuccess(Dictionary<string, object> targets, TimeSpan duration) =>
        new(OperationStatus.PartialSuccess, targets, duration: duration);
}

public enum TargetSelector {
    Initiator,  // Той, хто ініціював операцію
    Opponent,   // Опонент ініціатора
    AnyPlayer,  // Будь-який гравець (для рідкісних випадків)
    SpecificPlayer, // Для складних випадків (з можливістю вказати конкретного гравця)
    AllPlayers,
    NextPlayer
}

