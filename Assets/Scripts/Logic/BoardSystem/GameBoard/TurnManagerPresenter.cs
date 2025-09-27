using UnityEngine;
using Zenject;

public class TurnManagerPresenter : MonoBehaviour {
    [Inject] private TurnManager turnManager;

    [SerializeField] private TurnButtonView turnButtonView;

    private void Awake() {
        if (turnButtonView == null) {
            Debug.LogError("TurnButtonView is not assigned!");
            return;
        }
        turnButtonView.OnTurnButtonClicked += HandleTurnButtonClicked;
    }


    private void HandleTurnButtonClicked() {
        if (turnManager == null) {
            Debug.LogError("Dependencies not initialized!");
            return;
        }
    }

    private void OnDestroy() {
        if (turnButtonView != null)
            turnButtonView.OnTurnButtonClicked -= HandleTurnButtonClicked;
    }
}
