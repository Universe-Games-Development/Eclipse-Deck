using System;
using UnityEngine;

public enum ReflectMode {
    Percentage,
    KillAttacker
}

[CreateAssetMenu(fileName = "ReflectDamage", menuName = "Abilities/CardAbilities")]
public class ReflectDamageAbilitySO : EntityAbilityData {
    public ReflectMode reflectMode = ReflectMode.Percentage;
    [Range(0, 1)] public float damagePercentage = 0.5f;

    public override Ability GenerateAbility(IAbilitiesCaster owner, GameEventBus eventBus) {

        return new ReflectDamageAbility(this, owner, reflectMode, eventBus, damagePercentage);
    }
}

public class ReflectDamageAbility : EntityPassiveAbility {
    private ReflectMode reflectMode = ReflectMode.Percentage;
    private float percentageDamage;

    public ReflectDamageAbility(EntityAbilityData abilitySO, IAbilitiesCaster entity, ReflectMode reflectMode, GameEventBus eventBus, float percentage = 0) : base(abilitySO, entity, eventBus) {

        this.reflectMode = reflectMode;
        if (reflectMode == ReflectMode.Percentage) {
            percentageDamage = percentage;
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
                attckSourceHealth.ApplyDamage(reflectedDamage);
                break;
            case ReflectMode.KillAttacker:
                int fullHpDamage = attckSourceHealth.MaxValue;
                attckSourceHealth.ApplyDamage(fullHpDamage);
                break;
        }
    }

    public override void RegisterTrigger() {
        Health health = Entity.GetHealth();
        health.OnDamageTaken += ReflectDamage;
    }

    public override void DeregisterTrigger() {
        Health health = Entity.GetHealth();
        health.OnDamageTaken -= ReflectDamage;
    }
}
