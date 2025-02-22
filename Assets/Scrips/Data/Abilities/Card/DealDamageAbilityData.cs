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
    public override Ability<CardAbilityData, Card> CreateAbility(Card abilityCard, GameEventBus eventBus) {
        return new FireballCardAbility(abilityCard, this, eventBus);
    }
}

public class FireballCardAbility : CardActiveAbility {
    private IRequirement<Creature> enemyCreatureRequirement;
    private DealDamageAbilityData abilityData;
    private SpellCard abilityCard;

    public FireballCardAbility(Card abilityCard, DealDamageAbilityData abilityData, GameEventBus eventBus) : base(abilityData, abilityCard, eventBus) {
        RequirementBuilder<Creature> requirementBuilder = new();
        enemyCreatureRequirement = requirementBuilder.Add(new EnemyCreatureRequirement(abilityCard.Owner)).Build();
    }

    public override async UniTask<bool> PerformAbility(IActionFiller filler) {
        Creature enemyCreature = await filler.ProcessRequirementAsync(abilityCard.Owner, enemyCreatureRequirement);
        if (enemyCreature == null) return false;

        enemyCreature.GetHealth().TakeDamage(abilityData.damage);
        return true;
    }
}
