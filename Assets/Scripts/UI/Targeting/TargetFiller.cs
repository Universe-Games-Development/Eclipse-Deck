using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public interface ITargetFiller {
    bool CanFillTargets(List<TargetInfo> targets, string ownerId);
    UniTask<TargetOperationResult> FillTargetsAsync(TargetOperationRequest request, CancellationToken cancellationToken = default);
    void RegisterSelector(string playerId, ITargetSelectionService selectionService);
    UniTask<TargetFillResult> TryFillTargetAsync(TargetInfo target, UnitModel requestSource, bool isMandatory, CancellationToken cancellationToken = default);
    void UnregisterSelector(string playerId);
}

public class OperationTargetsFiller : ITargetFiller {
    private readonly ITargetValidator _targetValidator;
    private readonly ILogger _logger;
    private readonly IOpponentRegistry _opponentRegistry;

    private readonly Dictionary<string, ITargetSelectionService> _registeredSelectors = new();
    private readonly CancellationTokenSource _globalCancellationSource = new();
    private readonly ITargetSelectionService _fallbackSelector;

    // Налаштування
    private readonly int _maxRetryAttempts;
    private readonly TimeSpan _selectorTimeout;

    public OperationTargetsFiller( ITargetValidator targetValidator, ILogger logger,
        IOpponentRegistry opponentRegistry, ITargetSelectionService fallbackSelector = null,
        int maxRetryAttempts = 1, float selectorTimeoutSeconds = 10f) {

        _targetValidator = targetValidator ?? throw new ArgumentNullException(nameof(targetValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _opponentRegistry = opponentRegistry ?? throw new ArgumentNullException(nameof(opponentRegistry));

        _fallbackSelector = fallbackSelector ?? new RandomTargetSelector();
        _maxRetryAttempts = maxRetryAttempts;
        _selectorTimeout = TimeSpan.FromSeconds(selectorTimeoutSeconds);
    }


    public bool CanFillTargets(List<TargetInfo> targets, string ownerId) {
        if (targets?.Any() != true) {
            return false;
        }

        // Є хоча б один selector (не рахуючи fallback)
        if (!_registeredSelectors.Any()) {
            return false;
        }

        // Перевіряємо через validator чи існують валідні цілі
        // Тут можна передати ownerId якщо він відомий, або null для загальної перевірки
        return _targetValidator.CanValidateAllTargets(targets, ownerId);
    }

    public async UniTask<TargetOperationResult> FillTargetsAsync(TargetOperationRequest request, CancellationToken cancellationToken = default) {

        if (!IsValidRequest(request)) {
            return TargetOperationResult.Failure("Invalid request parameters");
        }

        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _globalCancellationSource.Token);

        _logger.LogInfo($"Starting target operation: {request.Targets.Count} targets, mandatory: {request.IsMandatory}",
            LogCategory.TargetsFiller);

        try {
            var result = await ProcessTargetsAsync(request, combinedTokenSource.Token);
            _logger.LogInfo($"Target operation completed: {result.Status.IsSuccess}", LogCategory.TargetsFiller);
            return result;
        } catch (OperationCanceledException) {
            _logger.LogInfo("Target operation cancelled", LogCategory.TargetsFiller);
            return TargetOperationResult.Failure("Operation was cancelled");
        } catch (Exception ex) {
            _logger.LogError($"Target operation failed: {ex.Message}", LogCategory.TargetsFiller);
            return TargetOperationResult.Failure(ex.Message);
        }
    }

    private async UniTask<TargetOperationResult> ProcessTargetsAsync(TargetOperationRequest request, CancellationToken cancellationToken) {

        var startTime = DateTime.UtcNow;
        var filledTargets = new Dictionary<string, object>();

        for (int i = 0; i < request.Targets.Count; i++) {
            var target = request.Targets[i];
            

            var result = await TryFillTargetAsync(target, request.Source, request.IsMandatory, cancellationToken);

            if (result.IsSuccess && result.Unit != null) {
                filledTargets[target.Key] = result.Unit;
            } else if (request.IsMandatory && result.Unit == null) {
                var duration = DateTime.UtcNow - startTime;
                return TargetOperationResult.Failure(
                    $"Failed to fill mandatory target: {target.Key}",
                    duration);
            }
        }

        var totalDuration = DateTime.UtcNow - startTime;

        if (filledTargets.Count == request.Targets.Count) {
            return TargetOperationResult.Success(filledTargets, totalDuration);
        } else if (filledTargets.Count > 0 || !request.IsMandatory) {
            // Часткове заповнення - ок для необов'язкових
            return TargetOperationResult.Success(filledTargets, totalDuration);
        } else {
            return TargetOperationResult.Failure("No targets were filled", totalDuration);
        }
    }

    public async UniTask<TargetFillResult> TryFillTargetAsync(TargetInfo target, UnitModel requestSource, bool isMandatory, CancellationToken cancellationToken = default) {

        var selector = GetSelectorForTarget(target, requestSource.OwnerId);

        for (int attempt = 0; attempt < _maxRetryAttempts; attempt++) {
            try {

                using var timeoutSource = new CancellationTokenSource(_selectorTimeout);
                using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken, timeoutSource.Token);

                TargetSelectionRequest targetSelectionRequest = new TargetSelectionRequest(target, requestSource);
                var selectedUnit = await selector.SelectTargetAsync(targetSelectionRequest, linkedSource.Token);

                if (selectedUnit == null) {
                    // Для необов'язкових - це ок
                    return TargetFillResult.Failed();
                }

                var validationResult = target.IsValid(
                    selectedUnit,
                    new ValidationContext(requestSource.OwnerId));

                if (validationResult.IsValid) {
                    return TargetFillResult.Success(selectedUnit);
                }

                _logger.LogWarning(
                    $"Invalid target {selectedUnit} for {target.Key} (attempt {attempt + 1}/{_maxRetryAttempts}): {validationResult.ErrorMessage}",
                    LogCategory.TargetsFiller);


            } catch (OperationCanceledException) {
                return TargetFillResult.Failed();
                throw;
            } catch (TimeoutException) {
                selector.CancelCurrentSelection();
                _logger.LogWarning(
                    $"Selector timeout for {target.Key} (attempt {attempt + 1}/{_maxRetryAttempts})",
                    LogCategory.TargetsFiller);

                // Якщо це обов'язковий і selector не відповідає - спробуємо fallback
                if (isMandatory && selector != _fallbackSelector) {
                    _logger.LogInfo($"Switching to fallback selector for mandatory target {target.Key}",
                        LogCategory.TargetsFiller);
                    selector = _fallbackSelector;
                    continue;
                }

                return TargetFillResult.Failed();
            } catch (Exception ex) {
                _logger.LogError(
                    $"Error selecting target {target.Key} (attempt {attempt + 1}/{_maxRetryAttempts}): {ex.Message}",
                    LogCategory.TargetsFiller);

                return TargetFillResult.Failed();
            }
        }

        return TargetFillResult.Failed();
    }

