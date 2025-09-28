using Zenject;

public interface IOpponentFactory {
    Opponent CreateOpponent(OpponentData data);
    Opponent CreatePlayer(OpponentData data);
}

public class OpponentFactory : IOpponentFactory {
    [Inject] DiContainer container;

    public Opponent CreateOpponent(OpponentData data) {
        var deck = container.Instantiate<Deck>();
        var hand = container.Instantiate<CardHand>();
        
        return container.Instantiate<Opponent>(new object[] { data, deck, hand});
    }

    public Opponent CreatePlayer(OpponentData data) {
        var deck = container.Instantiate<Deck>();
        var hand = container.Instantiate<CardHand>();
        return container.Instantiate<Opponent>(new object[] { data, deck, hand});
    }
}

