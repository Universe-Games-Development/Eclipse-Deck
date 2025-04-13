using System;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "ReflectDamage", menuName = "Abilities/CreatureAbilities")]
public class ReflectDamageAbilityData : CreatureAbilityData {
    public ReflectMode reflectMode = ReflectMode.Percentage;
    [Range(0, 1)] public float damagePercentage = 0.5f;

    public override Ability<CreatureAbilityData, Creature> CreateAbility(Creature owner, DiContainer container) {
        return container.Instantiate<ReflectDamageAbility>(new object[] { this, owner });
    }
}

public class ReflectDamageAbility : CreaturePassiveAbility {
    private ReflectMode reflectMode = ReflectMode.Percentage;
    private float percentageDamage;
    protected Creature creature;

    public ReflectDamageAbility(ReflectDamageAbilityData data, Creature owner) : base(data, owner) {
        reflectMode = data.reflectMode;
        if (reflectMode == ReflectMode.Percentage) {
            percentageDamage = data.damagePercentage;
        }
    }

    public ReflectDamageAbilityData ReflectAbilityData { get; set; }

    private void ReflectDamage(int damage, IDamageDealer damageDealer) {
        // Cast attacker to damagable to damage him
        if (!(damageDealer is IHealthEntity healthableSource)) {
            return;
        }

        IHealth attckSourceHealth = healthableSource.Health;

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
        IHealth health = creature.Health;
        health.OnDamageTaken += ReflectDamage;
    }

    protected override void DeactivateAbilityTriggers() {
        IHealth health = creature.Health;
        health.OnDamageTaken -= ReflectDamage;
    }
}