    private bool IsValidRequest(TargetOperationRequest request) {
        if (request?.Targets?.Any() != true) {
            _logger.LogWarning("Cannot process empty target request", LogCategory.TargetsFiller);
            return false;
        }

        if (request.Source == null) {
            _logger.LogWarning("Request requires source but none provided", LogCategory.TargetsFiller);
            return false;
        }

        return true;
    }

    private ITargetSelectionService GetSelectorForTarget(TargetInfo typeTarget, string initiatorId) {
        var selectorType = typeTarget.GetTargetSelector();

        return selectorType switch {
            TargetSelector.Initiator => _registeredSelectors.GetValueOrDefault(initiatorId, _fallbackSelector),
            TargetSelector.Opponent => GetOpponentSelector(initiatorId),
            _ => _fallbackSelector
        };
    }

    private ITargetSelectionService GetOpponentSelector(string initiatorId) {
        var opponent = _opponentRegistry.GetAgainstOpponentId(initiatorId);
        return opponent != null
            ? _registeredSelectors.GetValueOrDefault(opponent.InstanceId, _fallbackSelector)
            : _fallbackSelector;
    }

    public void RegisterSelector(string playerId, ITargetSelectionService selectionService) {
        if (string.IsNullOrEmpty(playerId)) {
            _logger.LogWarning("Cannot register selector with null or empty player ID", LogCategory.TargetsFiller);
            return;
        }

        if (selectionService == null) {
            _logger.LogWarning($"Cannot register null selector for player {playerId}", LogCategory.TargetsFiller);
            return;
        }

        if (_registeredSelectors.ContainsKey(playerId)) {
            _logger.LogWarning($"Selector already registered for player {playerId}, replacing", LogCategory.TargetsFiller);
        }

        _registeredSelectors[playerId] = selectionService;
        _logger.LogInfo($"Registered selector for player {playerId}", LogCategory.TargetsFiller);
    }

