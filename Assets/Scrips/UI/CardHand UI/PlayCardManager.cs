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

    private async void OnCardSelected(Card selectedCard) {
        if (bufferedCard != null) return;

        Field fieldToSummon = null;
        Command playCommand = new PlayCardCommand(selectedCard, opponent, commandFiller);
        if (playCommand == null) {
            Debug.LogWarning("Selected card has no valid play command.");
            return;
        }
    }
}

public interface ICardsInputFiller {
    UniTask<T> RequestInput<T>(CardInputRequirement<T> requirement);
}

public class PlayerCommandFiller : ICardsInputFiller {
    private PlayCardUI playCardUI;

    public PlayerCommandFiller(PlayCardUI playCardUI) {
        this.playCardUI = playCardUI;
    }

    public UniTask<T> RequestInput<T>(CardInputRequirement<T> requirement) {
        throw new NotImplementedException();
    }
}

public class EnemyCommandFiller : ICardsInputFiller {

    public UniTask<T> RequestInput<T>(CardInputRequirement<T> requirement) {
        throw new NotImplementedException();
    }
}

public class PlayCardCommand : Command {
    private readonly ICardsInputFiller cardsInputFiller;
    private readonly Opponent opponent;
    private readonly Card selectedCard;
    private ResourceData spentResources;

    public PlayCardCommand(Card selectedCard, Opponent opponent, ICardsInputFiller cardsInputFiller) {
        this.selectedCard = selectedCard ?? throw new ArgumentNullException(nameof(selectedCard));
        this.opponent = opponent ?? throw new ArgumentNullException(nameof(opponent));
        this.cardsInputFiller = cardsInputFiller ?? throw new ArgumentNullException(nameof(cardsInputFiller));
    }

    public override async UniTask Execute() {
        Debug.Log($"Playing card: {selectedCard.Data.Name}");

        // 1. Check for sufficient resources
        spentResources = opponent.CardResource.TrySpend(selectedCard.Cost.CurrentValue);

        // If not enough resources, stop the execution immediately
        if (spentResources.Mana + spentResources.Health != selectedCard.Cost.CurrentValue) {
            Debug.Log("Not enough resources to play card");
            await Undo();
            return;
        }

        if (!opponent.Health.IsAlive()) {
            Debug.Log("Player died during card play");
            await Undo(); // одразу відкотити дію
            return; // зупинити виконання
        }

        opponent.hand.RemoveCard(selectedCard);

        bool result = await selectedCard.PlayCard(opponent, cardsInputFiller);

        if (!result) {
            await Undo();
        }
    }

    public override async UniTask Undo() {
        opponent.CardResource.Add(spentResources);
        opponent.hand.AddCard(selectedCard);
        await UniTask.CompletedTask;
    }
}
