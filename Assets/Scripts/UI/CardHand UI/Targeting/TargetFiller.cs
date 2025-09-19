using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

public class OperationTargetsFiller : MonoBehaviour {

    [Header("Configuration")]
    [SerializeField] private HumanTargetSelector fallbackSelector;
    [SerializeField] private int maxRetryAttempts = 3;
    [SerializeField] private float operationTimeoutSeconds = 30f;

    // Dependencies - будуть ін'єктовані
    
    private ITargetValidator targetValidator;
    
    // State management
    private CancellationTokenSource globalCancellationSource = new();
    private TargetOperationContext currentOperation;

    [Inject] private BoardGame boardGame;
    [Inject] private IUnitPresenterRegistry _unitPresenterRegistry;

    private void Start() {
        InitializeDependencies();
    }

    private void InitializeDependencies() {
        // TODO: Замінити на правильну DI ін'єкцію
        targetValidator = new TargetValidator();
    }

    private void OnDestroy() {
        globalCancellationSource?.Cancel();
        globalCancellationSource?.Dispose();
        currentOperation?.Dispose();
    }

    public bool CanFillTargets(List<TypedTargetBase> targets) {
        if (targets?.Any() != true) return false;

        return targetValidator.CanValidateAllTargets(targets);
    }

    public async UniTask<TargetOperationResult> FillTargetsAsync(TargetOperationRequest request, CancellationToken cancellationToken = default) {

        if (!ValidateRequest(request)) {
            return TargetOperationResult.Failure("Invalid request parameters");
        }

        using var operationContext = new TargetOperationContext(request, maxRetryAttempts);
        currentOperation = operationContext;

        try {
            using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(operationTimeoutSeconds));
            using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, globalCancellationSource.Token, timeoutSource.Token);

            GameLogger.LogInfo($"Starting target operation: {request.Targets.Count} targets, mandatory: {request.IsMandatory}",
                LogCategory.TargetsFiller);

            var result = await ProcessTargetSelection(operationContext, combinedTokenSource.Token);

            GameLogger.LogInfo($"Target operation completed: {result.Status}", LogCategory.TargetsFiller);
            return result;

        } catch (OperationCanceledException) {
            GameLogger.LogInfo("Target operation cancelled", LogCategory.TargetsFiller);
            return TargetOperationResult.Cancelled();
        }  finally {
            currentOperation = null;
        }
    }

    private async UniTask<TargetOperationResult> ProcessTargetSelection(TargetOperationContext operationContext, CancellationToken cancellationToken) {
        var processor = new TargetSelectionProcessor(targetValidator);

        while (operationContext.HasNextTarget && !cancellationToken.IsCancellationRequested) {
            var targetState = operationContext.GetCurrentTarget();



            ITargetSelector targetSelector = CreateSelector(targetState.Target, operationContext.Request.Source.GetPlayer());

            try {
                var selectionResult = await processor.ProcessTarget(targetSelector, targetState, operationContext.Request, cancellationToken);

                var actionResult = operationContext.ProcessSelectionResult(selectionResult);
                if (actionResult == TargetActionResult.BreakLoop) {
                    break;
                }

            } catch (OperationCanceledException) {
                throw;
            }
        }

        return operationContext.GetResult();
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

    public ITargetSelector CreateSelector(TypedTargetBase typeTarget, Opponent initiator) {
        var selectorType = typeTarget.GetTargetSelector();

        Opponent target;

        switch (selectorType) {
            case TargetSelector.Initiator:
                target = initiator;
                break;

            case TargetSelector.Opponent:
                target = boardGame?.GetOpponent(initiator);
                break;

            case TargetSelector.AnyPlayer:
            default:
                return fallbackSelector;
        }

        var playerPresenter = _unitPresenterRegistry.GetPresenter<BoardPlayerPresenter>(target);
        return playerPresenter?.Selector ?? fallbackSelector;
    }
}

// === Data Transfer Objects ===

public class TargetOperationRequest {

    public TargetOperationRequest(List<TypedTargetBase> namedTargets, bool isMandatory, UnitModel source) {
        Targets = namedTargets;
        IsMandatory = isMandatory;
        Source = source;
    }

    public List<TypedTargetBase> Targets { get; set; }
    public bool IsMandatory { get; set; }
    public UnitModel Source { get; set; }
    public string OperationId { get; set; } = Guid.NewGuid().ToString();

