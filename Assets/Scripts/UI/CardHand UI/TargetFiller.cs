using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
public class OperationTargetsFiller : MonoBehaviour {

    [SerializeField] private HumanTargetSelector tempSelector; // REMOVE SOON BE DEFINED BY TARGET INFO
    private SelectHelperService selectorService;
    private BoardGame boardGame; // Добавляем ссылку на игру
    private CancellationTokenSource globalCancellationSource = new CancellationTokenSource();
    private CancellationTokenSource currentOperationCancellationSource;

    // Внутренний стан для работы с целями
    private List<NamedTarget> currentTargets;
    private int currentTargetIndex;
    private bool isOperationMandatory = false;
    private BoardPlayer operationInitiator;

    // Константы
    private const int MAX_RETRY_ATTEMPTS = 3;
    private const float OPERATION_TIMEOUT_SECONDS = 30f;

    private void Start() {
        selectorService = new SelectHelperService();
        //GameLogger.LogInfo("OperationTargetsFiller initialized", LogCategory.TargetsFiller);
    }

    private void OnDestroy() {
        globalCancellationSource?.Cancel();
        globalCancellationSource?.Dispose();
        currentOperationCancellationSource?.Cancel();
        currentOperationCancellationSource?.Dispose();

        GameLogger.LogInfo("OperationTargetsFiller destroyed", LogCategory.TargetsFiller);
    }

    public bool CanBeFilled(List<NamedTarget> targetData) {
        if (targetData == null || targetData.Count == 0) {
            GameLogger.LogWarning("Cannot check empty target list", LogCategory.TargetsFiller);
            return false;
        }

        try {
            var result = selectorService.CanFillTargets(targetData);
            GameLogger.LogDebug($"CanBeFilled check for {targetData.Count} targets: {result}", LogCategory.TargetsFiller);
            return result;
        } catch (Exception ex) {
            GameLogger.LogError($"Error checking if targets can be filled: {ex.Message}", LogCategory.TargetsFiller);
            GameLogger.LogException(ex, LogCategory.TargetsFiller);
            return false;
        }
    }

    public async UniTask<Dictionary<string, GameUnit>> FillTargetsAsync(
        List<NamedTarget> namedTargets,
        bool isOperationMandatory,
        BoardPlayer initiator = null,
        CancellationToken cancellationToken = default) {
        // Валідація параметрів
        if (namedTargets == null || namedTargets.Count == 0) {
            GameLogger.LogWarning("Cannot fill empty target list", LogCategory.TargetsFiller);
            return new Dictionary<string, GameUnit>();
        }

        GameLogger.LogInfo($"Starting target filling for {namedTargets.Count} targets (mandatory: {isOperationMandatory})",
            LogCategory.TargetsFiller);

        var startTime = DateTime.UtcNow;

        // Підготовка операції
        if (!SetupOperationTargets(namedTargets, isOperationMandatory, initiator, cancellationToken)) {
            GameLogger.LogError("Failed to setup operation targets", LogCategory.TargetsFiller);
            return null;
        }

        try {
            // Основний цикл вибору цілей з таймаутом
            using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(OPERATION_TIMEOUT_SECONDS));
            using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                currentOperationCancellationSource.Token,
                timeoutSource.Token);

            var result = await ProcessTargetSelectionLoop(combinedTokenSource.Token);

            var duration = DateTime.UtcNow - startTime;
            GameLogger.LogInfo($"Target filling completed in {duration.TotalMilliseconds:F1}ms", LogCategory.TargetsFiller);

