using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private LayerMask surfaceMask;
    [SerializeField] private Transform cursorIndicator;

    [SerializeField] private TargetingVisualizationStrategy visualizationStrategy;
    private ITargetingVisualization currentVisualization;

    [Inject] private readonly InputManager inputManager;

    private TaskCompletionSource<UnitModel> currentSelection;
    private InputSystem_Actions.BoardPlayerActions boardInputs;

    public CardPresenter CurrentCard { get; private set; }
    public Vector3 LastBoardPosition { get; private set; }

    [SerializeField] private bool isDebug = false;
    private TargetSelectionRequest currentrequest;

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
        if (boardInputManager.TryGetCursorPosition(surfaceMask, out Vector3 cursorPosition)) {
            LastBoardPosition = cursorPosition;
            if (isDebug)
                cursorIndicator.transform.position = LastBoardPosition;
        }
    }



    public async UniTask<UnitModel> SelectTargetAsync(TargetSelectionRequest selectionRequest, CancellationToken cancellationToken) {
        currentSelection = new TaskCompletionSource<UnitModel>();
        currentrequest = selectionRequest;

        StartTargetingVisualization(selectionRequest);
        ShowSelectionPrompt(selectionRequest.Target.GetInstruction());

        boardInputs.LeftClick.canceled += OnLeftClickUp;

        try {
            return await currentSelection.Task;
        } finally {
            StopTargetingVisualization();
            boardInputs.LeftClick.canceled -= OnLeftClickUp;
            HideSelectionPrompt();
            currentrequest = null;
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
        if (currentSelection == null || currentrequest == null) return;

        var presenters = GetTargetsUnderCursor();
        var models = presenters.Select(presenter => presenter.GetModel()).ToList();

        TypedTargetBase target = currentrequest.Target;
        Opponent opponent = currentrequest.Source.GetPlayer();

        UnitModel satisfyModel = models.Where(model => target.IsValid(model, new ValidationContext(opponent))).FirstOrDefault();

        var result = satisfyModel;

        currentSelection.TrySetResult(result);
    }


    private List<UnitPresenter> GetTargetsUnderCursor() {
        List<UnitPresenter> presenters = new();
        if (boardInputManager.TryGetAllCursorObjects(targetLayerMask, out GameObject[] hitObjects)) {
            foreach (var hitObj in hitObjects) {
                if (hitObj.TryGetComponent(out UnitPresenter presenter)) {
                    presenters.Add(presenter);
                }
            }
        }
        return presenters;
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
    public TypedTargetBase Target { get; }

    public TargetSelectionRequest(UnitModel initiator, TypedTargetBase target) {
        Source = initiator;
        Target = target;
    }
}

