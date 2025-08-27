using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class HumanTargetSelector : MonoBehaviour, ITargetSelector {
    [Header("Core Components")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private LayerMask targetLayerMask;
    [SerializeField] private BoardInputManager boardInputManager;
    [SerializeField] private LayerMask boardMask;
    [SerializeField] private Transform cursorIndicator;

    [Header("Visualization")]
    [SerializeField] private ArrowVisualizationController arrowVisualizer;
    [SerializeField] private CardMovementController cardMovement;
    [SerializeField] private CardPlayModule cardPlayModule;

    [Inject] private InputManager inputManager;

    // State
    public CardPresenter CurrentCard { get; private set; }
    private TaskCompletionSource<GameUnit> currentSelection;
    private InputSystem_Actions.BoardPlayerActions boardInputs;

    public Vector3 LastBoardPosition { get; private set; }
    public event Action<ITargetRequirement> OnSelectionStarted;

    private void Start() {
        InitializeComponents();
        SubscribeToEvents();
    }

    private void InitializeComponents() {
        if (gameCamera == null)
            gameCamera = Camera.main;

        boardInputs = inputManager.inputAsset.BoardPlayer;
    }

    private void SubscribeToEvents() {
        cardPlayModule.OnCardPlayStarted += SetCurrentCard;
    }

    private void Update() {
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition() {
        if (boardInputManager.TryGetCursorPosition(boardMask, out Vector3 cursorPosition)) {
            LastBoardPosition = cursorPosition;
            cursorIndicator.transform.position = LastBoardPosition;
        }
    }

    // ITargetSelector implementation
    public async UniTask<GameUnit> SelectTargetAsync(ITargetRequirement requirement, string targetName, CancellationToken cancellationToken) {
        currentSelection = new TaskCompletionSource<GameUnit>();

        // Запускаємо візуалізацію
        StartTargetingVisualization(requirement);

        // Показуємо UI підказки
        ShowSelectionPrompt(requirement.GetInstruction(), targetName);

        boardInputs.LeftClick.canceled += OnLeftClickUp;

        try {
            return await currentSelection.Task;
        } finally {
            // Cleanup
            StopTargetingVisualization();
            boardInputs.LeftClick.canceled -= OnLeftClickUp;
            HideSelectionPrompt();
        }
    }

    private void StartTargetingVisualization(ITargetRequirement requirement) {
        OnSelectionStarted?.Invoke(requirement);

        if (CurrentCard == null) {
            Vector3 playerPortraitPos = Vector3.zero;
            arrowVisualizer.ShowArrow(playerPortraitPos, () => LastBoardPosition);
        }

        // Якщо є карта, вибираємо стратегію на основі типу requirement
        if (requirement is ZoneRequirement) {
            cardMovement.StartMovement(CurrentCard, () => LastBoardPosition);
        } else {
            Vector3 startPos = CurrentCard.transform.position;
            arrowVisualizer.ShowArrow(startPos, () => LastBoardPosition);
        }
    }

    private void StopTargetingVisualization() {
        arrowVisualizer.Hide();
        cardMovement.ForceStop();
    }

    private void OnLeftClickUp(InputAction.CallbackContext context) {
        if (currentSelection == null) return;

        GameUnit result = GetTargetUnderCursor();
        currentSelection.TrySetResult(result);
    }

    private GameUnit GetTargetUnderCursor() {
        var ray = gameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out var hit, 10f, targetLayerMask)) {
            if (hit.collider.TryGetComponent<IGameUnitProvider>(out var provider)) {
                return provider.GetUnit();
            }
        }

        return null;
    }

    public void SetCurrentCard(CardPresenter cardPresenter) {
        CurrentCard = cardPresenter;
    }

    private void ShowSelectionPrompt(string description, string targetName) {
        Debug.Log($"Select target: {targetName} - {description}");
    }

    private void HideSelectionPrompt() {
        // TODO: Hide UI prompt
    }
}
