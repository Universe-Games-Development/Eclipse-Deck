using UnityEngine;
using Zenject;

public class CardPlayModule : MonoBehaviour {
    [SerializeField] HandPresenter handPresenter;
    [SerializeField] ActionPlayModule actionPlayModule;

    [Inject] InputManager inputManager;
    InputSystem_Actions.BoardPlayerActions boardInputs;
    private Vector2 cursorPosition;
    private bool isPlaying = false;

    private void Start() {
        handPresenter.OnCardSelected += StartCardPlay;
        boardInputs = inputManager.inputAsset.BoardPlayer;
    }

    private void Update() {
        if (isPlaying) {
            cursorPosition = boardInputs.CursorPosition.ReadValue<Vector2>();
            Debug.Log(cursorPosition);
        }
    }

    private void StartCardPlay(CardPresenter presenter) {
        CardView cardView = presenter.View;
        Card card = presenter.Model;
        isPlaying = true;
    }

    private void EndCardPlay(bool isCardCanceled) {
        isPlaying = false;
    }
}
