using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class HumanTargetSelector : MonoBehaviour, ITargetSelector {
    [SerializeField] private Camera gameCamera;
    [SerializeField] private LayerMask targetLayerMask;
    [Inject] InputManager inputManager;
    InputSystem_Actions.BoardPlayerActions boardInputs;

    private TaskCompletionSource<GameUnit> currentSelection;
    public event Action<ITargetRequirement> OnSelectionStarted;

    private void Start() {
        if (gameCamera == null)
        gameCamera = Camera.main;
        boardInputs = inputManager.inputAsset.BoardPlayer;
    }

    public async UniTask<GameUnit> SelectTargetAsync(ITargetRequirement requirement, string targetName, CancellationToken cancellationToken) {
        currentSelection = new TaskCompletionSource<GameUnit>();

        OnSelectionStarted?.Invoke(requirement);
        // Показуємо UI підказки
        ShowSelectionPrompt(requirement.GetInstruction(), targetName);

        // Підписуємося на input
        boardInputs.LeftClick.canceled += OnLeftClickUp;

        try {
            return await currentSelection.Task;
        } finally {
            boardInputs.LeftClick.canceled -= OnLeftClickUp;
            HideSelectionPrompt();
        }
    }

    private void OnLeftClickUp(InputAction.CallbackContext context) {
        if (currentSelection == null) return;

        GameUnit result = null;

        var ray = gameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 10f, targetLayerMask)) {
            if (hit.collider.TryGetComponent<IGameUnitProvider>(out var provider)) {
                result = provider.GetUnit();
            }
        }

        Debug.Log($"Result: {result}");
        currentSelection.TrySetResult(result);
    }

    private void ShowSelectionPrompt(string description, string targetName) {
        Debug.Log($"Select target: {targetName} - {description}");
    }

    private void HideSelectionPrompt() {
    }
}
