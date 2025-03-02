using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Zenject;

public interface IMannable {
    Mana GetMana();
}

public class Opponent : IDisposable, IHealthEntity, IAbilityOwner, IMannable {

    public string Name = "Opponent";

    public Mana Mana { get; private set; }
    public Health Health { get; private set; }

    public CardResource CardResource { get; private set; }

    public CardHand hand;
    public Deck deck;
    public Deck discardDeck;

    public CardCollection cardCollection;
    public IActionFiller actionFiller;

    public Action<Opponent> OnDefeat { get; internal set; }
    private readonly GameEventBus eventBus;
    [Inject] private CommandManager _commandManager;
    [Inject] private CardPlayService _cardPlayService;

    public Opponent(GameEventBus eventBus, AssetLoader assetLoader, IActionFiller actionFiller) {
        this.eventBus = eventBus;
        this.actionFiller = actionFiller;

        Stat healthSat = new Stat(20, 20);
        Stat manaStat = new Stat(0, 10);
        Health = new Health(this, healthSat, eventBus);
        Mana = new Mana(this, manaStat, eventBus);
        CardResource = new CardResource(Mana, Health);

        cardCollection = new CardCollection(assetLoader);
        hand = new CardHand(this, eventBus);
        hand.OnCardSelected += PlayCard;

        eventBus.SubscribeTo<BattleStartedEvent>(StartBattleActions);
        eventBus.SubscribeTo<OnTurnStart>(TurnStartActions);
    }

    private void PlayCard(Card card) {
        _cardPlayService.PlayCard(this, card);
    }

    protected virtual void TurnStartActions(ref OnTurnStart eventData) {
        if (eventData.startTurnOpponent == this)
        _commandManager.EnqueueCommand(new DrawCardCommand(this, 1));
    }

    private void StartBattleActions(ref BattleStartedEvent eventData) {
        _commandManager.EnqueueCommands(new List<Command> {
            new InitDeckCommand(this, 40, cardCollection, eventBus),
            new DrawCardCommand(this, 10),
        });
    }

    public virtual void Dispose() {
        if (eventBus != null) {
            eventBus.UnsubscribeFrom<BattleStartedEvent>(StartBattleActions);
            eventBus.UnsubscribeFrom<OnTurnStart>(TurnStartActions);
        }

        GC.SuppressFinalize(this);
    }

    public Health GetHealth() {
        return Health;
    }

    public Mana GetMana() {
        return Mana;
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
