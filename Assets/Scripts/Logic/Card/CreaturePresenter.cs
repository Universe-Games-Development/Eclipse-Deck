using System;

public class CreaturePresenter : UnitPresenter {
    public Creature Creature;
    public CreatureView CreatureView;

    public CreaturePresenter (Creature creature, CreatureView view) : base(creature, view) {
        Creature = creature;
        CreatureView = view;
        UpdateUI();

    }

    private void UpdateUI() {
        // Простий спосіб через готовий конфіг
        var displayData = ConvertToDisplayData(Creature);
        CardDisplayContext context = new(displayData, CardDisplayConfig.ForCreature());
        CreatureView.UpdateDisplay(context);
    }

    private CardDisplayData ConvertToDisplayData(Creature creature) {
        int attack = 0;
        int health = 0;

        attack = creature.Attack.Current;
        health = creature.Health.Current;

        return new CardDisplayData {
            name = creature.Data.Name,
            cost = creature.Cost.Current,
            attack = attack,
            health = health,
            portrait = creature.Data.Portait,
            background = creature.Data.Background,
            rarity = RarityUtility.GetRarityColor(creature.Data.Rarity)
        };
    }

}