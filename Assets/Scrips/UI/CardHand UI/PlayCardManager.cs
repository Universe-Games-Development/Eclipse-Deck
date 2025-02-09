using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class PlayCardManager : IDisposable {
    private CardHand cardHand;
    private ICommandFiller commandFiller;
    [Inject] private CommandManager commandManager;

    private Card bufferedCard;

    public PlayCardManager(CardHand cardHand, ICommandFiller commandFiller) {
        this.cardHand = cardHand;
        this.commandFiller = commandFiller;

        cardHand.OnCardSelected += OnCardSelected;
    }

    public void Dispose() {
        cardHand.OnCardSelected -= OnCardSelected;
    }

    private async void OnCardSelected(Card selectedCard) {
        if (bufferedCard != null) return;

        IInputCommand playCommand = selectedCard.GetPlayCardCommand();
        if (playCommand == null) {
            Debug.LogWarning("Selected card has no valid play command.");
            return;
        }

        // temporary removes card from hand
        bufferedCard = selectedCard;
        cardHand.RemoveCard(selectedCard);

        bool isValid;
        try {
            isValid = await commandFiller.FillInputs(playCommand);
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

public interface ICommandFiller {
    Task<bool> FillInputs(IInputCommand command);
}

public class PlayerCommandFiller : ICommandFiller {
    private PlayCardUI playCardUI;

    public PlayerCommandFiller(PlayCardUI playCardUI) {
        this.playCardUI = playCardUI;
    }

    public async Task<bool> FillInputs(IInputCommand command) {
        return await playCardUI.FillInputs(command);
    }
}

public class EnemyCommandFiller : ICommandFiller {
    public async Task<bool> FillInputs(IInputCommand command) {
        // Soon : Algoritm to fill data by AI
        await Task.Delay(500); // Thinking simulation
        return true;
    }
}
