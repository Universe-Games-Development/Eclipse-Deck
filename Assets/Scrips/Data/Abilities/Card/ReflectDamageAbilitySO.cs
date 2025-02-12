using System;
using Unity.VisualScripting;
using UnityEngine;
using static ReflectDamageAbilitySO;

[CreateAssetMenu(fileName = "ReflectDamage", menuName = "Abilities/CardAbilities")]
public class ReflectDamageAbilitySO : CreatureAbilitySO {
    public enum ReflectMode {
        Percentage,
        KillAttacker
    }

    [Header("Reflect Damage Settings")]
    public ReflectMode reflectMode = ReflectMode.Percentage;
    [Range(0, 1)] public float damagePercentage = 0.5f;

    public ReflectDamageAbility GenerateAbility(IAbilitySource owner, GameEventBus eventBus) {
        return new ReflectDamageAbility(this, owner, eventBus, reflectMode, damagePercentage);
    }
}

public class ReflectDamageAbility : CreatureAbility {
    private ReflectMode reflectMode;
    private float percentageDamage;

    public ReflectDamageAbility(CreatureAbilitySO creatureAbilitySO, IAbilitySource owner, GameEventBus eventBus, ReflectMode reflectMode, float percentage = 0) : base(creatureAbilitySO, owner, eventBus) {
        eventBus.SubscribeTo<OnDamageTaken>(ReflectDamage);
        this.reflectMode = reflectMode;
        if (reflectMode == ReflectMode.Percentage) {
            percentageDamage = percentage;
        }
    }

    private void ReflectDamage(ref OnDamageTaken eventData) {
        // Cast attacker to damagable to damage him
        IDamageable damagableSourse = eventData.Source as IDamageable;
        if (damagableSourse == null) {
            return;
        }

        switch (reflectMode) {
            case ReflectMode.Percentage:
                int damageAmount = eventData.Amount;
                int reflectedDamage = Mathf.CeilToInt(damageAmount * percentageDamage);
                damagableSourse.Health.ApplyDamage(reflectedDamage);
                break;
            case ReflectMode.KillAttacker:
                int fullHpDamage = damagableSourse.Health.MaxValue;
                damagableSourse.Health.ApplyDamage(fullHpDamage);
                break;
        }
    }

    public override void Register() {
        EventBus.SubscribeTo<OnDamageTaken>(ReflectDamage);
    }

    public override void Deregister() {
        EventBus.UnsubscribeFrom<OnDamageTaken>(ReflectDamage);
    }
}