            return ValidateAndReturnResult(result);
        } catch (OperationCanceledException) {
            GameLogger.LogInfo("Target filling operation was cancelled", LogCategory.TargetsFiller);
            return null;
        } catch (Exception ex) {
            GameLogger.LogError($"Unexpected error during target filling: {ex.Message}", LogCategory.TargetsFiller);
            GameLogger.LogException(ex, LogCategory.TargetsFiller);
            return null;
        } finally {
            ClearTargets();
        }
    }

    private async UniTask<Dictionary<string, GameUnit>> ProcessTargetSelectionLoop(CancellationToken cancellationToken) {
        int retryCount = 0;
        int processedTargets = 0;

        GameLogger.LogDebug("Starting target selection loop", LogCategory.TargetsFiller);

        while (HasNextTarget() && !cancellationToken.IsCancellationRequested) {
            var currentTargetName = GetCurrentTargetName();

            try {
                GameLogger.LogDebug($"Processing target [{currentTargetIndex + 1}/{currentTargets.Count}]: {currentTargetName}",
                    LogCategory.TargetsFiller);

                var targetResult = await SelectNextTargetAsync(cancellationToken);

                // Обробка результату вибору цілі
                if (targetResult.ShouldCancel) {
                    GameLogger.LogInfo($"Target selection indicates operation should be cancelled at: {currentTargetName}",
                        LogCategory.TargetsFiller);
                    break;
                }

                if (targetResult.IsSuccess) {
                    processedTargets++;
                    FindNextEmptyTarget();
                    retryCount = 0; // Скидаємо лічильник повторів при успіху
                    GameLogger.LogDebug($"Successfully processed target: {currentTargetName}", LogCategory.TargetsFiller);
                } else if (targetResult.ShouldRetry) {
                    retryCount++;
                    GameLogger.LogDebug($"Retry attempt {retryCount}/{MAX_RETRY_ATTEMPTS} for target: {currentTargetName}",
                        LogCategory.TargetsFiller);

                    if (retryCount >= MAX_RETRY_ATTEMPTS) {
                        GameLogger.LogWarning($"Maximum retry attempts ({MAX_RETRY_ATTEMPTS}) reached for target: {currentTargetName}",
                            LogCategory.TargetsFiller);

                        if (!isOperationMandatory) {
                            GameLogger.LogInfo("Breaking from loop due to max retries on optional operation", LogCategory.TargetsFiller);
                            break; // Виходимо для необов'язкової операції
                        }
                        GameLogger.LogWarning("Continuing despite max retries for mandatory operation", LogCategory.TargetsFiller);
                        // Для обов'язкової операції продовжуємо
                    }
                }
            } catch (OperationCanceledException) {
                GameLogger.LogInfo($"Target selection cancelled at: {currentTargetName}", LogCategory.TargetsFiller);
                throw; // Пропускаємо вгору
            } catch (Exception ex) {
                GameLogger.LogError($"Error during target selection for {currentTargetName}: {ex.Message}", LogCategory.TargetsFiller);
                GameLogger.LogDebug($"Target selection exception details: {ex}", LogCategory.TargetsFiller);
                retryCount++;

                if (retryCount >= MAX_RETRY_ATTEMPTS) {
                    GameLogger.LogError($"Too many errors for target {currentTargetName}, aborting target selection",
                        LogCategory.TargetsFiller);
                    break;
                }
            }
        }

        // Перевірка на скасування
        cancellationToken.ThrowIfCancellationRequested();

        GameLogger.LogInfo($"Target selection loop completed. Processed {processedTargets} targets", LogCategory.TargetsFiller);
        return GetFilledTargets();
    }

    private bool SetupOperationTargets(
        List<NamedTarget> namedTargets,
        bool isOperationMandatory,
        BoardPlayer initiator,
        CancellationToken externalToken) {
        try {
            GameLogger.LogDebug("Setting up operation targets", LogCategory.TargetsFiller);

            // Скасовуємо попередню операцію
            currentOperationCancellationSource?.Cancel();
            currentOperationCancellationSource?.Dispose();

            // Створюємо новий токен
            currentOperationCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(
                externalToken,
                globalCancellationSource.Token
            );

            this.isOperationMandatory = isOperationMandatory;
            this.operationInitiator = initiator;
            currentTargets = new List<NamedTarget>(namedTargets);
            currentTargetIndex = 0;

            FindNextEmptyTarget();

            GameLogger.LogInfo($"Operation setup completed for {namedTargets.Count} targets (initiator: {initiator?.name ?? "None"})",
                LogCategory.TargetsFiller);

            // Log target details
            if (GameLogger.IsEnabled(LogCategory.TargetsFiller, LogLevel.Debug)) {
                var targetNames = namedTargets.Select(t => t.Name);
                GameLogger.LogDebug($"Target names: {string.Join(", ", targetNames)}", LogCategory.TargetsFiller);
            }

            return true;
        } catch (Exception ex) {
            GameLogger.LogError($"Failed to setup operation: {ex.Message}", LogCategory.TargetsFiller);
            GameLogger.LogException(ex, LogCategory.TargetsFiller);
            return false;
        }
    }

    private async UniTask<TargetSelectionResult> SelectNextTargetAsync(CancellationToken cancellationToken) {
        var requirement = GetCurrentRequirement();
        var targetName = GetCurrentTargetName();

        if (requirement == null || string.IsNullOrEmpty(targetName)) {
            GameLogger.LogError("Invalid target requirement or name", LogCategory.TargetsFiller);
            return TargetSelectionResult.Cancel();
        }

        GameLogger.LogDebug($"Selecting target for: {targetName} (requirement: {requirement.GetType().Name})",
            LogCategory.TargetsFiller);

        try {
            var selectionStartTime = DateTime.UtcNow;

            /*
             * 
             * 
             * 
             * 
             * 
             * 
             * NEED TO DEFINE SELECTOR BY TARGET INFO
             * 
             * 
             * Get target initiator requirement
             * Get current initiator if 
             * 
             * 
            */
            ITargetSelector selector = DetermineActualSelector(requirement, operationInitiator);

            var selectedUnit = await selector.SelectTargetAsync(
                requirement,
                targetName,
                cancellationToken
            );

            var selectionDuration = DateTime.UtcNow - selectionStartTime;
            GameLogger.LogDebug($"Target selection took {selectionDuration.TotalMilliseconds:F1}ms for: {targetName}",
                LogCategory.TargetsFiller);

            // Перевірка на скасування після вибору
            cancellationToken.ThrowIfCancellationRequested();

            return ProcessTargetSelection(selectedUnit, requirement, targetName);
        } catch (OperationCanceledException) {
            GameLogger.LogInfo($"Target selection cancelled for: {targetName}", LogCategory.TargetsFiller);
            throw; // Пропускаємо exception вгору для правильної обробки
        } catch (Exception ex) {
            GameLogger.LogError($"Error during target selection for {targetName}: {ex.Message}", LogCategory.TargetsFiller);
            GameLogger.LogDebug($"Target selection exception: {ex}", LogCategory.TargetsFiller);
            return TargetSelectionResult.Retry();
        }
    }

    private ITargetSelector DetermineActualSelector(ITargetRequirement requirement, BoardPlayer initiator) {
        var selectorType = requirement.GetTargetSelector();
        BoardPlayer player = selectorType switch {
            TargetSelector.Initiator => initiator,
            TargetSelector.Opponent => boardGame.GetOpponent(initiator),
            TargetSelector.AnyPlayer => initiator, // По умолчанию инициатор, но может быть логика выбора
            TargetSelector.AllPlayers => throw new NotImplementedException("AllPlayers selector requires special handling"),
            _ => initiator
        };

        ITargetSelector selector = player?.Selector;
        if (selector == null) {             
            GameLogger.LogWarning($"No valid selector found for target requirement: {requirement.GetType().Name}", LogCategory.TargetsFiller);
            selector = tempSelector; // Используем временный селектор, если ничего не найдено
        }
        return selector;
    }

    private TargetSelectionResult ProcessTargetSelection(
        GameUnit selectedUnit,
        ITargetRequirement requirement,
        string targetName) {
        // Якщо нічого не вибрано
        if (selectedUnit == null) {
            GameLogger.LogDebug($"No unit selected for target: {targetName}", LogCategory.TargetsFiller);
            return HandleNullSelection();
        }

        GameLogger.LogDebug($"Processing selected unit {selectedUnit} for target: {targetName}", LogCategory.TargetsFiller);

        // Валідація вибраної цілі
        try {
            var validationResult = requirement.IsValid(selectedUnit, operationInitiator);
            if (!validationResult.IsValid) {
                GameLogger.LogWarning($"Invalid target selected for '{targetName}': {validationResult.ErrorMessage ?? "Unknown reason"}",
                    LogCategory.TargetsFiller);
                return HandleInvalidSelection();
            }

            GameLogger.LogDebug($"Target validation passed for {targetName}", LogCategory.TargetsFiller);
        } catch (Exception ex) {
            GameLogger.LogError($"Error during target validation: {ex.Message}", LogCategory.TargetsFiller);
            GameLogger.LogException(ex, LogCategory.TargetsFiller);
            return TargetSelectionResult.Retry();
        }

        // Обробка дублікатів
        var duplicateResult = HandleDuplicateSelection(selectedUnit);
        if (!duplicateResult.IsSuccess) {
            return duplicateResult;
        }

        // Успішне призначення цілі
        return AssignTarget(selectedUnit, targetName);
    }

    private TargetSelectionResult HandleNullSelection() {
        var targetName = GetCurrentTargetName();
        GameLogger.LogDebug($"Handling null selection for target: {targetName}", LogCategory.TargetsFiller);

        // Для одиночної необов'язкової цілі - скасовуємо операцію
        if (!isOperationMandatory && IsSingleTargetOperation()) {
            GameLogger.LogInfo("Single optional target not selected, cancelling operation", LogCategory.TargetsFiller);
            return TargetSelectionResult.Cancel();
        }

        // Для обов'язкової операції - повторюємо спробу
        if (isOperationMandatory) {
            GameLogger.LogInfo("Mandatory operation requires target selection, retrying...", LogCategory.TargetsFiller);
            return TargetSelectionResult.Retry();
        }

        // Для множинних необов'язкових цілей - пропускаємо
        GameLogger.LogInfo("Optional target skipped, moving to next", LogCategory.TargetsFiller);
        return TargetSelectionResult.Success();
    }

    private TargetSelectionResult HandleInvalidSelection() {
        var targetName = GetCurrentTargetName();
        GameLogger.LogDebug($"Handling invalid selection for target: {targetName}", LogCategory.TargetsFiller);

        // Для одиночної необов'язкової цілі можна скасувати
        if (!isOperationMandatory && IsSingleTargetOperation()) {
            GameLogger.LogInfo("Invalid target for single optional operation, allowing retry or cancel", LogCategory.TargetsFiller);
            return TargetSelectionResult.Retry(); // Даємо можливість повторити або скасувати
        }

        // В інших випадках повторюємо спробу
        GameLogger.LogDebug("Retrying target selection due to invalid selection", LogCategory.TargetsFiller);
        return TargetSelectionResult.Retry();
    }

    private TargetSelectionResult HandleDuplicateSelection(GameUnit selectedUnit) {
        var existingIndex = FindTargetWithUnit(selectedUnit);
        if (existingIndex == -1) {
            return TargetSelectionResult.Success();
        }

        GameLogger.LogWarning($"Unit {selectedUnit} already assigned to target at index {existingIndex}, clearing previous assignment",
            LogCategory.TargetsFiller);

        // Очищуємо попереднє призначення
        var previousTargetName = currentTargets[existingIndex].Name;
        currentTargets[existingIndex].Unit = null;

        GameLogger.LogDebug($"Cleared previous assignment: {previousTargetName}", LogCategory.TargetsFiller);

        // Якщо попереднє призначення було раніше, повертаємося до нього
        if (existingIndex < currentTargetIndex) {
            currentTargetIndex = existingIndex;
            GameLogger.LogDebug($"Moved back to earlier target index: {existingIndex}", LogCategory.TargetsFiller);
        }

        return TargetSelectionResult.Success(); // Продовжуємо з поточною ціллю
    }

    private TargetSelectionResult AssignTarget(GameUnit selectedUnit, string targetName) {
        try {
            currentTargets[currentTargetIndex].Unit = selectedUnit;
            GameLogger.LogInfo($"Target '{targetName}' successfully filled with unit: {selectedUnit}", LogCategory.TargetsFiller);
            return TargetSelectionResult.Success();
        } catch (Exception ex) {
            GameLogger.LogError($"Failed to assign target: {ex.Message}", LogCategory.TargetsFiller);
            GameLogger.LogException(ex, LogCategory.TargetsFiller);
            return TargetSelectionResult.Retry();
        }
    }

    private Dictionary<string, GameUnit> ValidateAndReturnResult(Dictionary<string, GameUnit> result) {
        if (result == null) {
            GameLogger.LogWarning("Result is null", LogCategory.TargetsFiller);
            return new Dictionary<string, GameUnit>();
        }

        if (result.Count == 0 && isOperationMandatory) {
            GameLogger.LogError("Mandatory operation completed with no targets filled", LogCategory.TargetsFiller);
            return null;
        }

        if (result.Count > 0) {
            GameLogger.LogInfo($"Operation completed successfully with {result.Count} targets filled", LogCategory.TargetsFiller);

            // Логування деталей заповнених цілей
            if (GameLogger.IsEnabled(LogCategory.TargetsFiller, LogLevel.Debug)) {
                foreach (var kvp in result) {
                    GameLogger.LogDebug($"- {kvp.Key}: {kvp.Value}", LogCategory.TargetsFiller);
                }
            }
        } else {
            GameLogger.LogInfo("Operation completed with no targets filled", LogCategory.TargetsFiller);
        }

        return result;
    }

    // Допоміжні методи
    private bool IsSingleTargetOperation() => currentTargets?.Count == 1;

    private bool HasNextTarget() {
        return currentTargetIndex < currentTargets?.Count;
    }

    private ITargetRequirement GetCurrentRequirement() {
        return HasNextTarget() ? currentTargets[currentTargetIndex].Requirement : null;
    }

    private string GetCurrentTargetName() {
        return HasNextTarget() ? currentTargets[currentTargetIndex].Name : "Unknown";
    }

    private void FindNextEmptyTarget() {
        if (currentTargets == null) return;

        var startIndex = currentTargetIndex;

        for (int i = currentTargetIndex; i < currentTargets.Count; i++) {
            if (currentTargets[i].Unit == null) {
                currentTargetIndex = i;

                if (i != startIndex) {
                    GameLogger.LogDebug($"Moved to next empty target at index {i}: {currentTargets[i].Name}", LogCategory.TargetsFiller);
                }

                return;
            }
        }

        currentTargetIndex = currentTargets.Count; // Всі заповнені
        GameLogger.LogDebug("All targets are filled", LogCategory.TargetsFiller);
    }

    private int FindTargetWithUnit(GameUnit unit) {
        if (currentTargets == null || unit == null) return -1;

        for (int i = 0; i < currentTargets.Count; i++) {
            if (currentTargets[i].Unit == unit) {
                return i;
            }
        }
        return -1;
    }

    private Dictionary<string, GameUnit> GetFilledTargets() {
        var result = new Dictionary<string, GameUnit>();

        if (currentTargets != null) {
            foreach (var target in currentTargets) {
                if (target.Unit != null) {
                    result[target.Name] = target.Unit;
                }
            }
        }

        GameLogger.LogDebug($"Collected {result.Count} filled targets", LogCategory.TargetsFiller);
        return result;
    }

    private void ClearTargets() {
        if (currentTargets != null) {
            var clearedCount = 0;
            foreach (var target in currentTargets) {
                if (target.Unit != null) {
                    target.Unit = null;
                    clearedCount++;
                }
            }

            currentTargets.Clear();
            currentTargets = null;

            GameLogger.LogDebug($"Cleared {clearedCount} target assignments", LogCategory.TargetsFiller);
        }

        currentTargetIndex = 0;
        operationInitiator = null;

        GameLogger.LogDebug("Target filling state reset", LogCategory.TargetsFiller);
    }

    // Додаткові методи для діагностики
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void LogCurrentState() {
        var message = $"Current State - Target Index: {currentTargetIndex}/{currentTargets?.Count ?? 0}, " +
                     $"Mandatory: {isOperationMandatory}, " +
                     $"Current Target: {GetCurrentTargetName()}, " +
                     $"Initiator: {operationInitiator?.name ?? "None"}";

        GameLogger.LogInfo(message, LogCategory.TargetsFiller);

        if (currentTargets != null && GameLogger.IsEnabled(LogCategory.TargetsFiller, LogLevel.Debug)) {
            for (int i = 0; i < currentTargets.Count; i++) {
                var target = currentTargets[i];
                var status = target.Unit != null ? $"Filled with {target.Unit}" : "Empty";
                var marker = i == currentTargetIndex ? " <- CURRENT" : "";
                GameLogger.LogDebug($"  [{i}] {target.Name}: {status}{marker}", LogCategory.TargetsFiller);
            }
        }
    }
}

// Результат вибору цілей (якщо його ще немає)
public struct TargetSelectionResult {
    public bool IsSuccess { get; private set; }
    public bool ShouldRetry { get; private set; }
    public bool ShouldCancel { get; private set; }

    public static TargetSelectionResult Success() => new TargetSelectionResult { IsSuccess = true };
    public static TargetSelectionResult Retry() => new TargetSelectionResult { ShouldRetry = true };
    public static TargetSelectionResult Cancel() => new TargetSelectionResult { ShouldCancel = true };
}

public interface ITargetSelector {
    public abstract UniTask<GameUnit> SelectTargetAsync(ITargetRequirement requirement, string targetName, CancellationToken cancellationToken);
}

public class SelectHelperService {
    BoardGame boardGame; // will be used to search for game units matching the action requirements
    internal bool CanFillTargets(List<NamedTarget> targetData) {
        return true; // TODO: Implement actual logic to check if targets can be filled
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

// will be used to get model of game unit objects
public interface IGameUnitProvider {
    GameUnit GetUnit();
}