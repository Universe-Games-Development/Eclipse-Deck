using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using Zenject;

public class PlayManagerRegistrator : IDisposable {
    private readonly Dictionary<Opponent, CardPlayService> cardPlayServices = new();
    private GameBoardController boardController; // Используется для розыгрыша карт
    private TurnManager turnManager;

    [Inject]
    private void Construct(GameBoardController boardController, TurnManager turnManager) {
        this.boardController = boardController ?? throw new ArgumentNullException(nameof(boardController));
        this.turnManager = turnManager;
    }

    public void EnablePlayCardServices(List<Opponent> opponents) {
        foreach (var opponent in opponents) {
            if (!cardPlayServices.ContainsKey(opponent)) {
                var service = new CardPlayService(opponent, boardController, turnManager);
                cardPlayServices.Add(opponent, service);
                Debug.Log($"CardPlayService created for opponent {opponent}");
            } else {
                Debug.LogWarning($"CardPlayService for opponent {opponent} already exists.");
            }
        }
    }

    public void StopPlaying(Opponent opponent) {
        if (opponent == null) return;

        if (cardPlayServices.TryGetValue(opponent, out var service)) {
            service.Dispose();
            cardPlayServices.Remove(opponent);
            Debug.Log($"Stopped CardPlayService for opponent {opponent}");
        } else {
            Debug.LogWarning($"No CardPlayService found for opponent {opponent} to stop.");
        }
    }

    public void Dispose() {
        foreach (var service in cardPlayServices.Values) {
            service.Dispose();
        }
        cardPlayServices.Clear();
        GC.SuppressFinalize(this);
    }
}

public class CardPlayService : IDisposable {
    private readonly GameBoardController boardController;
    private readonly Opponent opponent;
    private readonly CardHand cardHand;
    private bool _isPlaying;

    private Card bufferedCard;
    private TurnManager _turnManager;
    private CancellationTokenSource _playCTS;

    public CardPlayService(Opponent opponent, GameBoardController boardController, TurnManager turnManager) {
        this.boardController = boardController ?? throw new ArgumentNullException(nameof(boardController));
        this.opponent = opponent ?? throw new ArgumentNullException(nameof(opponent));
        cardHand = opponent.hand ?? throw new ArgumentNullException(nameof(opponent.hand));

        cardHand.OnCardSelected += OnCardSelected;
        _turnManager = turnManager;
        _turnManager.OnTurnEnd += CancelPlaying;
    }

    private void OnCardSelected(Card selectedCard) {
        if (selectedCard == null || _isPlaying || _turnManager.ActiveOpponent != opponent || opponent.Health.isDead) return;

        BeginPlayCard(selectedCard).Forget();
    }

    private async UniTask BeginPlayCard(Card card) {
        _isPlaying = true;
        _playCTS?.Dispose();
        _playCTS = new CancellationTokenSource();

        try {
            bufferedCard = card;
            cardHand.SetInteraction(false);
            cardHand.DeselectCurrentCard();
            cardHand.RemoveCard(card);

            bool playResult = await card.PlayCard(
                opponent,
                boardController,
                _playCTS.Token // Додаємо токен
            );

            if (playResult) {
                opponent.CardResource.TrySpend(card.Cost.CurrentValue);
                Debug.Log("Card playing successful");
            } else {
                // Если розыгрыш не удался, возвращаем карту обратно в руку
                cardHand.AddCard(bufferedCard);
                Debug.LogWarning("Card playing canceled");
            }
        } catch (OperationCanceledException) {
            cardHand.AddCard(bufferedCard);
            Debug.Log("Card play canceled");
        } finally {
            _isPlaying = false;
        }
    }

    private void CancelPlaying(Opponent opponent) {
        if (this.opponent == opponent) {
            _playCTS?.Cancel();
        }
    }

    public void Dispose() {
        _playCTS?.Dispose();
        cardHand.OnCardSelected -= OnCardSelected;
    }
}
