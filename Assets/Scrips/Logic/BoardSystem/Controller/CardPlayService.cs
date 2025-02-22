using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayManagerRegistrator : IDisposable {
    private readonly Dictionary<Opponent, CardPlayService> cardPlayServices = new();
    private GameBoardController boardController; // Используется для розыгрыша карт

    [Inject]
    private void Construct(GameBoardController boardController) {
        this.boardController = boardController ?? throw new ArgumentNullException(nameof(boardController));
    }

    public void EnablePlayCardServices(List<Opponent> opponents) {
        foreach (var opponent in opponents) {
            if (!cardPlayServices.ContainsKey(opponent)) {
                var service = new CardPlayService(opponent, boardController);
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
    
    public CardPlayService(Opponent opponent, GameBoardController boardController) {
        this.boardController = boardController ?? throw new ArgumentNullException(nameof(boardController));
        this.opponent = opponent ?? throw new ArgumentNullException(nameof(opponent));
        cardHand = opponent.hand ?? throw new ArgumentNullException(nameof(opponent.hand));

        cardHand.OnCardSelected += OnCardSelected;
    }

    public void Dispose() {
        cardHand.OnCardSelected -= OnCardSelected;
    }

    private void OnCardSelected(Card selectedCard) {
        if (selectedCard == null || _isPlaying) return;

        BeginPlayCard(selectedCard).Forget();
    }

    private async UniTask BeginPlayCard(Card card) {
        _isPlaying = true;
        try {
            bufferedCard = card;

            cardHand.DeselectCurrentCard();
            cardHand.RemoveCard(card);

            bool playResult = await card.PlayCard(opponent, boardController);

            if (playResult) {
                Debug.Log("Card playing successful");
            } else {
                // Если розыгрыш не удался, возвращаем карту обратно в руку
                cardHand.AddCard(bufferedCard);
                Debug.LogWarning("Card playing canceled");
            }
        } catch (Exception ex) {
            Debug.LogError($"Error while playing card: {ex.Message}");
            cardHand.AddCard(bufferedCard);
        } finally {
            _isPlaying = false;
            bufferedCard = null;
        }
    }
}
