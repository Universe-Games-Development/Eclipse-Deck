using Cysharp.Threading.Tasks;
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

    private void Start() {
        gameCamera ??= Camera.main;
        boardInputs = inputManager.inputAsset.BoardPlayer;
    }

    public async UniTask<GameUnit> SelectTargetAsync(ITargetRequirement requirement, string targetName) {
        currentSelection = new TaskCompletionSource<GameUnit>();

        // Показуємо UI підказки
        ShowSelectionPrompt(requirement.GetInstruction(), targetName);

        // Підписуємося на input
        boardInputs.LeftClick.canceled += OnLeftClick;

        try {
            return await currentSelection.Task;
        } finally {
            boardInputs.LeftClick.canceled -= OnLeftClick;
            HideSelectionPrompt();
        }
    }

    private void OnLeftClick(InputAction.CallbackContext context) {
        if (currentSelection == null) return;

        var ray = gameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 10f, targetLayerMask)) {
            if (hit.collider.TryGetComponent<IGameUnitProvider>(out var provider)) {
                var unit = provider.GetUnit();
                if (unit != null) {
                    currentSelection.TrySetResult(unit);
                }
            }
        }
    }

    private void ShowSelectionPrompt(string description, string targetName) {
        Debug.Log($"Select target: {targetName} - {description}");
    }

    private void HideSelectionPrompt() {
    }
}
