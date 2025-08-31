using Unity.VisualScripting;
using UnityEngine;

public abstract class OperationData : ScriptableObject {
    public abstract GameOperation CreateOperation();
}

public class DamageOperationData : OperationData {
    public int damage = 6;

    public override GameOperation CreateOperation() {
        return new DamageOperation(damage);
    }
}

public class DamageOperation : GameOperation {
    private const string TargetCreatureKey = "targetCreature";
    private readonly int _damage;

    public DamageOperation(int damage) {
        _damage = damage;

        var anyDamagableEnemyTarget = TargetRequirements.EnemyDamageable;

        // Додаємо вимогу до цілі - ворожа істота
        RequestTargets.Add(new Target(TargetCreatureKey, anyDamagableEnemyTarget)
        );
    }
    public override bool Execute() {
        if (!TryGetTarget(TargetCreatureKey, out UnitPresenter damagablePresenter)) {
            Debug.LogError($"Valid {TargetCreatureKey} not found for damage operation");
            return false;
        }

        IHealthable target = damagablePresenter as IHealthable;

        //Restrict creature view Update

        // Deal damage to the model
        target.Health.TakeDamage(_damage);

        // Create animation or effect here if needed + add final method to allow View update animation will use it to define whne view update should happen

        return true;
    }
}
