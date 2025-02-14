using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class Opponent : IDisposable, IHasHealth, IAbilityOwner {

    public string Name = "Opponent";

    public Mana Mana { get; private set; }
    public Health Health { get; private set; }
    public AbilityManager AbilityManager { get; private set; }

    public CardResource CardResource { get; private set; }

    public CardHand hand;
    public Deck deck;
    public Deck discardDeck;

    public CardCollection cardCollection;
    public PlayCardManager playCardManager;

    public Action<Opponent> OnDefeat { get; internal set; }

    private readonly GameEventBus eventBus;
    private readonly CommandManager commandManager;

    public Opponent(GameEventBus eventBus, AssetLoader assetLoader, ICardsInputFiller commandFiller, CommandManager commandManager) {
        this.eventBus = eventBus;
        this.commandManager = commandManager;

        Health = new Health(this, 0, 20, eventBus);
        Mana = new Mana(this, 0, 10, eventBus);
        CardResource = new CardResource(Mana, Health);

        cardCollection = new CardCollection(assetLoader);
        hand = new CardHand(this, eventBus);
        playCardManager = new PlayCardManager(this, commandFiller);

        eventBus.SubscribeTo<OnBattleBegin>(StartBattleActions);
        eventBus.SubscribeTo<OnTurnStart>(TurnStartActions);
    }

    private void TurnStartActions(ref OnTurnStart eventData) {
        commandManager.EnqueueCommand(new DrawCardCommand(this, 1));
    }

    private void StartBattleActions(ref OnBattleBegin eventData) {
        commandManager.EnqueueCommands(new List<Command> {
            new InitDeckCommand(this, 40, cardCollection, eventBus),
            new DrawCardCommand(this, 3),
        });
    }

    public void Dispose() {
        if (eventBus != null) {
            eventBus.UnsubscribeFrom<OnBattleBegin>(StartBattleActions);
            eventBus.UnsubscribeFrom<OnTurnStart>(TurnStartActions);
        }

        GC.SuppressFinalize(this);
    }

    public AbilityManager GetAbilityManager() {
        return AbilityManager;
    }

    public Health GetHealth() {
        return Health;
    }
}


public class InitCardHandCommand : Command {
    private Opponent opponent;
    private GameEventBus eventBus;
    private ICardsInputFiller commandFiller;

    public InitCardHandCommand(Opponent opponent, GameEventBus eventBus, ICardsInputFiller commandFiller) {
        this.opponent = opponent;
        this.eventBus = eventBus;
        this.commandFiller = commandFiller;
    }

    public async override UniTask Execute() {
        CardHand initHand = opponent.hand;

        initHand = new CardHand(opponent, eventBus);
        opponent.playCardManager = new PlayCardManager(opponent, commandFiller);
        await UniTask.CompletedTask;
    }

    public async override UniTask Undo() {
        opponent.hand = null;
        opponent.playCardManager = null;
        await UniTask.CompletedTask;
    }
}

public class InitDeckCommand : Command {
    private Opponent opponent;
    private int deckSize;
    private CardCollection cardCollection;
    private GameEventBus eventBus;

    public InitDeckCommand(Opponent opponent, int amount, CardCollection cardCollection, GameEventBus eventBus) {
        this.opponent = opponent;
        this.deckSize = amount;
        this.cardCollection = cardCollection;
        this.eventBus = eventBus;
    }

    public async override UniTask Execute() {
        Deck mainDeck = new Deck(opponent, eventBus);
        await mainDeck.Initialize(cardCollection);

        opponent.deck = mainDeck;
        opponent.discardDeck = new Deck(opponent, eventBus);
        await UniTask.CompletedTask;
    }

    public async override UniTask Undo() {
        opponent.deck = null;
        await UniTask.CompletedTask;
    }
}
