using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using Zenject;

public class SelectorView : MonoBehaviour {
    public event Action<List<UnitView>> OnTargetsSelected;

    [Inject] private readonly InputManager inputManager;
    private InputSystem_Actions.BoardPlayerActions boardInputs;

    [Header("Messaging")]
    [SerializeField] private TextMeshProUGUI selectionPromptText;
    [SerializeField] private TextMeshProUGUI ErrorText;
    [SerializeField] float tempMessageSeconds = 3f;
    private CancellationTokenSource messageCancellation;

    [Header("Targeting")]
    [SerializeField] private LayerMask targetLayerMask;
    [SerializeField] private LayerMask surfaceMask;
    [SerializeField] private BoardInputManager boardInputManager;
    public Vector3 LastBoardPosition { get; private set; }
    private ITargetingVisualization currentVisualization;

    [SerializeField] public CardMovementTargeting cardTargeting;
    [SerializeField] public ArrowTargeting arrowTargeting;

    [Header("Debug")]
    [SerializeField] private bool isDebug = false;
    [SerializeField] private Transform debugIndicator;

    private void Awake() {
        boardInputs = inputManager.inputAsset.BoardPlayer;
        HideErrorMessage();
        HideMessage();
    }

    private void Update() {
        UpdateCursorPosition();
        currentVisualization?.UpdateTargeting(LastBoardPosition);
    }

    private void UpdateCursorPosition() {
        if (boardInputManager.TryGetCursorPosition(surfaceMask, out Vector3 cursorPosition)) {
            LastBoardPosition = cursorPosition;

            if (isDebug && debugIndicator != null) {
                debugIndicator.position = LastBoardPosition;
            }
        }
    }

    #region Targeting
    public void StartTargeting(ITargetingVisualization visualization) {
        if (visualization == null) return;

        StopTargeting();

        currentVisualization = visualization;
        currentVisualization.StartTargeting();

        boardInputs.LeftClick.canceled += OnLeftClickUp;
    }

    public void StopTargeting() {
        boardInputs.LeftClick.canceled -= OnLeftClickUp;
        currentVisualization?.StopTargeting();
        currentVisualization = null;
        HideMessage();
    }

    private void OnLeftClickUp(UnityEngine.InputSystem.InputAction.CallbackContext context) {
        var views = GetTargetsUnderCursor();
        OnTargetsSelected?.Invoke(views);
    }

    private List<UnitView> GetTargetsUnderCursor() {
        var views = new List<UnitView>();

        if (boardInputManager.TryGetAllCursorObjects(targetLayerMask, out GameObject[] hitObjects)) {
            foreach (var hitObj in hitObjects) {
                if (hitObj.TryGetComponent(out UnitViewProvider provider)) {
                    var view = provider.GetUnitView();
                    if (view != null) {
                        views.Add(view);
                    }
                } else if (hitObj.TryGetComponent(out UnitView directView)) {
                    views.Add(directView);
                }
            }
        }

        return views;
    }

    #endregion

    #region Messages
    public async UniTask ShowTemporaryError(string message) {
        messageCancellation?.Cancel();
        messageCancellation = new CancellationTokenSource();

        try {
            ShowErrorMessage(message);

            await UniTask.Delay(TimeSpan.FromSeconds(tempMessageSeconds), cancellationToken: messageCancellation.Token);

            HideErrorMessage();
        } catch (OperationCanceledException) {
            // Нове повідомлення перервало попереднє
        }
    }

    private void ShowErrorMessage(string message) {
        if (ErrorText != null) {
            ErrorText.gameObject.SetActive(true);
            ErrorText.text = $"<color=red>{message}</color>";
        }
    }

    public void HideErrorMessage() {
        messageCancellation?.Cancel();
        if (ErrorText != null) {
            ErrorText.gameObject.SetActive(false);
        }
    }

    public void ShowMessage(string message) {
        if (selectionPromptText != null) {
            selectionPromptText.gameObject.SetActive(true);
            selectionPromptText.text = message;
        }
    }

    private void HideMessage() {
        if (selectionPromptText != null) {
            selectionPromptText.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Visuals
    public ITargetingVisualization CreateCardMovementTargeting(CardPresenter presenter) {
        cardTargeting.Initialize(presenter);
        return cardTargeting;
    }

    public ITargetingVisualization CreateArrowTargeting(TargetSelectionRequest request) {
        arrowTargeting.Initialize(request);
        return arrowTargeting;
    }
    #endregion

    private void OnDestroy() {
        boardInputs.LeftClick.canceled -= OnLeftClickUp;
        messageCancellation?.Cancel();
        messageCancellation?.Dispose();
    }
}