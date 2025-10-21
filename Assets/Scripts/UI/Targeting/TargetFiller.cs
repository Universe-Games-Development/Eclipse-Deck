using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public interface ITargetFiller {
    UniTask<TargetsFillResult> FillTargetsAsync(OperationData operationData, UnitModel requestSource, CancellationToken cancellationToken = default);
    bool CanFillTargets(OperationData operationData, string ownerId);
    void RegisterSelector(string playerId, ITargetSelectionService selectionService);
    void UnregisterSelector(string playerId);
}

public class OperationTargetsFiller : ITargetFiller {
    private readonly ILogger _logger;
    private readonly IOpponentRegistry _opponentRegistry;
    private readonly ITargetValidator _targetValidator;
    private readonly Dictionary<string, ITargetSelectionService> _registeredSelectors = new();
    private readonly CancellationTokenSource _globalCancellationSource = new();
    private readonly ITargetSelectionService _fallbackSelector;
    private readonly TimeSpan _selectorTimeout;

    // ✅ Налаштування retry
    private readonly int _maxRetryAttempts;
    private readonly TimeSpan _retryDelay;

    public OperationTargetsFiller(
            ITargetValidator targetValidator,
            ILogger logger,
            IOpponentRegistry opponentRegistry,
            ITargetSelectionService fallbackSelector = null,
            int maxRetryAttempts = 1,
            float selectorTimeoutSeconds = 10f,
            float retryDelaySeconds = 0.5f) {

        _targetValidator = targetValidator ?? throw new ArgumentNullException(nameof(targetValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _opponentRegistry = opponentRegistry ?? throw new ArgumentNullException(nameof(opponentRegistry));
        _fallbackSelector = fallbackSelector ?? new RandomTargetSelector();
        _selectorTimeout = TimeSpan.FromSeconds(selectorTimeoutSeconds);
        _maxRetryAttempts = maxRetryAttempts;
        _retryDelay = TimeSpan.FromSeconds(retryDelaySeconds);
    }

    public async UniTask<TargetsFillResult> FillTargetsAsync(
        OperationData operationData,
        UnitModel requestSource,
        CancellationToken cancellationToken = default) {
        if (!CanFillTargets(operationData, requestSource.OwnerId)) {
            return TargetsFillResult.Failure("Invalid request parameters");
        }

        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _globalCancellationSource.Token);

        var startTime = DateTime.UtcNow;
        var targets = new Dictionary<TargetKeys, object>();

        _logger.LogInfo(
            $"Starting target filling for {operationData.name}: {operationData.targetRequirements.Count} requirements",
            LogCategory.TargetsFiller
        );

        // ✅ Build всі runtime requirements спочатку
        var runtimeRequirements = operationData.BuildRuntimeRequirements();

        for (int i = 0; i < operationData.targetRequirements.Count; i++) {
            var requirementData = operationData.targetRequirements[i];
            var runtimeRequirement = runtimeRequirements[i];

            _logger.LogInfo(
                $"[{i + 1}/{operationData.targetRequirements.Count}] Filling target: " +
                $"Key={requirementData.TargetKey}, Selector={requirementData.RequiredSelector}",
                LogCategory.TargetsFiller
            );

            // ✅ Select with validation and retries
            var selectedTarget = await TrySelectValidTarget(
                operationData,
                requirementData,
                runtimeRequirement,
                requestSource,
                targets, // previously selected targets
                combinedTokenSource.Token
            );

            if (selectedTarget == null) {
                return TargetsFillResult.Failure(
                    $"Failed to select valid target for '{requirementData.TargetKey}'",
                    DateTime.UtcNow - startTime
                );
            }

            targets.Add(requirementData.TargetKey, selectedTarget);

            _logger.LogInfo(
                $"✓ Target selected: {requirementData.TargetKey} = {selectedTarget}",
                LogCategory.TargetsFiller
            );
        }

        var targetRegistry = new TargetRegistry(targets);
        var totalDuration = DateTime.UtcNow - startTime;

        _logger.LogInfo(
            $"✓ Target filling completed successfully in {totalDuration.TotalSeconds:F2}s",
            LogCategory.TargetsFiller
        );

        return TargetsFillResult.Success(targetRegistry, totalDuration);
    }

    // ✅ Ключовий метод - select з validation і retry
    private async UniTask<object> TrySelectValidTarget(
        OperationData operationData,
        ITargetRequirementData requirementData,
        ITargetRequirement runtimeRequirement,
        UnitModel requestSource,
        Dictionary<TargetKeys, object> previouslySelectedTargets,
        CancellationToken cancellationToken) {
        var selector = GetSelectorForTarget(requirementData, requestSource.OwnerId);
        var validationContext = CreateValidationContext(requestSource, previouslySelectedTargets);

        // Create request with runtime requirement
        var request = new TargetSelectionRequest(
            operationData,
            requirementData,
            runtimeRequirement,
            requestSource,
            validationContext
        );

        for (int attempt = 0; attempt < _maxRetryAttempts; attempt++) {
            if (cancellationToken.IsCancellationRequested) {
                _logger.LogInfo("Target selection cancelled by user", LogCategory.TargetsFiller);
                return null;
            }

            if (attempt > 0) {
                _logger.LogWarning(
                    $"Retry attempt {attempt + 1}/{_maxRetryAttempts} for target '{requirementData.TargetKey}'",
                    LogCategory.TargetsFiller
                );

                // Short delay before retry
                await UniTask.Delay(_retryDelay, cancellationToken: cancellationToken);
            }

            // 1. Let selector choose
            object selectedTarget = await SelectWithTimeout(
                selector,
                request,
                cancellationToken
            );

            if (selectedTarget == null) {
                _logger.LogWarning(
                    $"Selector returned null for '{requirementData.TargetKey}'",
                    LogCategory.TargetsFiller
                );

                if (attempt == _maxRetryAttempts - 1) {
                    return null; // Final attempt failed
                }
                continue; // Retry
            }

            // 2. ✅ Validate selected target
            var validationResult = runtimeRequirement.IsValid(selectedTarget, validationContext);

            if (validationResult.IsValid) {
                // ✅ Valid target selected!
                return selectedTarget;
            }

            // ❌ Invalid target selected
            _logger.LogWarning(
                $"Selected target is invalid: {validationResult.ErrorMessage}",
                LogCategory.TargetsFiller
            );

            // Last attempt?
            if (attempt == _maxRetryAttempts - 1) {
                _logger.LogError(
                    $"Failed to select valid target after {_maxRetryAttempts} attempts",
                    LogCategory.TargetsFiller
                );
                return null;
            }
        }

        return null;
    }

    private async UniTask<object> SelectWithTimeout(
        ITargetSelectionService selector,
        TargetSelectionRequest request,
        CancellationToken cancellationToken) {
        using var timeoutCts = new CancellationTokenSource(_selectorTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCts.Token
        );

        try {
            return await selector.SelectTargetAsync(request, linkedCts.Token);
        } catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested) {
            _logger.LogWarning(
                $"Target selection timed out after {_selectorTimeout.TotalSeconds}s",
                LogCategory.TargetsFiller
            );
            return null;
        } catch (Exception ex) {
            _logger.LogError(
                $"Error during target selection: {ex.Message}",
                LogCategory.TargetsFiller
            );
            return null;
        }
    }

