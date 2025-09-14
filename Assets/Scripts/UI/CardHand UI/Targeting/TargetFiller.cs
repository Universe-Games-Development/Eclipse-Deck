using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class OperationTargetsFiller : MonoBehaviour {

    [Header("Configuration")]
    [SerializeField] private HumanTargetSelector fallbackSelector;
    [SerializeField] private int maxRetryAttempts = 3;
    [SerializeField] private float operationTimeoutSeconds = 30f;

    // Dependencies - будуть ін'єктовані
    private ITargetSelectorFactory selectorFactory;
    private ITargetValidator targetValidator;
    private BoardGame boardGame;

    // State management
    private CancellationTokenSource globalCancellationSource = new();
    private TargetOperationContext currentOperation;

    private void Start() {
        InitializeDependencies();
    }

    private void InitializeDependencies() {
        // TODO: Замінити на правильну DI ін'єкцію
        selectorFactory = new TargetSelectorFactory(boardGame, fallbackSelector);
        targetValidator = new TargetValidator();
    }

    private void OnDestroy() {
        globalCancellationSource?.Cancel();
        globalCancellationSource?.Dispose();
        currentOperation?.Dispose();
    }

    public bool CanFillTargets(List<Target> targets) {
        if (targets?.Any() != true) return false;

        try {
            return selectorFactory.CanCreateSelectors(targets) &&
                   targetValidator.CanValidateAllTargets(targets);
        } catch (Exception ex) {
            GameLogger.LogError($"Error checking target filling capability: {ex.Message}", LogCategory.TargetsFiller);
            return false;
        }
    }

    public async UniTask<TargetOperationResult> FillTargetsAsync(TargetOperationRequest request, CancellationToken cancellationToken = default) {

        if (!ValidateRequest(request)) {
            return TargetOperationResult.Failure("Invalid request parameters");
        }

        using var operation = new TargetOperationContext(request, maxRetryAttempts);
        currentOperation = operation;

        try {
            using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(operationTimeoutSeconds));
            using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, globalCancellationSource.Token, timeoutSource.Token);

            GameLogger.LogInfo($"Starting target operation: {request.Targets.Count} targets, mandatory: {request.IsMandatory}",
                LogCategory.TargetsFiller);

            var result = await ProcessTargetSelection(operation, combinedTokenSource.Token);

            GameLogger.LogInfo($"Target operation completed: {result.Status}", LogCategory.TargetsFiller);
            return result;

        } catch (OperationCanceledException) {
            GameLogger.LogInfo("Target operation cancelled", LogCategory.TargetsFiller);
            return TargetOperationResult.Cancelled();
        } catch (Exception ex) {
            GameLogger.LogError($"Target operation failed: {ex.Message}", LogCategory.TargetsFiller);
            return TargetOperationResult.Failure(ex.Message);
        } finally {
            currentOperation = null;
        }
    }

    private async UniTask<TargetOperationResult> ProcessTargetSelection(TargetOperationContext operation, CancellationToken cancellationToken) {
        var processor = new TargetSelectionProcessor(selectorFactory, targetValidator);

        while (operation.HasNextTarget && !cancellationToken.IsCancellationRequested) {
            var target = operation.GetCurrentTarget();

            try {
                var selectionResult = await processor.ProcessTarget(target, operation.Request, cancellationToken);

                var actionResult = operation.ProcessSelectionResult(selectionResult);
                if (actionResult == TargetActionResult.BreakLoop) {
                    break;
                }

            } catch (OperationCanceledException) {
                throw;
            } catch (Exception ex) {
                GameLogger.LogError($"Error processing target {target.Name}: {ex.Message}", LogCategory.TargetsFiller);
                GameLogger.LogException(ex);

                if (operation.ShouldAbortOnError()) {
                    break;
                }
            }
        }

        return operation.GetResult();
    }

    private bool ValidateRequest(TargetOperationRequest request) {
        if (request?.Targets?.Any() != true) {
            GameLogger.LogWarning("Cannot process empty target request", LogCategory.TargetsFiller);
            return false;
        }

        if (request.Source == null && request.RequiresSource()) {
            GameLogger.LogWarning("Request requires initiator but none provided", LogCategory.TargetsFiller);
            return false;
        }

        return true;
    }
}

