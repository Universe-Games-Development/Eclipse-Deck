using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using Zenject;

public class SelectorView : MonoBehaviour {
    public event Action<GameObject[]> OnTargetsSelected;

    [Inject] private readonly InputManager inputManager;
    private InputSystem_Actions.BoardPlayerActions boardInputs;

    [Header("Visual Components")]
    [SerializeField] private TextMeshProUGUI selectionPromptText;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private float tempMessageSeconds = 3f;

    [Header("Targeting")]
    [SerializeField] private LayerMask targetLayerMask;
    [SerializeField] private LayerMask surfaceMask;
    [SerializeField] private BoardInputManager boardInputManager;
    public Vector3 LastBoardPosition { get; private set; }

    [Header("Targeting Visualizations")]
    [SerializeField] private CardMovementTargeting cardTargeting;
    [SerializeField] private ArrowTargeting arrowTargeting;

    private ITargetingVisualization _currentVisualization;
    private CancellationTokenSource _messageCancellation;

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
        _currentVisualization?.UpdateTargeting(LastBoardPosition);
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

        _currentVisualization = visualization;
        _currentVisualization.StartTargeting();

        boardInputs.LeftClick.canceled += OnLeftClickUp;
    }

    public void StopTargeting() {
        boardInputs.LeftClick.canceled -= OnLeftClickUp;

        _currentVisualization?.StopTargeting();
        _currentVisualization = null;

        HideMessage();
    }

    private void OnLeftClickUp(UnityEngine.InputSystem.InputAction.CallbackContext context) {
        boardInputManager.TryGetAllCursorObjects(targetLayerMask, out GameObject[] hitObjects);
        OnTargetsSelected?.Invoke(hitObjects); // even if its empty
    }


    #endregion

    #region Messages
    public async UniTask ShowTemporaryError(string message) {
        _messageCancellation?.Cancel();
        _messageCancellation = new CancellationTokenSource();

        try {
            ShowErrorMessage(message);

            await UniTask.Delay(TimeSpan.FromSeconds(tempMessageSeconds), cancellationToken: _messageCancellation.Token);

            HideErrorMessage();
        } catch (OperationCanceledException) {
            // Нове повідомлення перервало попереднє
        }
    }

    private void ShowErrorMessage(string message) {
        if (errorText != null) {
            errorText.gameObject.SetActive(true);
            errorText.text = $"<color=red>{message}</color>";
        }
    }

    public void HideErrorMessage() {
        _messageCancellation?.Cancel();
        if (errorText != null) {
            errorText.gameObject.SetActive(false);
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
        arrowTargeting.Initialize();
        return arrowTargeting;
    }
    #endregion

    private void OnDestroy() {
        boardInputs.LeftClick.canceled -= OnLeftClickUp;
        _messageCancellation?.Cancel();
        _messageCancellation?.Dispose();
    }

    public void UpdateHoverStatus(TargetValidationState state) {
        _currentVisualization?.UpdateHoverStatus(state);
    }
}