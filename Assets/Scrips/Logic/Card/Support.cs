using Cysharp.Threading.Tasks;

internal class Support : Card {
    private SupportCard card;
    private Opponent summoner;
    private GameEventBus eventBus;

    public Support(CardSO cardSO, Opponent owner, GameEventBus eventBus) : base(cardSO, owner, eventBus) {
    }

    public override UniTask<bool> PlayCard(Opponent cardPlayer, ICardsInputFiller cardsInputFiller) {
        throw new System.NotImplementedException();
    }
}