// === Data Transfer Objects ===

public class TargetOperationRequest {

    public TargetOperationRequest(List<Target> namedTargets, bool isMandatory, UnitModel source) {
        Targets = namedTargets;
        IsMandatory = isMandatory;
        Source = source;
    }

    public List<Target> Targets { get; set; }
    public bool IsMandatory { get; set; }
    public UnitModel Source { get; set; }
    public string OperationId { get; set; } = Guid.NewGuid().ToString();

    public bool RequiresSource() =>
        Targets?.Any(t => t.Requirement?.GetTargetSelector() != TargetSelector.AnyPlayer) == true;
}

public class TargetOperationResult {
    public OperationStatus Status { get; private set; }
    public Dictionary<string, UnitModel> FilledTargets { get; private set; }
    public string ErrorMessage { get; private set; }
    public TimeSpan Duration { get; private set; }

    private TargetOperationResult(OperationStatus status, Dictionary<string, UnitModel> targets = null, string error = null) {
        Status = status;
        FilledTargets = targets ?? new Dictionary<string, UnitModel>();
        ErrorMessage = error;
    }

    public static TargetOperationResult Success(Dictionary<string, UnitModel> targets, TimeSpan duration) {
        TargetOperationResult result = new(OperationStatus.Success, targets);
        result.Duration = duration;
        return result;
    }

    public static TargetOperationResult Failure(string error) =>
        new(OperationStatus.Failed, error: error);

    public static TargetOperationResult Cancelled() =>
        new(OperationStatus.Cancelled);

    public static TargetOperationResult PartialSuccess(Dictionary<string, UnitModel> targets, TimeSpan duration) {
        var result = new TargetOperationResult(OperationStatus.PartialSuccess, targets);
        result.Duration = duration;
        return result;
    }
}



// === Core Logic Classes ===

public class TargetOperationContext : IDisposable {
    private readonly DateTime startTime;
    private readonly List<TargetState> targetStates;
    private int currentIndex;

    public TargetOperationRequest Request { get; }
    public int MaxRetries { get; }
    public bool HasNextTarget => currentIndex < targetStates.Count;

    public TargetOperationContext(TargetOperationRequest request, int maxRetries) {
        Request = request;
        MaxRetries = maxRetries;
        startTime = DateTime.UtcNow;
        currentIndex = 0;

        targetStates = request.Targets.Select(t => new TargetState(t)).ToList();
    }

    public TargetState GetCurrentTarget() {
        return HasNextTarget ? targetStates[currentIndex] : null;
    }

    public TargetActionResult ProcessSelectionResult(TargetSelectionResult result) {
        var currentTarget = GetCurrentTarget();

        switch (result.Action) {
            case SelectionAction.Success:
                currentTarget.SetUnit(result.SelectedUnit);
                currentTarget.ResetRetries();
                MoveToNextEmptyTarget();
                return TargetActionResult.Continue;

            case SelectionAction.Retry:
                currentTarget.IncrementRetries();
                if (currentTarget.RetryCount >= MaxRetries) {
                    if (Request.IsMandatory) {
                        return TargetActionResult.Continue; // Продовжуємо для обов'язкових
                    }
                    GameLogger.LogWarning($"Max retries reached for target: {currentTarget.Name}", LogCategory.TargetsFiller);
                    return TargetActionResult.BreakLoop;
                }
                return TargetActionResult.Continue;

            case SelectionAction.Cancel:
                return TargetActionResult.BreakLoop;

            case SelectionAction.ClearDuplicate:
                HandleDuplicateUnit(result.SelectedUnit, currentTarget);
                return TargetActionResult.Continue;

            default:
                return TargetActionResult.Continue;
        }
    }

