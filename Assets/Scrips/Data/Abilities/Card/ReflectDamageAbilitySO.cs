using System;
using UnityEngine;

public enum ReflectMode {
    Percentage,
    KillAttacker
}

[CreateAssetMenu(fileName = "ReflectDamage", menuName = "Abilities/CardAbilities")]
public class ReflectDamageAbilitySO : AbilitySO {
    public ReflectMode reflectMode = ReflectMode.Percentage;
    [Range(0, 1)] public float damagePercentage = 0.5f;

    public override Ability GenerateAbility(IAbilityOwner owner, GameEventBus eventBus) {
        return new ReflectDamageAbility(this, owner, reflectMode, eventBus, damagePercentage);
    }
}

public class ReflectDamageAbility : Ability {
    private ReflectMode reflectMode = ReflectMode.Percentage;
    private float percentageDamage;

    public ReflectDamageAbility(AbilitySO abilitySO, IAbilityOwner owner, ReflectMode reflectMode, GameEventBus eventBus, float percentage = 0) : base(abilitySO, owner, eventBus) {

        this.reflectMode = reflectMode;
        if (reflectMode == ReflectMode.Percentage) {
            percentageDamage = percentage;
        }
    }

    private void ReflectDamage(int damage, IDamageDealer damageDealer) {
        // Cast attacker to damagable to damage him
        if (!(damageDealer is IHasHealth healthableSource)) {
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

    public override void Register() {
        if (abilityOwner is IHasHealth healthProvider) {
            Health health = healthProvider.GetHealth();
            health.OnDamageTaken += ReflectDamage;
            IsActive = true;
        }
    }

    public override void Deregister() {
        if (abilityOwner is IHasHealth healthProvider) {
            Health health = healthProvider.GetHealth();
            health.OnDamageTaken -= ReflectDamage;
            IsActive = true;
        }
    }
}
