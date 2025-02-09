using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class Opponent : IEventListener {
    public Action<CardHand> OnHandInitialized;

    public string Name = "Opponent";
    public Health health;

    public CardHand hand;
    public Deck deck;
    public Deck discardDeck;

    public CardCollection cardCollection;
    private IEventQueue eventQueue;

    public PlayCardManager playCardManager;
    public ICommandFiller commandFiller;

    public Action<Opponent> OnDefeat { get; internal set; }

    public Opponent(IEventQueue eventQueue, AssetLoader assetLoader, ICommandFiller commandFiller, CommandManager commandManager) {
        this.eventQueue = eventQueue;

        eventQueue.RegisterListener(this, EventType.BATTLE_START);
        eventQueue.RegisterListener(this, EventType.ON_TURN_START);

        health = new Health(0, 20);
        cardCollection = new CardCollection(assetLoader);

        hand = new CardHand(this, eventQueue);
        playCardManager = new PlayCardManager(hand, commandFiller);
    }

    // Object will be List<IComand> or single ICommand
    public object OnEventReceived(object data) {
        return data switch {
            BattleStartEventData battleStartEventData => new List<ICommand> {
            new InitDeckCommand(this, 40, cardCollection, eventQueue),
            new DrawCardCommand(this, 3),
            //new InitCardHandCommand(this, eventQueue, commandFiller),
        },
        TurnChangeEventData turnChangeEventData => new List<ICommand> {
            new DrawCardCommand(this, 1),
        },
            _ => null
        };
    }

}

public class InitCardHandCommand : ICommand {
    private Opponent opponent;
    private IEventQueue eventQueue;
    private ICommandFiller commandFiller;

    public InitCardHandCommand(Opponent opponent, IEventQueue eventQueue, ICommandFiller commandFiller) {
        this.opponent = opponent;
        this.eventQueue = eventQueue;
        this.commandFiller = commandFiller;
    }

    public async UniTask Execute() {
        CardHand initHand = opponent.hand;

        initHand = new CardHand(opponent, eventQueue);
        opponent.playCardManager = new PlayCardManager(initHand, commandFiller);
        await UniTask.CompletedTask;
    }

    public async UniTask Undo() {
        opponent.hand = null;
        opponent.playCardManager = null;
        await UniTask.CompletedTask;
    }
}

public class InitDeckCommand : ICommand {
    private Opponent opponent;
    private int deckSize;
    private CardCollection cardCollection;
    private IEventQueue eventQueue;

    public InitDeckCommand(Opponent opponent, int amount, CardCollection cardCollection, IEventQueue eventQueue) {
        this.opponent = opponent;
        this.deckSize = amount;
        this.cardCollection = cardCollection;
        this.eventQueue = eventQueue;
    }

    public async UniTask Execute() {
        Deck mainDeck = new Deck(opponent, eventQueue);
        await mainDeck.Initialize(cardCollection);

        opponent.deck = mainDeck;
        opponent.discardDeck = new Deck(opponent, eventQueue);
        await UniTask.CompletedTask;
    }

    public async UniTask Undo() {
        opponent.deck = null;
        await UniTask.CompletedTask;
    }
}