using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class OperationPlayModule : MonoBehaviour {
    public event Action<GameOperation> OnActionCompleted;
    public event Action<GameOperation> OnActionCancelled;

    [Header("Settings")]
    public LayerMask boardLayerMask = -1;
    public Camera gameCamera;
    public UnityEngine.UI.Button cancelButton;

    private SelectorService selectorService;

    [Inject] InputManager inputManager;
    InputSystem_Actions.BoardPlayerActions boardInputs;


    List<NamedTarget> currentRequirenments;
    private NamedTarget CurrentNamedTarget =>
       currentOperation != null && currentTargetIndex < currentOperation.namedTargets.Count
           ? currentOperation.namedTargets[currentTargetIndex] : null;


    private GameOperation currentOperation;
    private int currentTargetIndex = 0;
    private bool isWaitingForTarget = false;
    private bool isProcessing = false;

    private void Start() {
        boardInputs = inputManager.inputAsset.BoardPlayer;
        selectorService = new();
        if (gameCamera == null)
            gameCamera = Camera.main;

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelCurrentSelection);
    }

    public void CancelCurrentSelection() {
        if (!isWaitingForTarget) return;

        if (currentOperation.CanBeCancelled) {
            // Отменяем всё действие
            CancelAction();
        }
    }

    public void ProcessOperation(GameOperation operation) {
        if (isProcessing) {
            Debug.LogWarning("ActionPlayModule уже обрабатывает действие!");
            return;
        }

        currentOperation = operation;
        currentTargetIndex = 0;
        isProcessing = true;

        Debug.Log($"Начинаем обработку действия: {currentOperation.actionName}");

        ProcessNextTarget();
    }

    private void ProcessNextTarget() {
        if (CurrentNamedTarget == null) {
            // Все цели выбраны - завершаем действие
            CompleteAction();
            return;
        }

        Debug.Log($"Обрабатываем цель: {CurrentNamedTarget.targetKey} ({CurrentNamedTarget.requirement.GetInstruction()})");

        StartTargetSelection();
    }

    private void StartTargetSelection() {
        isWaitingForTarget = true;

        // Показываем UI для выбора цели
        boardInputs.LeftClick.canceled += TryCollectGameTarget;

        if (cancelButton != null)
            cancelButton.gameObject.SetActive(currentOperation.CanBeCancelled);

        Debug.Log($"Ожидаем выбор цели для: {CurrentNamedTarget.targetKey}");
    }

    private void CompleteAction() {
        isProcessing = false;
        isWaitingForTarget = false;

        // Выполняем действие
        //currentAction.Execute();

        Debug.Log($"Действие {currentOperation.actionName} завершено");
        OnActionCompleted?.Invoke(currentOperation);

        // Очищаем состояние
        currentOperation = null;
        currentTargetIndex = 0;
    }

    private void CancelAction() {
        isProcessing = false;
        isWaitingForTarget = false;
        ToggleTargetingUI(false);

        OnActionCancelled?.Invoke(currentOperation);

        Debug.Log($"Действие {currentOperation.actionName} отменено");

        // Очищаем состояние
        currentOperation = null;
        currentTargetIndex = 0;
    }

    public void EndRequirenmentCollect() {
        boardInputs.LeftClick.canceled -= TryCollectGameTarget;
        currentTargetIndex = 0;
    }

    private void TryCollectGameTarget(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f)) {
            Debug.Log($"Hit: {hit.collider.name} at {hit.point}");
        }
    }

    private void ToggleTargetingUI(bool value) {
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(value);
    }
}

public abstract class GameOperation {
    public List<NamedTarget> namedTargets = new();
    public string actionName = "defaultOperationName";

    public bool CanBeCancelled { get; internal set; }
}

public abstract class Condition {
    public bool isMet(GameUnit gameUnit) {
        return true; // Placeholder for actual logic
    }
}

public struct OperationQueueFinishedEvent {
    OperationsQueueState state;

    public OperationQueueFinishedEvent(OperationsQueueState state) {
        this.state = state;
    }
}

// Clean means queue is not started or has no actions.(card may return to hand)
public enum OperationsQueueState {
    NotStarted, Partically, Completed
}

public class SelectorService {
    BoardGame boardGame; // will be used to search for game units matching the action requirements
    public bool IsPossibleAction(GameOperation action) {
        return true; // Placeholder for actual logic
    }
}