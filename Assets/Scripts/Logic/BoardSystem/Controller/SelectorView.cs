using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class SelectorView : MonoBehaviour {
    public Action<List<UnitView>> OnTargetSelected;

    [Inject] private readonly InputManager inputManager;
    private InputSystem_Actions.BoardPlayerActions boardInputs;

    [SerializeField] TextMeshProUGUI selectionPropmtText;
    
    public LayerMask targetLayerMask;
    public LayerMask surfaceMask;

    public BoardInputManager boardInputManager;
    public Vector3 LastBoardPosition;

    [Header ("Strategy components")]
    [SerializeField] public CardMovementTargeting cardTargeting;
    [SerializeField] public ArrowTargeting arrowTargeting;
    private ITargetingVisualization currentVisualization;

    
    [Header("Debug")]
    [SerializeField] private bool isDebug = false;
    [SerializeField] private Transform debugIndicator;

    private void Awake() {
        boardInputs = inputManager.inputAsset.BoardPlayer;
    }

    private void Update() {
        UpdateCursorPosition();
        if (currentVisualization != null) {
            currentVisualization.UpdateTargeting(LastBoardPosition);
        }
        
    }

    private void UpdateCursorPosition() {
        if (boardInputManager.TryGetCursorPosition(surfaceMask, out Vector3 cursorPosition)) {
            LastBoardPosition = cursorPosition;
            if (isDebug && debugIndicator != null)
                debugIndicator.transform.position = LastBoardPosition;
        }
    }

    public void StartTargeting(ITargetingVisualization visualization) {
        if (visualization == null) return;

        StopTargeting(); // очистити попередній стан
        currentVisualization = visualization;
        currentVisualization.StartTargeting();
        boardInputs.LeftClick.canceled += OnLeftClickUp;
    }

    private void OnLeftClickUp(UnityEngine.InputSystem.InputAction.CallbackContext context) {
        var views = GetTargetsUnderCursor();
        OnTargetSelected?.Invoke(views);
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

    public void StopTargeting() {
        boardInputs.LeftClick.canceled -= OnLeftClickUp; 
        currentVisualization?.StopTargeting();
        currentVisualization = null;
    }

    private void OnDestroy() {
        boardInputs.LeftClick.canceled -= OnLeftClickUp;
    }

    public void ShowTargetingMessage(string message) {
        if (selectionPropmtText == null) return;
        selectionPropmtText.gameObject.SetActive(true);
        selectionPropmtText.text = message;
    }

    public void HideTargetingMessage() {
        if (selectionPropmtText == null) return;
        selectionPropmtText?.gameObject.SetActive(false);
    }
}
