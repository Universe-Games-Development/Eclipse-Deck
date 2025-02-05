using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "ReflectDamage", menuName = "Abilities/CardAbilities")]
public class ReflectDamageAbilitySO : CreatureAbilitySO {
    public enum ReflectMode {
        FixedAmount,
        Percentage,
        FullDamage,
        KillAttacker
    }

    [Header("Reflect Damage Settings")]
    public ReflectMode reflectMode = ReflectMode.FullDamage;
    public int fixedDamage = 0;
    [Range(0, 1)] public float damagePercentage = 0.5f;

    public override ICommand GenerateAbility(object data) {
        CreatureBattleData creatureBattleData = data as CreatureBattleData;
        if (creatureBattleData == null) {
            return null;
        }

        switch (reflectMode) {
            case ReflectMode.FixedAmount:
                return new ReflectDamageAbilityCommand(creatureBattleData, fixedDamage);

            case ReflectMode.Percentage:
                int reflectedDamage = Mathf.CeilToInt(creatureBattleData.damage * damagePercentage);
                return new ReflectDamageAbilityCommand(creatureBattleData, reflectedDamage);

            case ReflectMode.FullDamage:
                return new ReflectDamageAbilityCommand(creatureBattleData, creatureBattleData.damage);

            case ReflectMode.KillAttacker:
                return new ReflectDamageAbilityCommand(creatureBattleData, true);
        }

        return null;
    }
}

public class ReflectDamageAbilityCommand : ICommand {
    private CreatureBattleData creatureBattleData;
    private int reflectedDamage;
    private bool killAttacker;

    public ReflectDamageAbilityCommand(CreatureBattleData data, int damage) {
        creatureBattleData = data;
        reflectedDamage = damage;
        killAttacker = false;
    }

    public ReflectDamageAbilityCommand(CreatureBattleData data, bool kill) {
        creatureBattleData = data;
        killAttacker = kill;
    }

    public async UniTask Execute() {
        var attacker = creatureBattleData.attacker;
        var defender = creatureBattleData.defender;

        if (attacker == null || defender == null) {
            return;
        }

        if (killAttacker) {
            attacker.Health.ApplyDamage(attacker.Health.CurrentValue);
            // Add death visual effect and notify death event
        } else {
            attacker.Health.ApplyDamage(reflectedDamage);
            // Add reflect visual effect and notify reflect event
        }
        await UniTask.Yield();
    }

    public async UniTask Undo() {
        Debug.Log("Empty undo for reflect ability");
        await UniTask.CompletedTask;
        // Implement undo logic if needed
    }
}
