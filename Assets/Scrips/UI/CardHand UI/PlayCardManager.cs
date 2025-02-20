using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

public class PlayCardManager : IDisposable {
    private Opponent opponent;

    private CardHand cardHand;
    private IAbilityInputter abilityInputter;
    [Inject] private CommandManager commandManager;

    private Card bufferedCard;

    public PlayCardManager(Opponent opponent, IAbilityInputter abilityInputter) {
        this.abilityInputter = abilityInputter;
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
        bufferedCard = card;
        cardHand.RemoveCard(card);

        bool result = await card.PlayCard(opponent, abilityInputter);
        
        if (result) {
            Debug.LogWarning("Card playing successsful");
        } else {
            cardHand.AddCard(bufferedCard);
            Debug.LogWarning("Card playing canceled");
        }
        bufferedCard = null;
    }
}
