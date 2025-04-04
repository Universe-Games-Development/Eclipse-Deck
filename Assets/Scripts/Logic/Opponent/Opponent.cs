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
    public Action<Opponent> OnDefeat { get; internal set; }
    protected GameEventBus _eventBus;
    private CommandManager _commandManager;
    [Inject(Optional = true)] private CardPlayService _cardPlayService;
    private CardProvider _cardProvider;
    public IActionFiller actionFiller;
    public OpponentData Data { get; private set; }

    public Opponent(GameEventBus eventBus, CommandManager commandManager, CardProvider cardProvider) {
        
        _eventBus = eventBus;
        _commandManager = commandManager;
        _cardProvider = cardProvider;

        eventBus.SubscribeTo<BattleStartedEvent>(StartBattleActions);
        eventBus.SubscribeTo<BattleEndEventData>(EndBattleActions);
        deck = new Deck(this, _eventBus);
        discardDeck = new Deck(this, _eventBus);
        hand = new CardHand(this, _eventBus);
        hand.OnCardSelected += PlayCard;
    }

    public void SetData(OpponentData data) {
        Data = data;
        Name = Data.Name;
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
            new InitDeckCommand(this, 40, _cardProvider, _eventBus).SetPriority(11),
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
    private Opponent _opponent;
    private int _deckSize;
    private CardCollection _cardCollection;
    private CardProvider _cardProvider;
    private GameEventBus _eventBus;
    private int cardAmount = 20;

    public InitDeckCommand(Opponent opponent, int amount, CardProvider cardProvider, GameEventBus eventBus) {
        _opponent = opponent;
        _deckSize = amount;
        _cardProvider = cardProvider;
        _eventBus = eventBus;
    }

    public async override UniTask Execute() {
        Deck mainDeck = new(_opponent, _eventBus);
        
        _cardCollection = await GenerateRandomCollection(); 
        mainDeck.Initialize(_cardCollection);

        _opponent.deck = mainDeck;
        _opponent.discardDeck = new Deck(_opponent, _eventBus);
        await UniTask.CompletedTask;
    }

    public async UniTask<CardCollection> GenerateRandomCollection() {
        CardCollection collection = new();
        List<CardData> _unclokedCards = await _cardProvider.GetRandomUnlockedCards(cardAmount);
        foreach (var card in _unclokedCards) {
            collection.AddCardToCollection(card);
        }
        return collection;
    }

    public async override UniTask Undo() {
        _opponent.deck = null;
        await UniTask.CompletedTask;
    }
}
