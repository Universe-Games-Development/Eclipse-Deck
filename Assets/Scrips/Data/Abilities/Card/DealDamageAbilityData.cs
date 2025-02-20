using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public enum ReflectMode {
    Percentage,
    KillAttacker
}

[CreateAssetMenu(fileName = "Summon Creature Ability", menuName = "Abilities/CardAbilities/Summon")]
public class DealDamageAbilityData : CardAbilityData {
    public int damage = 1;
    // IAbilitiesCaster is CreatureCard or Opponent
    public override CardAbility GenerateAbility(Card spellCard, GameEventBus eventBus) {
        if (!(spellCard is SpellCard spell)) throw new ArgumentException("Owner must be a CreatureCard");
        return new FireballCardAbility(spell, this, eventBus);
    }
}

public class FireballCardAbility : ActiveCardAbility {
    private IRequirement<Creature> enemyCreatureRequirement;
    private DealDamageAbilityData abilityData;
    private SpellCard spellCard;

    public FireballCardAbility(SpellCard spellCard, DealDamageAbilityData abilityData, GameEventBus eventBus) : base(abilityData, eventBus) {
        this.abilityData = abilityData;
        this.spellCard = spellCard;
        RequirementBuilder<Creature> requirementBuilder = new();
        enemyCreatureRequirement = requirementBuilder.Add(new EnemyCreatureRequirement(spellCard.Owner)).Build();
    }

    public override async UniTask<bool> PerformAbility(IAbilityInputter filler) {
        Creature enemyCreature = await filler.ProcessRequirementAsync(spellCard.Owner, enemyCreatureRequirement);
        if (enemyCreature == null) return false;

        enemyCreature.GetHealth().TakeDamage(abilityData.damage);
        return true;
    }
}
