using System;
using UnityEngine;
using static CardAbility;

[CreateAssetMenu(fileName = "ReflectDamage", menuName = "Abilities/CreatureAbilities")]
public class ReflectDamageAbilityData : CreatureAbilityData {
    public ReflectMode reflectMode = ReflectMode.Percentage;
    [Range(0, 1)] public float damagePercentage = 0.5f;

    public override CreatureAbility GenerateAbility(Creature owner, GameEventBus eventBus) {
        if (!(owner is Creature creature)) throw new ArgumentException("Owner must be a CreatureCard");
        return new ReflectDamageAbility(creature, this, eventBus);
    }
}

public class ReflectDamageAbility : PassiveCreatureAbility {
    private ReflectMode reflectMode = ReflectMode.Percentage;
    private float percentageDamage;
    protected Creature creature;
    private IRequirement<Creature> isCreatureAliveReq;
    public ReflectDamageAbilityData ReflectAbilityData { get; set; }

    public ReflectDamageAbility(Creature creature, ReflectDamageAbilityData abilityData, GameEventBus eventBus) : base(creature, abilityData, eventBus) {
        // Dara initialization
        this.creature = creature;
        reflectMode = abilityData.reflectMode;
        if (reflectMode == ReflectMode.Percentage) {
            percentageDamage = abilityData.damagePercentage;
        }

        //Passive Requirements
        isCreatureAliveReq = new RequirementBuilder<Creature>()
            .And(new CreatureAliveRequirement())
            .Build();
    }

    private void ReflectDamage(int damage, IDamageDealer damageDealer) {
        // Cast attacker to damagable to damage him
        if (!(damageDealer is IHealthEntity healthableSource)) {
            return;
        }

        Health attckSourceHealth = healthableSource.GetHealth();

        switch (reflectMode) {
            case ReflectMode.Percentage:
                int reflectedDamage = Mathf.CeilToInt(damage * percentageDamage);
                attckSourceHealth.TakeDamage(reflectedDamage);
                break;
            case ReflectMode.KillAttacker:
                int fullHpDamage = attckSourceHealth.Max;
                attckSourceHealth.TakeDamage(fullHpDamage);
                break;
        }
    }
    public override void RegisterTrigger() {
        Health health = creature.GetHealth();
        health.OnDamageTaken += ReflectDamage;
    }

    public override void DeregisterTrigger() {
        Health health = creature.GetHealth();
        health.OnDamageTaken -= ReflectDamage;
    }

    public override bool ActivationCondition() {
        return isCreatureAliveReq.IsMet(creature, out string error);
    }
}