    private void HandleDuplicateUnit(UnitModel unit, TargetState currentTarget) {
        var duplicateTarget = targetStates.FirstOrDefault(t => t.Unit == unit);
        if (duplicateTarget != null) {
            duplicateTarget.ClearUnit();

            // Якщо дублікат був раніше - повертаємося до нього
            var duplicateIndex = targetStates.IndexOf(duplicateTarget);
            if (duplicateIndex < currentIndex) {
                currentIndex = duplicateIndex;
            }
        }

        currentTarget.SetUnit(unit);
    }

    private void MoveToNextEmptyTarget() {
        for (int i = currentIndex + 1; i < targetStates.Count; i++) {
            if (targetStates[i].Unit == null) {
                currentIndex = i;
                return;
            }
        }
        currentIndex = targetStates.Count; // All filled
    }

    public bool ShouldAbortOnError() {
        var currentTarget = GetCurrentTarget();
        return currentTarget?.RetryCount >= MaxRetries && !Request.IsMandatory;
    }

    public TargetOperationResult GetResult() {
        var filledTargets = targetStates
            .Where(t => t.Unit != null)
            .ToDictionary(t => t.Name, t => t.Unit);

        var duration = DateTime.UtcNow - startTime;

        if (filledTargets.Count == 0 && Request.IsMandatory) {
            return TargetOperationResult.Failure("No targets filled for mandatory operation");
        }

        if (filledTargets.Count == Request.Targets.Count) {
            return TargetOperationResult.Success(filledTargets, duration);
        }

        return filledTargets.Count > 0
            ? TargetOperationResult.PartialSuccess(filledTargets, duration)
            : TargetOperationResult.Failure("No targets were filled");
    }

    public void Dispose() {
    }
}

public class TargetState {
    public string Name { get; }
    public ITargetRequirement Requirement { get; }
    public UnitModel Unit { get; private set; }
    public int RetryCount { get; private set; }

    public TargetState(Target target) {
        Name = target.Key;
        Requirement = target.Requirement;
    }

    public void SetUnit(UnitModel unit) => Unit = unit;
    public void ClearUnit() => Unit = null;
    public void IncrementRetries() => RetryCount++;
    public void ResetRetries() => RetryCount = 0;
}

public enum TargetActionResult {
    Continue,
    BreakLoop
}

// === Selection Processing ===

public class TargetSelectionProcessor {
    private readonly ITargetSelectorFactory selectorFactory;
    private readonly ITargetValidator validator;

    public TargetSelectionProcessor(ITargetSelectorFactory selectorFactory, ITargetValidator validator) {
        this.selectorFactory = selectorFactory;
        this.validator = validator;
    }

    public async UniTask<TargetSelectionResult> ProcessTarget(
        TargetState target,
        TargetOperationRequest request,
        CancellationToken cancellationToken) {

        var selector = selectorFactory.CreateSelector(target.Requirement, request.Source.GetPlayer());

        try {
            var selectedUnit = await selector.SelectTargetAsync(
                new TargetSelectionRequest(request.Source, target.Requirement),
                cancellationToken);

            return ProcessSelection(selectedUnit, target, request);

        } catch (OperationCanceledException) {
            throw;
        } catch (Exception ex) {
            GameLogger.LogException(ex);
            GameLogger.LogError($"Selector error for {target.Name}: {ex.Message}", LogCategory.TargetsFiller);
            return TargetSelectionResult.Retry();
        }
    }

    private TargetSelectionResult ProcessSelection(UnitModel unit, TargetState target, TargetOperationRequest request) {
        if (unit == null) {
            return HandleNullSelection(request);
        }

        // Валідація
        var validationResult = validator.ValidateTarget(unit, target.Requirement, request.Source.GetPlayer());
        if (!validationResult.IsValid) {
            GameLogger.LogWarning($"Invalid target {unit} for {target.Name}: {validationResult.ErrorMessage}",
                LogCategory.TargetsFiller);
            return TargetSelectionResult.Retry();
        }

        return TargetSelectionResult.Success(unit);
    }

