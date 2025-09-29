using System;

public class CreaturePresenter : UnitPresenter, IDisposable {
    public Creature Creature;
    public CreatureView CreatureView;

    public CreaturePresenter (Creature creature, CreatureView view) : base(creature, view) {
        Creature = creature;
        CreatureView = view;
        UpdateUI();

    }

    private void UpdateUI() {
        // Простий спосіб через готовий конфіг
        var displayData = ConvertToDisplayData(Creature.SourceCard);
        CardDisplayContext context = new(displayData, CardDisplayConfig.ForCreature());
        CreatureView.UpdateDisplay(context);
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


    public void Reset() {
        // Do reset logic here
    }

    public void Dispose() {
    }
}