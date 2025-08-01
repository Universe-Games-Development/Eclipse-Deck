using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;


public class CardPlayModule : MonoBehaviour {
    [SerializeField] HandPresenter handPresenter;
    [SerializeField] OperationPlayModule operationPlayModule;

    [Inject] InputManager inputManager;
    InputSystem_Actions.BoardPlayerActions boardInputs;
    private Vector2 cursorPosition;
    private bool isPlaying = false;
    private CardPlayData playData;

    private void Start() {
        boardInputs = inputManager.inputAsset.BoardPlayer;
        handPresenter.OnCardClicked += OnCardClicked;

        operationPlayModule.OnActionCompleted += HandleOperationCompeted;
        operationPlayModule.OnActionCancelled += HandleOperationCacnceled;
    }

    private void HandleOperationCacnceled(GameOperation operation) {
        // if we not active
        if (!isPlaying || playData == null) return;

        // if its not ours
        if (!playData.operationSucess.ContainsKey(operation)) {
            return;
        }

        // If thats first canceled operation return card
        if (!playData.IsStarted()) {
            handPresenter.RetrieveCard(playData.Presenter);
            EndCardPlay();
        }

        // try get next operation or end
        if (!TryContinueOperations()) {
            EndCardPlay();
        }

    }

    private bool TryContinueOperations() {
        if (playData.TryGetNextOperation(out GameOperation operation)) {
            operationPlayModule.ProcessOperation(operation);
            return true;
        }
        return false;
    }

    private void HandleOperationCompeted(GameOperation operation) {
        if (playData == null) return;
        if (playData.operationSucess.ContainsKey(operation)) {
            playData.operationSucess[operation] = true;
        }
    }

    private void OnCardClicked(CardPresenter cardPresenter) {
        if (isPlaying) {
            return;
        }

        StartCardPlay(cardPresenter);
    }

    private void StartCardPlay(CardPresenter presenter) {
        playData = new(presenter);
        isPlaying = true;
        TryContinueOperations();
    }

    private void EndCardPlay() {
        isPlaying = false;
        playData = null;
    }


    private void Update() {
        if (isPlaying) {
            cursorPosition = boardInputs.CursorPosition.ReadValue<Vector2>();
            if (playData.View != null) {
                CardView cardView = playData.View;
                if (cardView is Card3DView card3DView) {
                    card3DView.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(cursorPosition.x, cursorPosition.y, 10f));
                } else {
                    cardView.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(cursorPosition.x, cursorPosition.y, 10f));
                }
            }
            //Debug.Log(cursorPosition);
        }
    }
}

public class CardPlayData {
    public Card Card;
    public CardPresenter Presenter;
    public Card3DView View;
    public Dictionary<GameOperation, bool> operationSucess = new();
    public CardPlayData(CardPresenter presenter) {
        Card = presenter.Model; ;
        View = presenter.View as Card3DView;
        Presenter = presenter;
        foreach (var operation in Card.Operations) {
            operationSucess.Add(operation, false);
        }
    }

    public bool IsStarted() {
        return operationSucess.ContainsValue(true);
    }

    public bool TryGetNextOperation(out GameOperation nextOperation) {
        nextOperation = null;
        foreach (var pair in operationSucess) {
            if (pair.Value == false) {
                nextOperation = pair.Key;
                return true;
            }
        }
        return false;
    }
}

public struct CardPlayedEvent : IEvent {
    public Card playedCard;
    // think how to collect data "by who played"
}