    private ValidationContext CreateValidationContext(
        UnitModel requestSource,
        Dictionary<TargetKeys, object> previouslySelectedTargets) {
        return new ValidationContext(
            requestSource.OwnerId,
            extra: new {
                PreviousTargets = previouslySelectedTargets,
                RequestSource = requestSource
            }
        );
    }

    private ITargetSelectionService GetSelectorForTarget(
        ITargetRequirementData requirement,
        string initiatorId) {
        return requirement.RequiredSelector switch {
            TargetSelector.Initiator => _registeredSelectors.GetValueOrDefault(initiatorId, _fallbackSelector),
            TargetSelector.Opponent => GetOpponentSelector(initiatorId),
            TargetSelector.Auto => _fallbackSelector,
            _ => _fallbackSelector
        };
    }

    private ITargetSelectionService GetOpponentSelector(string initiatorId) {
        var opponent = _opponentRegistry.GetAgainstOpponentId(initiatorId);
        return opponent != null
            ? _registeredSelectors.GetValueOrDefault(opponent.InstanceId, _fallbackSelector)
            : _fallbackSelector;
    }

    public bool CanFillTargets(OperationData operationData, string ownerId) {
        var targets = operationData.targetRequirements;
        if (targets == null || !targets.Any()) {
            return false;
        }

        if (!_registeredSelectors.Any()) {
            return false;
        }

        return _targetValidator.CanValidateAllTargets(operationData, ownerId);
    }

