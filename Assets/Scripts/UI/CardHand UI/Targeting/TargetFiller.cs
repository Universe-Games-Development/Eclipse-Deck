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

    public bool CanFillTargets(List<NamedTarget> targets) {
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

        if (request.Initiator == null && request.RequiresInitiator()) {
            GameLogger.LogWarning("Request requires initiator but none provided", LogCategory.TargetsFiller);
            return false;
        }

        return true;
    }
}

// === Data Transfer Objects ===

public class TargetOperationRequest {

    public TargetOperationRequest(List<NamedTarget> namedTargets, bool isMandatory, BoardUnit initiator) {
        Targets = namedTargets;
        IsMandatory = isMandatory;
        Initiator = initiator;
    }

    public List<NamedTarget> Targets { get; set; }
    public bool IsMandatory { get; set; }
    public BoardUnit Initiator { get; set; }
    public string OperationId { get; set; } = Guid.NewGuid().ToString();

    public bool RequiresInitiator() =>
        Targets?.Any(t => t.Requirement?.GetTargetSelector() != TargetSelector.AnyPlayer) == true;
}

public class TargetOperationResult {
    public OperationStatus Status { get; private set; }
    public Dictionary<string, UnitInfo> FilledTargets { get; private set; }
    public string ErrorMessage { get; private set; }
    public TimeSpan Duration { get; private set; }

    private TargetOperationResult(OperationStatus status, Dictionary<string, UnitInfo> targets = null, string error = null) {
        Status = status;
        FilledTargets = targets ?? new Dictionary<string, UnitInfo>();
        ErrorMessage = error;
    }

    public static TargetOperationResult Success(Dictionary<string, UnitInfo> targets, TimeSpan duration) {
        var result = new TargetOperationResult(OperationStatus.Success, targets);
        result.Duration = duration;
        return result;
    }

    public static TargetOperationResult Failure(string error) =>
        new TargetOperationResult(OperationStatus.Failed, error: error);

    public static TargetOperationResult Cancelled() =>
        new TargetOperationResult(OperationStatus.Cancelled);

    public static TargetOperationResult PartialSuccess(Dictionary<string, UnitInfo> targets, TimeSpan duration) {
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

    private void HandleDuplicateUnit(UnitInfo unit, TargetState currentTarget) {
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
    public UnitInfo Unit { get; private set; }
    public int RetryCount { get; private set; }

    public TargetState(NamedTarget target) {
        Name = target.Name;
        Requirement = target.Requirement;
    }

    public void SetUnit(UnitInfo unit) => Unit = unit;
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

        var selector = selectorFactory.CreateSelector(target.Requirement, request.Initiator.GetPlayer());

        try {
            var selectedUnit = await selector.SelectTargetAsync(
                new TargetSelectionRequest(request.Initiator, target.Requirement),
                cancellationToken);

            return ProcessSelection(selectedUnit, target, request);

        } catch (OperationCanceledException) {
            throw;
        } catch (Exception ex) {
            GameLogger.LogError($"Selector error for {target.Name}: {ex.Message}", LogCategory.TargetsFiller);
            return TargetSelectionResult.Retry();
        }
    }

    private TargetSelectionResult ProcessSelection(UnitInfo unit, TargetState target, TargetOperationRequest request) {
        if (unit == null) {
            return HandleNullSelection(request);
        }

        // Валідація
        var validationResult = validator.ValidateTarget(unit, target.Requirement, request.Initiator.GetPlayer());
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
    public UnitInfo SelectedUnit { get; private set; }

    private TargetSelectionResult(SelectionAction action, UnitInfo unit = null) {
        Action = action;
        SelectedUnit = unit;
    }

    public static TargetSelectionResult Success(UnitInfo unit) =>
        new TargetSelectionResult(SelectionAction.Success, unit);

    public static TargetSelectionResult Retry() =>
        new TargetSelectionResult(SelectionAction.Retry);

    public static TargetSelectionResult Cancel() =>
        new TargetSelectionResult(SelectionAction.Cancel);

    public static TargetSelectionResult ClearDuplicate(UnitInfo unit) =>
        new TargetSelectionResult(SelectionAction.ClearDuplicate, unit);
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
    bool CanCreateSelectors(List<NamedTarget> targets);
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

    public bool CanCreateSelectors(List<NamedTarget> targets) {
        return targets?.All(t => t.Requirement?.GetTargetSelector() != TargetSelector.AllPlayers) == true;
    }
}

public interface ITargetValidator {
    ValidationResult ValidateTarget(UnitInfo unit, ITargetRequirement requirement, BoardPlayer initiator);
    bool CanValidateAllTargets(List<NamedTarget> targets);
}

public class TargetValidator : ITargetValidator {
    public ValidationResult ValidateTarget(UnitInfo unit, ITargetRequirement requirement, BoardPlayer initiator) {
        try {
            return requirement.IsValid(unit, initiator);
        } catch (Exception ex) {
            GameLogger.LogError($"Validation error: {ex.Message}", LogCategory.TargetsFiller);
            return ValidationResult.InValid($"Validation failed: {ex.Message}");
        }
    }

    public bool CanValidateAllTargets(List<NamedTarget> targets) {
        return targets?.All(t => t.Requirement != null) == true;
    }
}

// === Extensions ===

public static class TargetRequirementExtensions {
    public static TargetSelector GetTargetSelector(this ITargetRequirement requirement) {
        // Implementation depends on your ITargetRequirement interface
        // This is a placeholder
        return TargetSelector.Initiator;
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
    UniTask<UnitInfo> SelectTargetAsync(TargetSelectionRequest selectionRequst, CancellationToken cancellationToken);
}

public abstract class BoardUnit : MonoBehaviour {
    public abstract UnitInfo GetInfo();
    public abstract BoardPlayer GetPlayer();
}

