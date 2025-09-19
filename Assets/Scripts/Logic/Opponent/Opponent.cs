using System;
using Zenject;


public class Opponent : UnitModel, IHealthable, IMannable, IDisposable {
    public Action<Opponent> OnDefeat { get; internal set; }
    public Health Health { get; private set; }
    public Mana Mana { get; private set; }
    public CharacterData Data { get; private set; }

    public CardSpendable CardSpendable { get; private set; }
    public Deck Deck { get; private set; }
    public CardHand Hand { get; private set; }

    public Opponent(CharacterData data) {
        Data = data;

        Health = new Health(Data.Health, this);
        Mana = new Mana(this, Data.Mana);
        CardSpendable = new CardSpendable(Mana, Health);

        Deck = new();
        Hand = new();
        Hand.ChangeOwner(this);
    }

    public void SpendMana(int currentValue) {
        int was = Mana.Current;
        Mana.Subtract(currentValue);
        //Debug.Log($"Mana: {Mana.Current} / {Mana.Max}");
    }

    public virtual void Dispose() {
        GC.SuppressFinalize(this);
    }
}

public class Player : Opponent {

    public PlayerData PlayerData => (PlayerData)base.Data;

    public Player(PlayerData data) : base(data) {

    }
}


public class Enemy : Opponent {
    private Speaker speech;
    [Inject] private TurnManager _turnManager;
    [Inject] protected OpponentRegistrator opponentRegistrator;

    public Enemy(CharacterData opponentData, DialogueSystem dialogueSystem, IEventBus<IEvent> eventBus)
        : base(opponentData) {
        SpeechData speechData = opponentData.speechData;
        if (speechData != null) {
            speech = new Speaker(speechData, this, dialogueSystem, eventBus);
        }
    }
}