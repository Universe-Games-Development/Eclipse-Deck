using System;

public class CreaturePresenter : UnitPresenter {
    public Creature Creature;
    public Card3DView View;

    public void Initialize(Creature creature, CardView view) {
        Creature = creature;
        View = (Card3DView)view;
        UpdateUI();
    }

    private void UpdateUI() {
        var data = Creature.SourceCard.CreatureCardData;

        // Простий спосіб через готовий конфіг
        var displayData = ConvertToDisplayData(Creature.SourceCard);
        CardDisplayContext context = new(displayData, CardDisplayConfig.ForCreature());
        View.UpdateDisplay(context);
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

    #region UnitPresenter API
    public override UnitModel GetModel() {
        return Creature;
    }
    public override BoardPlayer GetPlayer() {
        return Creature.GetPlayer();
    }
    #endregion

    public void Reset() {
        // Do reset logic here
    }
}