using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class OldClumsySelector : MonoBehaviour {
    public Action<TargetSelectionRequest> OnSelectionStarted;
    public Action OnSelectionEnded;

    [Header("Core Components")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private LayerMask targetLayerMask;
    [SerializeField] private BoardInputManager boardInputManager;
    [SerializeField] private LayerMask surfaceMask;
    [SerializeField] private Transform cursorIndicator;

    private ITargetingVisualization currentVisualization;

    [Inject] private readonly InputManager inputManager;
    private InputSystem_Actions.BoardPlayerActions boardInputs;
    private TaskCompletionSource<UnitModel> currentSelection;


    public Vector3 LastBoardPosition;
    public Action<TargetSelectionRequest> OnSelectionRequested { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action<TargetSelectionRequest, UnitModel> OnSelectionFinished { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    [SerializeField] private bool isDebug = false;
    private TargetSelectionRequest currentrequest;
    [Inject] public IUnitRegistry _unitRegistry;

    private void Start() {
        InitializeComponents();
    }

    private void InitializeComponents() {
        if (gameCamera == null)
            gameCamera = Camera.main;


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

        //currentVisualization = CreateVisualization(selectionRequest);
        //currentVisualization.StartTargeting();
    }

    private void StopTargetingVisualization() {
        currentVisualization?.StopTargeting();

        currentVisualization = null;
    }

    private void OnLeftClickUp(InputAction.CallbackContext context) {
        if (currentSelection == null || currentrequest == null) return;

        var views = GetTargetsUnderCursor();
        List<UnitModel> models = views.Select(view => _unitRegistry.GetPresenterByView(view))
            .Where(presenter => presenter != null)
            .Select(presenter => presenter.Model)
            .ToList();

        TypedTargetBase target = currentrequest.Target;
        Opponent opponent = currentrequest.Source.GetPlayer();

        UnitModel satisfyModel = models.Where(model => target.IsValid(model, new ValidationContext(opponent))).FirstOrDefault();

        var result = satisfyModel;

        currentSelection.TrySetResult(result);
    }


    private List<UnitView> GetTargetsUnderCursor() {
        List<UnitView> views = new();
        if (boardInputManager.TryGetAllCursorObjects(targetLayerMask, out GameObject[] hitObjects)) {
            foreach (var hitObj in hitObjects) {
                if (hitObj.TryGetComponent(out UnitView view)) {
                    views.Add(view);
                }
            }
        }
        return views;
    }

    private void ShowSelectionPrompt(string description) {
        Debug.Log($"Select target: {description}");
    }

    private void HideSelectionPrompt() {
        // TODO: Hide UI prompt
    }


}




