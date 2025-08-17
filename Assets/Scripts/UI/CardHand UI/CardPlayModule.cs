using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Zenject;

public class CardPlayModule : MonoBehaviour {
    [SerializeField] HandPresenter handPresenter;
    [SerializeField] OperationManager operationManager;
    [SerializeField] LayerMask layerMask;
    [SerializeField] BoardInputManager boardInputManager;
    [SerializeField] private Transform _testObject;
    [SerializeField] private float _movementSpeed = 1f;
    [SerializeField] private Vector3 _cardOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] BoardPlayer player;

    private bool isPlaying = false;
    private CardPlayData playData;

    private void Start() {
        handPresenter.OnCardClicked += OnCardClicked;

        operationManager.OnOperationStatus += HandleOperationStatus;
    }

    private void Update() {
        if (isPlaying) {
            if (boardInputManager.TryGetCursorPosition(layerMask, out Vector3 cursorPosition)) {
                UpdateCardPosition(cursorPosition);
            }
           
            //Debug.Log(cursorPosition);
        }
    }

    private void UpdateCardPosition(Vector3 cursorPosition) {
        if (playData?.View == null) return;

        _testObject.transform.position = cursorPosition;
        var targetPosition = cursorPosition + _cardOffset;
        playData.View.transform.position = Vector3.Lerp(
            playData.View.transform.position,
            targetPosition,
            Time.deltaTime * _movementSpeed);
    }

    private void HandleOperationStatus(GameOperation operation, OperationStatus status) {
        if (!isPlaying || playData == null) return;
        if (!playData.Card.Operations.Contains(operation)) return;

        switch (status) {
            case OperationStatus.Success:
                playData.IsStarted = true;
                if (playData.IsLastOperation(operation)) {
                    FinishCardPlay();
                } else {
                    // Ждем освобождения менеджера и подаем следующую операцию
                    StartCoroutine(WaitAndSubmitNext());
                }
                break;
            case OperationStatus.Canceled:
                HandleOperationCanceled(operation);
                break;
            case OperationStatus.Failed:
                HandleOperationFailed(operation);
                break;
        }
    }

    private IEnumerator WaitAndSubmitNext() {
        // Ждем пока текущая операция завершится и менеджер освободится
        yield return new WaitUntil(() => !operationManager.IsRunning && operationManager.QueueCount == 0);

        // Небольшая задержка для отработки реакций
        yield return new WaitForSeconds(0.1f);

        yield return new WaitUntil(() => !operationManager.IsRunning && operationManager.QueueCount == 0);

        SubmitNextOperation();
    }

    private void HandleOperationCanceled(GameOperation operation) {
        if (playData != null && !playData.IsStarted) {
            RollbackCardPlay();
        }
    }

    private void HandleOperationFailed(GameOperation operation) {
        if (playData != null && !playData.IsStarted) {
            RollbackCardPlay();
        }
    }

    private void RollbackCardPlay() {
        if (!isPlaying || playData == null) return;
        
        handPresenter.RetrieveCard(playData.Presenter);

        GameLogger.LogDebug($"Card {playData.Card.Data.Name} returned to hand");
        isPlaying = false;
        playData = null;
    }

    private void OnCardClicked(CardPresenter cardPresenter) {
        if (isPlaying || cardPresenter == null) {
            GameLogger.LogWarning("Card click ignored - already playing or null card");
            return;
        }

        playData = new(cardPresenter);
        isPlaying = true;

        // Подаем только первую операцию
        SubmitNextOperation();
    }

    private void SubmitNextOperation() {
        if (playData?.HasNextOperation() == true) {
            var nextOperation = playData.GetNextOperation();
            nextOperation.Initiator = player;
            operationManager.Push(nextOperation);
        }
    }

    private void FinishCardPlay() {
        playData = null;
        isPlaying = false;
    }
}

public class CardPlayData {
    public Card Card;
    public CardPresenter Presenter;
    public Card3DView View;
    public bool IsStarted = false;
    public int CurrentOperationIndex = 0;
    public int CompletedOperations = 0;

    public CardPlayData(CardPresenter presenter) {
        Card = presenter.Card;
        View = presenter.View as Card3DView;
        Presenter = presenter;
    }

    public bool HasNextOperation() => CurrentOperationIndex < Card.Operations.Count;

    public GameOperation GetNextOperation() {
        if (HasNextOperation()) {
            return Card.Operations[CurrentOperationIndex++];
        }
        return null;
    }

    public bool IsLastOperation(GameOperation operation) {
        return Card.Operations.LastOrDefault() == operation;
    }
}

public struct CardPlayedEvent : IEvent {
    public Card playedCard;
    // think how to collect data "by who played"
}
