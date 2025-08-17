using UnityEngine;

public class PlayerController : MonoBehaviour{
    public HandPresenter handPresenter;

    private PlayerState currentState;

    private void Start() {
        handPresenter ??= GetComponent<HandPresenter>();
        if (handPresenter == null) {
            Debug.LogError("HandPresenter is not assigned to PlayerController.");
            return;
        }
        ChangeState(new IdleState());
    }
    private void ChangeState(PlayerState newState) {
        if (currentState != null && currentState.GetType() == newState.GetType()) {
            return; // Не менять состояние, если оно уже такое же
        }
        currentState?.Exit();
        
        currentState = newState;
        currentState.controller = this;
        currentState.Enter();
    }
}

public abstract class State {
    public virtual void Enter() { }
    public virtual void Exit() { }
}

public class PlayerState : State {
    public PlayerController controller;
}

public class PassiveState : PlayerState {

}
// hover card, select card to play
public class IdleState : PlayerState {
    private HandPresenter handPresenter;
    public override void Enter() {
        base.Enter();
        handPresenter = controller.handPresenter;
        handPresenter.OnCardClicked += OnCardClicked;
        handPresenter.OnCardHovered += OnCardHovered;
        // Здесь можно добавить логику, которая выполняется при входе в состояние Playing
    }

    private void OnCardClicked(CardPresenter presenter) {
        //Debug.Log($"Card clicked: {presenter.Card.Data.Name}");
    }

    private void OnCardHovered(CardPresenter presenter, bool isHovered) {
        //Debug.Log($"Card hovered: {presenter.Card.Data.Name}");
    }

    public override void Exit() {
        base.Exit();
        handPresenter.OnCardClicked -= OnCardClicked;
        handPresenter.OnCardHovered -= OnCardHovered;
        // Здесь можно добавить логику, которая выполняется при выходе из состояния Playing
    }
}

public class PlayingState : PlayerState {
    public CardPlayData playData;
    public bool isPlaying = false;
    public override void Enter() {
        base.Enter();
        // Здесь можно добавить логику, которая выполняется при входе в состояние Playing
    }
    public override void Exit() {
        base.Exit();
        // Здесь можно добавить логику, которая выполняется при выходе из состояния Playing
    }
}