    public bool RequiresSource() {
        return Targets.Any(t => t.RequiresSource());
    }
}

public class TargetOperationResult {
    public OperationStatus Status { get; private set; }
    public Dictionary<string, object> FilledTargets { get; private set; }
    public string ErrorMessage { get; private set; }
    public TimeSpan Duration { get; private set; }

    private TargetOperationResult(OperationStatus status, Dictionary<string, object> targets = null, string error = null) {
        Status = status;
        FilledTargets = targets ?? new Dictionary<string, object>();
        ErrorMessage = error;
    }

    public static TargetOperationResult Success(Dictionary<string, object> targets, TimeSpan duration) {
        TargetOperationResult result = new(OperationStatus.Success, targets);
        result.Duration = duration;
        return result;
    }

    public static TargetOperationResult Failure(string error) =>
        new(OperationStatus.Failed, error: error);

    public static TargetOperationResult Cancelled() =>
        new(OperationStatus.Cancelled);

    public static TargetOperationResult PartialSuccess(Dictionary<string, object> targets, TimeSpan duration) {
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

    private void HandleDuplicateUnit(object unit, TargetState currentTarget) {
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
    public TypedTargetBase Target { get; }

    public string Name { get; }
    public object Unit { get; private set; }
    public int RetryCount { get; private set; }

    public TargetState(TypedTargetBase target) {
        Name = target.Key;
        Target = target;
    }

    public void SetUnit(object unit) => Unit = unit;
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
    private readonly ITargetValidator validator;

    public TargetSelectionProcessor(ITargetValidator validator) {
        this.validator = validator;
    }

    public async UniTask<TargetSelectionResult> ProcessTarget(
        ITargetSelector targetSelector,
        TargetState targetState,
        TargetOperationRequest request,
        CancellationToken cancellationToken) {

        try {
            var selectedUnit = await targetSelector.SelectTargetAsync(
                new TargetSelectionRequest(request.Source, targetState.Target),
                cancellationToken);

            return ProcessSelection(selectedUnit, targetState, request);

        } catch (OperationCanceledException) {
            throw;
        } catch (Exception ex) {
            GameLogger.LogException(ex);
            GameLogger.LogError($"Selector error for {targetState.Name}: {ex.Message}", LogCategory.TargetsFiller);
            return TargetSelectionResult.Retry();
        }
    }

    

    private TargetSelectionResult ProcessSelection(UnitModel unit, TargetState targetState, TargetOperationRequest request) {
        if (unit == null) {
            return HandleNullSelection(request);
        }

        // Валідація
        var validationResult = validator.ValidateTarget(unit, targetState.Target, request.Source.GetPlayer());
        if (!validationResult.IsValid) {
            GameLogger.LogWarning($"Invalid target {unit} for {targetState.Name}: {validationResult.ErrorMessage}",
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
    public object SelectedUnit { get; private set; }

    private TargetSelectionResult(SelectionAction action, object unit = null) {
        Action = action;
        SelectedUnit = unit;
    }

    public static TargetSelectionResult Success(object unit) =>
        new(SelectionAction.Success, unit);

    public static TargetSelectionResult Retry() =>
        new(SelectionAction.Retry);

    public static TargetSelectionResult Cancel() =>
        new(SelectionAction.Cancel);

    public static TargetSelectionResult ClearDuplicate(object unit) =>
        new(SelectionAction.ClearDuplicate, unit);
}

public enum SelectionAction {
    Success,
    Retry,
    Cancel,
    ClearDuplicate
}

public interface ITargetValidator {
    ValidationResult ValidateTarget(UnitModel unit, TypedTargetBase targetBase, Opponent initiator);
    bool CanValidateAllTargets(List<TypedTargetBase> targets);
}

public class TargetValidator : ITargetValidator {
    public ValidationResult ValidateTarget(UnitModel unit, TypedTargetBase targetBase, Opponent initiator) {
        try {
            return targetBase.IsValid(unit, initiator);
        } catch (Exception ex) {
            GameLogger.LogError($"Validation error: {ex.Message}", LogCategory.TargetsFiller);
            return ValidationResult.Error($"Validation failed: {ex.Message}");
        }
    }

    // Soon it will search and compose all possible targets
    public bool CanValidateAllTargets(List<TypedTargetBase> targets) {
        return true;
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

