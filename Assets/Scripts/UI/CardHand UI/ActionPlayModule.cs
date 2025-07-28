using System;
using UnityEngine;
using Zenject;

public class ActionPlayModule : MonoBehaviour {
    [Inject] InputManager inputManager;
    InputSystem_Actions.BoardPlayerActions boardInputs;

    public Action OnActionQueueCanceled;
    public Action OnActionQueueCompleted;

    private void Start() {
        boardInputs = inputManager.inputAsset.BoardPlayer;
    }
}
