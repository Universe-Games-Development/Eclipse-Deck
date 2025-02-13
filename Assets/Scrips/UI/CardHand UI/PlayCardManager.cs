using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class PlayCardManager : IDisposable {
    private CardHand cardHand;
    private ICardsInputFiller commandFiller;
    [Inject] private CommandManager commandManager;

    private Card bufferedCard;

    public PlayCardManager(CardHand cardHand, ICardsInputFiller commandFiller) {
        this.cardHand = cardHand;
        this.commandFiller = commandFiller;

        cardHand.OnCardSelected += OnCardSelected;
    }

    public void Dispose() {
        cardHand.OnCardSelected -= OnCardSelected;
    }

    private async void OnCardSelected(Card selectedCard) {
        if (bufferedCard != null) return;

        Field fieldToSummon = null;
        Command playCommand = selectedCard.GetPlayCardCommand(fieldToSummon);
        if (playCommand == null) {
            Debug.LogWarning("Selected card has no valid play command.");
            return;
        }

        // temporary removes card from hand
        bufferedCard = selectedCard;
        cardHand.RemoveCard(selectedCard);

        bool isValid;
        try {
            isValid = await commandFiller.FillCardnputs(selectedCard);
        } catch (Exception ex) {
            Debug.LogError("Error occurred during input filling: " + ex.Message);
            isValid = false;
        }

        if (isValid) {
            commandManager.EnqueueCommand(playCommand);
        } else {
            cardHand.AddCard(bufferedCard);
        }

        bufferedCard = null; // Clearing buffer
    }
}

public interface ICardsInputFiller {
    Task<bool> FillCardnputs(Card card);
}

public class PlayerCommandFiller : ICardsInputFiller {
    private PlayCardUI playCardUI;

    public PlayerCommandFiller(PlayCardUI playCardUI) {
        this.playCardUI = playCardUI;
    }

    public async Task<bool> FillCardnputs(Card card) {
        return await playCardUI.FillInputs(card);
    }
}

public class EnemyCommandFiller : ICardsInputFiller {
    public async Task<bool> FillCardnputs(Card card) {
        // Soon : Algoritm to fill data by AI
        await Task.Delay(500); // Thinking simulation
        return true;
    }
}
