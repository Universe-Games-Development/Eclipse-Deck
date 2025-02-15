using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

public class PlayCardManager : IDisposable {
    private Opponent opponent;

    private CardHand cardHand;
    private ICardsInputFiller commandFiller;
    [Inject] private CommandManager commandManager;

    private Card bufferedCard;

    public PlayCardManager(Opponent opponent, ICardsInputFiller commandFiller) {
        this.commandFiller = commandFiller;
        this.opponent = opponent;
        cardHand = opponent.hand;
        cardHand.OnCardSelected += OnCardSelected;
    }

    public void Dispose() {
        cardHand.OnCardSelected -= OnCardSelected;
    }

    private void OnCardSelected(Card selectedCard) {
        if (bufferedCard != null) return;
        TryPlayCard(selectedCard).Forget();
    }

    private async UniTask TryPlayCard(Card card) {
        // Intent = card.GetIntent();

        cardHand.DeselectCurrentCard();
        bool result = await card.PlayCard(opponent, commandFiller);
        
        if (result) {
            bufferedCard = card;
            cardHand.RemoveCard(card);
        } else {
            cardHand.AddCard(bufferedCard);
            Debug.LogWarning("Selected card has no valid play.");
        }
    }
}
