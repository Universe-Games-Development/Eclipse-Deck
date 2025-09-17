using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class HumanTargetSelector : MonoBehaviour, ITargetSelector {
    public Action<TargetSelectionRequest> OnSelectionStarted;
    public Action OnSelectionEnded;

    [Header("Core Components")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private LayerMask targetLayerMask;
    [SerializeField] private BoardInputManager boardInputManager;
    [SerializeField] private LayerMask boardMask;
    [SerializeField] private Transform cursorIndicator;
    
    [SerializeField] private TargetingVisualizationStrategy visualizationStrategy;
    private ITargetingVisualization currentVisualization;

    [Inject] private InputManager inputManager;

    private TaskCompletionSource<UnitModel> currentSelection;
    private InputSystem_Actions.BoardPlayerActions boardInputs;
    
    public CardPresenter CurrentCard { get; private set; }
    public Vector3 LastBoardPosition { get; private set; }

    [SerializeField] private bool isDebug = false;

    private void Start() {
        InitializeComponents();
    }

    private void InitializeComponents() {
        if (gameCamera == null)
            gameCamera = Camera.main;

        if (visualizationStrategy == null) {
            visualizationStrategy = GetComponent<TargetingVisualizationStrategy>();
        }

        if (visualizationStrategy == null) {
            Debug.LogError("HumanTargetSelector: No TargetingVisualizationStrategy assigned or found!");
            enabled = false;
            return;
        }

        if (inputManager == null) {
            Debug.LogError("HumanTargetSelector: No InputManager assigned!");
            return;
        }

        boardInputs = inputManager.inputAsset.BoardPlayer;
    }

    private void Update() {
        UpdateCursorPosition();
        currentVisualization?.UpdateTargeting(LastBoardPosition);
    }

    private void UpdateCursorPosition() {
        if (boardInputManager.TryGetCursorPosition(boardMask, out Vector3 cursorPosition)) {
            LastBoardPosition = cursorPosition;
            if (isDebug)
                cursorIndicator.transform.position = LastBoardPosition;
        }
    }

    public async UniTask<UnitModel> SelectTargetAsync(TargetSelectionRequest selectionRequest, CancellationToken cancellationToken) {
        currentSelection = new TaskCompletionSource<UnitModel>();

        StartTargetingVisualization(selectionRequest);
        ShowSelectionPrompt(selectionRequest.Requirement.GetInstruction());

        boardInputs.LeftClick.canceled += OnLeftClickUp;

        try {
            return await currentSelection.Task;
        } finally {
            StopTargetingVisualization();
            boardInputs.LeftClick.canceled -= OnLeftClickUp;
            HideSelectionPrompt();
        }
    }

    private void StartTargetingVisualization(TargetSelectionRequest selectionRequest) {
        OnSelectionStarted?.Invoke(selectionRequest);

        currentVisualization = visualizationStrategy.CreateVisualization(selectionRequest);
        currentVisualization.StartTargeting();
    }

    private void StopTargetingVisualization() {
        currentVisualization?.StopTargeting();

        currentVisualization = null;
    }

    private void OnLeftClickUp(InputAction.CallbackContext context) {
        if (currentSelection == null) return;

        var presenter = GetTargetUnderCursor();
        var result = presenter?.GetModel();

        currentSelection.TrySetResult(result);
    }


    private UnitPresenter GetTargetUnderCursor() {
        if (boardInputManager.TryGetCursorObject(boardMask, out GameObject hitObject)) {
            hitObject.TryGetComponent(out UnitPresenter presenter);
            return presenter;
        }
        return null;
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
    void StartTargeting();
    void UpdateTargeting(Vector3 cursorPosition);
    void StopTargeting();
}

public class TargetSelectionRequest {
    public UnitModel Source { get; } // Карта, істота, гравець
    public ITargetRequirement Requirement { get; }

    public TargetSelectionRequest(UnitModel initiator, ITargetRequirement requirement) {
        Source = initiator;
        Requirement = requirement;
    }
}

