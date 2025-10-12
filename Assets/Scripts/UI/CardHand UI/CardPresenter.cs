public class CardPresenter : UnitPresenter {
    public Card Card { get; private set; }
    public CardView CardView { get; private set; }

    public CardPresenter (Card card, CardView cardView) : base(card, cardView) {
        this.Card = card;
        CardView = cardView;

        UpdateUIInfo();
    }

    private void UpdateUIInfo() {
        CardDisplayData cardDisplayData = ConvertToDisplayData(Card);
        CardDisplayConfig cardDisplayConfig = CardDisplayConfig.ForHandCard();
        if (!(Card is CreatureCard card)) {
            cardDisplayConfig.showStats = false;
        }
        CardDisplayContext context = new(cardDisplayData, cardDisplayConfig);
        CardView.UpdateDisplay(context);
    }

    private CardDisplayData ConvertToDisplayData(Card card) {
        int attack = 0;
        int health = 0;
        if (card is CreatureCard creature) {
            attack = creature.Attack.Current;
            health = creature.Health.Current;
        }

        return new CardDisplayData {
            name = card.Data.Name,
            cost = card.Cost.Current,
            attack = attack,
            health = health,
            portrait = card.Data.Portait,
            background = card.Data.Background,
            rarity = RarityUtility.GetRarityColor(card.Data.Rarity)
        };
    }
}