    public void RegisterSelector(string playerId, ITargetSelectionService selectionService) {
        if (string.IsNullOrEmpty(playerId) || selectionService == null) {
            _logger.LogWarning("Invalid selector registration parameters", LogCategory.TargetsFiller);
            return;
        }

        _registeredSelectors[playerId] = selectionService;
        _logger.LogInfo($"Registered selector for player {playerId}", LogCategory.TargetsFiller);
    }

    public void UnregisterSelector(string playerId) {
        if (_registeredSelectors.Remove(playerId)) {
            _logger.LogInfo($"Unregistered selector for player {playerId}", LogCategory.TargetsFiller);
        }
    }

    public void Dispose() {
        _globalCancellationSource?.Cancel();
        _globalCancellationSource?.Dispose();
        _logger.LogInfo("OperationTargetsFiller disposed", LogCategory.TargetsFiller);
    }
}

public class TargetsFillResult {
    public OperationResult Status { get; private set; }
    public TargetRegistry TargetRegistry { get; private set; }
    public TimeSpan Duration { get; private set; }

    private TargetsFillResult(OperationResult status, TargetRegistry targetRegistry = null, TimeSpan duration = default) {
        Status = status;
        TargetRegistry = targetRegistry;
        Duration = duration;
    }

    public static TargetsFillResult Success(TargetRegistry targetRegistry, TimeSpan duration) =>
        new(OperationResult.Success(), targetRegistry, duration: duration);

    public static TargetsFillResult Failure(string error = null, TimeSpan duration = default) =>
        new(OperationResult.Failure(error), duration : duration);
    
}

public class TargetSelectionRequest {
    // Data layer
    public OperationData OperationData { get; }
    public ITargetRequirementData RequirementData { get; }

    // ✅ Runtime requirement для pre-filtering
    public ITargetRequirement RuntimeRequirement { get; }

    public UnitModel RequestSource { get; }
    public ValidationContext ValidationContext { get; }

    public TargetSelectionRequest(
        OperationData operationData,
        ITargetRequirementData requirementData,
        ITargetRequirement runtimeRequirement,
        UnitModel requestSource,
        ValidationContext validationContext) { // ✅ Отримуємо ззовні

        OperationData = operationData;
        RequirementData = requirementData;
        RuntimeRequirement = runtimeRequirement;
        RequestSource = requestSource;
        ValidationContext = validationContext; // ✅ Зберігаємо
    }
}

public enum TargetSelector {
    Initiator,  // Той, хто ініціював операцію
    Opponent,   // Опонент ініціатора
    AnyPlayer,  // Будь-який гравець (для рідкісних випадків)
    SpecificPlayer, // Для складних випадків (з можливістю вказати конкретного гравця)
    AllPlayers,
    NextPlayer,
    Auto
}
