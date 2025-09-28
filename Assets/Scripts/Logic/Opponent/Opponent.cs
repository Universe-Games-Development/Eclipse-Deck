using System;

public class Opponent : UnitModel, IHealthable, IMannable {
    public override string OwnerId {
        get { return Id; }
    }

    public Action<Opponent> OnDefeat { get; internal set; }
    public Health Health { get; private set; }
    public Mana Mana { get; private set; }
    public OpponentData Data { get; private set; }

    public CardSpendable CardSpendable { get; private set; }
    public Deck Deck { get; private set; }
    public CardHand Hand { get; private set; }

    public Opponent(OpponentData data, Deck deck, CardHand hand) {
        Data = data;
        Id = $"{Data.Name}_{Guid.NewGuid()}";

        Health = new Health(data.Health);
        Mana = new Mana(data.Mana);
        CardSpendable = new CardSpendable(Mana, Health);

        Deck = deck;
        Hand = hand;
        Deck.ChangeOwner(Id);
        Hand.ChangeOwner(Id);

    }

    public void SpendMana(int currentValue) {
        int was = Mana.Current;
        Mana.Subtract(currentValue);
        //Debug.Log($"Mana: {Mana.Current} / {Mana.Max}");
    }
}

public struct OnDamageTaken : IEvent {
    public IDamageDealer Source { get; }
    public IHealthable Target { get; }
    public int Amount { get; }

    public OnDamageTaken(IHealthable target, IDamageDealer source, int amount) {
        Source = source;
        Target = target;
        Amount = amount;
    }
}

public struct DeathEvent : IEvent {
    public IHealthable DeadEntity { get; }

    public DeathEvent(IHealthable deadEntity) {
        DeadEntity = deadEntity;
    }
}
