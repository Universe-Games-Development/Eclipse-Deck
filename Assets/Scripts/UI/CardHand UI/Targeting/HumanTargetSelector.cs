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
    [SerializeField] private Transform playerPortrait; // Додати посилання на портрет гравця

    [Header("Visualization")]
    [SerializeField] private TargetingVisualizationFactory visualizationFactory;
    [SerializeField] private CardPlayModule cardPlayModule;

    [Inject] private InputManager inputManager;

    // State
    public CardPresenter CurrentCard { get; private set; }
    private TaskCompletionSource<UnitPresenter> currentSelection;
    private InputSystem_Actions.BoardPlayerActions boardInputs;
    private ITargetingVisualization currentVisualization;

    public Vector3 LastBoardPosition { get; private set; }
    public event Action<TargetSelectionRequest> OnSelectionStarted;

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
        currentVisualization?.UpdateTargeting();
    }

    private void UpdateCursorPosition() {
        if (boardInputManager.TryGetCursorPosition(boardMask, out Vector3 cursorPosition)) {
            LastBoardPosition = cursorPosition;
            cursorIndicator.transform.position = LastBoardPosition;
        }
    }

    // ITargetSelector implementation
    public async UniTask<UnitPresenter> SelectTargetAsync(TargetSelectionRequest selectionRequst, CancellationToken cancellationToken) {
        currentSelection = new TaskCompletionSource<UnitPresenter>();

        // Створюємо відповідну візуалізацію
        StartTargetingVisualization(selectionRequst);

        ShowSelectionPrompt(selectionRequst.Requirement.GetInstruction());
        boardInputs.LeftClick.canceled += OnLeftClickUp;

        try {
            return await currentSelection.Task;
        } finally {
            StopTargetingVisualization();
            boardInputs.LeftClick.canceled -= OnLeftClickUp;
            HideSelectionPrompt();
        }
    }

    private void StartTargetingVisualization(TargetSelectionRequest selectionRequst) {
        OnSelectionStarted?.Invoke(selectionRequst);

        currentVisualization = visualizationFactory.CreateVisualization(selectionRequst, CurrentCard);

        currentVisualization.StartTargeting(
            () => LastBoardPosition,
            selectionRequst
        );
    }

    private void StopTargetingVisualization() {
        currentVisualization?.StopTargeting();
        currentVisualization = null;
    }

    private void OnLeftClickUp(InputAction.CallbackContext context) {
        if (currentSelection == null) return;

        UnitPresenter result = GetTargetUnderCursor();
        currentSelection.TrySetResult(result);
    }

    private UnitPresenter GetTargetUnderCursor() {
        UnitPresenter presenter = null;

        if (boardInputManager.TryGetCursorObject(boardMask, out GameObject hitObject)) {
            hitObject.TryGetComponent<UnitPresenter>(out presenter);
        }

        return presenter;
    }

    public void SetCurrentCard(CardPresenter cardPresenter) {
        CurrentCard = cardPresenter;
    }

    private void ShowSelectionPrompt(string description) {
        Debug.Log($"Select target: {description}");
    }

    private void HideSelectionPrompt() {
        // TODO: Hide UI prompt
    }
}

public interface ITargetingVisualization {
    void StartTargeting(Func<Vector3> targetPositionProvider, TargetSelectionRequest targetSelectionRequest);
    void StopTargeting();
    void UpdateTargeting();
}

public class TargetSelectionRequest {
    public UnitPresenter Initiator { get; } // Карта, істота, гравець
    public ITargetRequirement Requirement { get; }

    public TargetSelectionRequest(UnitPresenter initiator, ITargetRequirement requirement) {
        Initiator = initiator;
        Requirement = requirement;
    }
}