    private TargetSelectionResult HandleNullSelection(TargetOperationRequest request) {
        if (request.IsMandatory) {
            return TargetSelectionResult.Retry();
        }

        return request.Targets.Count == 1
            ? TargetSelectionResult.Cancel()
            : TargetSelectionResult.Success(null); // Skip for multi-target optional operations
    }
}

// === Selection Results ===

public class TargetSelectionResult {
    public SelectionAction Action { get; private set; }
    public UnitModel SelectedUnit { get; private set; }

    private TargetSelectionResult(SelectionAction action, UnitModel unit = null) {
        Action = action;
        SelectedUnit = unit;
    }

    public static TargetSelectionResult Success(UnitModel unit) =>
        new(SelectionAction.Success, unit);

    public static TargetSelectionResult Retry() =>
        new(SelectionAction.Retry);

    public static TargetSelectionResult Cancel() =>
        new(SelectionAction.Cancel);

    public static TargetSelectionResult ClearDuplicate(UnitModel unit) =>
        new(SelectionAction.ClearDuplicate, unit);
}

public enum SelectionAction {
    Success,
    Retry,
    Cancel,
    ClearDuplicate
}

// === Factory and Services ===

public interface ITargetSelectorFactory {
    ITargetSelector CreateSelector(ITargetRequirement requirement, BoardPlayer initiator);
    bool CanCreateSelectors(List<Target> targets);
}

public class TargetSelectorFactory : ITargetSelectorFactory {
    private readonly BoardGame boardGame;
    private readonly ITargetSelector fallbackSelector;

    public TargetSelectorFactory(BoardGame boardGame, ITargetSelector fallbackSelector) {
        this.boardGame = boardGame;
        this.fallbackSelector = fallbackSelector;
    }

    public ITargetSelector CreateSelector(ITargetRequirement requirement, BoardPlayer initiator) {
        var selectorType = requirement.GetTargetSelector();

        return selectorType switch {
            TargetSelector.Initiator => initiator?.Selector ?? fallbackSelector,
            TargetSelector.Opponent => boardGame?.GetOpponent(initiator)?.Selector ?? fallbackSelector,
            TargetSelector.AnyPlayer => initiator?.Selector ?? fallbackSelector,
            _ => fallbackSelector
        };
    }

    public bool CanCreateSelectors(List<Target> targets) {
        return targets?.All(t => t.Requirement?.GetTargetSelector() != TargetSelector.AllPlayers) == true;
    }
}

public interface ITargetValidator {
    ValidationResult ValidateTarget(UnitModel unit, ITargetRequirement requirement, BoardPlayer initiator);
    bool CanValidateAllTargets(List<Target> targets);
}

public class TargetValidator : ITargetValidator {
    public ValidationResult ValidateTarget(UnitModel unit, ITargetRequirement requirement, BoardPlayer initiator) {
        try {
            return requirement.IsValid(unit, initiator);
        } catch (Exception ex) {
            GameLogger.LogError($"Validation error: {ex.Message}", LogCategory.TargetsFiller);
            return ValidationResult.Error($"Validation failed: {ex.Message}");
        }
    }

    public bool CanValidateAllTargets(List<Target> targets) {
        return targets?.All(t => t.Requirement != null) == true;
    }
}


// === Existing interfaces (keeping for compatibility) ===
public enum TargetSelector {
    Initiator,  // Той, хто ініціював операцію
    Opponent,   // Опонент ініціатора
    AnyPlayer,  // Будь-який гравець (для рідкісних випадків)
    SpecificPlayer, // Для складних випадків (з можливістю вказати конкретного гравця)
    AllPlayers,
    NextPlayer
}

public interface ITargetSelector {
    UniTask<UnitModel> SelectTargetAsync(TargetSelectionRequest selectionRequst, CancellationToken cancellationToken);
}