    public void UnregisterSelector(string playerId) {
        if (_registeredSelectors.Remove(playerId)) {
            _logger.LogInfo($"Unregistered selector for player {playerId}", LogCategory.TargetsFiller);
        } else {
            _logger.LogWarning($"No selector found to unregister for player {playerId}", LogCategory.TargetsFiller);
        }
    }

    public void Dispose() {
        _globalCancellationSource?.Cancel();
        _globalCancellationSource?.Dispose();
        _logger.LogInfo("OperationTargetsFiller disposed", LogCategory.TargetsFiller);
    }
}

// Допоміжна структура для результату заповнення одного таргету
public readonly struct TargetFillResult {
    public bool IsSuccess { get; }
    public object Unit { get; }

    private TargetFillResult(bool success, object unit) {
        IsSuccess = success;
        Unit = unit;
    }

    public static TargetFillResult Success(object unit) => new(true, unit);
    public static TargetFillResult Failed() => new(false, null);
}

// === Simplified Data Objects ===

public class TargetOperationRequest {
    public TargetOperationRequest(List<TargetInfo> namedTargets, bool isMandatory, UnitModel source) {
        Targets = namedTargets ?? throw new ArgumentNullException(nameof(namedTargets));
        IsMandatory = isMandatory;
        Source = source ?? throw new ArgumentNullException(nameof(source));
        OperationId = Guid.NewGuid().ToString();
    }

    public List<TargetInfo> Targets { get; }
    public bool IsMandatory { get; }
    public UnitModel Source { get; }
    public string OperationId { get; }
}

public class TargetOperationResult {
    public OperationResult Status { get; private set; }
    public IReadOnlyDictionary<string, object> FilledTargets { get; private set; }
    public TimeSpan Duration { get; private set; }

    private TargetOperationResult(OperationResult status, Dictionary<string, object> targets = null, TimeSpan duration = default) {
        Status = status;
        FilledTargets = targets ?? new Dictionary<string, object>();
        Duration = duration;
    }

    public static TargetOperationResult Success(Dictionary<string, object> targets, TimeSpan duration) =>
        new(OperationResult.Success(), targets, duration: duration);

    public static TargetOperationResult Failure(string error = null, TimeSpan duration = default) {
        return new TargetOperationResult(OperationResult.Failure(error), duration : duration);
    }

}

public enum TargetSelector {
    Initiator,  // Той, хто ініціював операцію
    Opponent,   // Опонент ініціатора
    AnyPlayer,  // Будь-який гравець (для рідкісних випадків)
    SpecificPlayer, // Для складних випадків (з можливістю вказати конкретного гравця)
    AllPlayers,
    NextPlayer
}

