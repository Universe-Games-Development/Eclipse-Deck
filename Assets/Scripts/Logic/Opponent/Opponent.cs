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
    public CardSpendable CardSpendable { get; private set; }
    public CardHand hand;
    public Deck deck;
    public Deck discardDeck;
    public CardCollection cardCollection;
    public Action<Opponent> OnDefeat { get; internal set; }
    protected GameEventBus _eventBus;
    private CommandManager _commandManager;
    [Inject(Optional = true)] private CardPlayService _cardPlayService;
    private CardProvider _cardProvider;
    public IActionFiller actionFiller;
    public OpponentData Data { get; private set; }

    [Inject]
    public Opponent(OpponentData opponentData, GameEventBus eventBus, CommandManager commandManager, CardProvider cardProvider) {
        Data = opponentData;
        _eventBus = eventBus;
        _commandManager = commandManager;
        _cardProvider = cardProvider;

        eventBus.SubscribeTo<BattleStartedEvent>(StartBattleActions);
        eventBus.SubscribeTo<BattleEndEventData>(EndBattleActions);
        deck = new Deck(this, _eventBus);
        discardDeck = new Deck(this, _eventBus);
        hand = new CardHand(this, _eventBus);
        hand.OnCardSelected += PlayCard;
        cardCollection = new CardCollection(_cardProvider);

        Stat healthStat = new(Data.Health);
        Stat manaStat = new(Data.Mana);
        Health = new Health(this, healthStat, _eventBus);
        Mana = new Mana(this, manaStat, _eventBus);
        CardSpendable = new CardSpendable(Mana, Health);
    }

    private void PlayCard(Card card) {
        _cardPlayService.PlayCard(this, card);
    }

    protected virtual void TurnStartActions(ref OnTurnStart eventData) {
        if (eventData.StartingOpponent == this)
        _commandManager.EnqueueCommand(new DrawCardCommand(this, 1));
    }

    private void EndBattleActions(ref BattleEndEventData eventData) {
        _eventBus.UnsubscribeFrom<OnTurnStart>(TurnStartActions);
        deck.ClearDeck();
        hand.ClearHand();
    }

    private void StartBattleActions(ref BattleStartedEvent eventData) {
        _eventBus.SubscribeTo<OnTurnStart>(TurnStartActions);

        _commandManager.EnqueueCommands(new List<Command> {
            new InitDeckCommand(this, 40, cardCollection, _eventBus).SetPriority(11),
            new DrawCardCommand(this, 10),
        });
    }

    public virtual void Dispose() {
        if (_eventBus != null) {
            _eventBus.UnsubscribeFrom<BattleStartedEvent>(StartBattleActions);
            _eventBus.UnsubscribeFrom<OnTurnStart>(TurnStartActions);
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
        Deck mainDeck = new(opponent, eventBus);
        cardCollection.GenerateTestCollection(20);
        mainDeck.Initialize(cardCollection);

        opponent.deck = mainDeck;
        opponent.discardDeck = new Deck(opponent, eventBus);
        await UniTask.CompletedTask;
    }

    public async override UniTask Undo() {
        opponent.deck = null;
        await UniTask.CompletedTask;
    }
}
