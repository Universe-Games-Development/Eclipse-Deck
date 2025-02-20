using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ReflectDamage", menuName = "Abilities/CreatureAbilities")]
public class ReflectDamageAbilityData : CreatureAbilityData {
    public ReflectMode reflectMode = ReflectMode.Percentage;
    [Range(0, 1)] public float damagePercentage = 0.5f;

    public override Ability<CreatureAbilityData, Creature> CreateAbility(Creature owner, GameEventBus eventBus) {
        if (!(owner is Creature creature)) throw new ArgumentException("Owner must be a CreatureCard");
        return new ReflectDamageAbility(creature, this, eventBus);
    }
}

public class ReflectDamageAbility : CreaturePassiveAbility {
    private ReflectMode reflectMode = ReflectMode.Percentage;
    private float percentageDamage;
    protected Creature creature;
    public ReflectDamageAbilityData ReflectAbilityData { get; set; }

    public ReflectDamageAbility(Creature creature, ReflectDamageAbilityData abilityData, GameEventBus eventBus) : base(abilityData, creature, eventBus) {
        // Dara initialization
        reflectMode = abilityData.reflectMode;
        if (reflectMode == ReflectMode.Percentage) {
            percentageDamage = abilityData.damagePercentage;
        }
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
    protected override void ActivateAbilityTriggers() {
        Health health = creature.GetHealth();
        health.OnDamageTaken += ReflectDamage;
    }

    protected override void DeactivateAbilityTriggers() {
        Health health = creature.GetHealth();
        health.OnDamageTaken -= ReflectDamage;
    }

    protected override bool CheckActivationConditions() {
        return abilityActivationRequirement.IsMet(creature, out string error);
    }
}
