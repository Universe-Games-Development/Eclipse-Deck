using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class CardPlayService : IDisposable {
    private readonly GameBoardPresenter _boardController;
    private bool _isPlaying;
    private bool _isEnabled;

    private Card bufferedCard;
    private TurnManager _turnManager;
    private CancellationTokenSource _playCTS;

    public CardPlayService(GameBoardPresenter boardController, TurnManager turnManager) {
        _boardController = boardController;

        _turnManager = turnManager;
        _turnManager.OnTurnStart += EnablePlaying;
        _turnManager.OnTurnEnd += DisablePlaying;
    }

    private void EnablePlaying(Opponent enableOppoennt) {
        _isEnabled = true;
    }

    public void DisablePlaying(Opponent disableOpponent) {
        _isEnabled = false;
        CancelPlaying(disableOpponent);
    }

    private void CancelPlaying(Opponent opponentToStop) {
        // if current player is not active stop card playing
        if (_turnManager.ActiveOpponent != opponentToStop)
        _playCTS?.Cancel();
    }

    public void PlayCard(Opponent cardPlayer, Card cardToPlay) {
        BeginPlayCard(cardPlayer, cardToPlay).Forget();
    }

    private async UniTask BeginPlayCard(Opponent cardPlayer, Card card) {
        if (card == null) throw new ArgumentException("Card to play is null");
        if (!_isEnabled || _isPlaying || cardPlayer.Health.isDead) return;
        CardHand cardHand = cardPlayer.hand;

        _isPlaying = true;
        _playCTS?.Dispose();
        _playCTS = new CancellationTokenSource();

        try {
            bufferedCard = card;
            cardHand.SetInteraction(false);
            cardHand.DeselectCurrentCard();
            cardHand.RemoveCard(card);

            bool playResult = await card.PlayCard(
                cardPlayer,
                _boardController,
                _playCTS.Token // Додаємо токен
            );

            if (playResult) {
                cardPlayer.CardSpendable.TrySpend(card.Cost.CurrentValue);
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

    public void Dispose() {
        _playCTS?.Dispose();
    }
}
