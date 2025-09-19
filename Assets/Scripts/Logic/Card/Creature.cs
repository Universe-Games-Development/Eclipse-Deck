using System;

public class Creature : UnitModel, IHealthable {
    public Health Health { get; private set; }
    public Attack Attack { get; private set; }
    public CreatureCardData Data { get; private set; }

    private CreatureCard sourceCard;
    private Zone currentZone;

    public CreatureCard SourceCard => sourceCard;
    public Zone CurrentZone => currentZone;

    public Creature(CreatureCard card) {
        Data = card.CreatureCardData ?? throw new ArgumentNullException(nameof(card.CreatureCardData));
        Health = new Health(card.Health, this);
        Attack = new Attack(card.Attack, this);
        sourceCard = card;

        Id = System.Guid.NewGuid().ToString();
    }


    public void SetZone(Zone zone) {
        currentZone = zone;
    }

    public bool CanAttack() {
        // Логіка перевірки чи може істота атакувати
        return Health.Current > 0 && Attack.Current > 0;
    }

    public override string ToString() {
        return $"{Data.Name} ({Attack.Current}/{Health.Current})";
    }

    public override string GetName() {
        return Data.Name;
    }
}

