using UnityEngine;

[CreateAssetMenu(fileName = "Damaged Activation Condition", menuName = "Abilities/ActiveConditions/Entity")]
public class EntityDamagedCondition : ActivationConditionSO {
    public override bool IsConditionMet(IAbilityOwner owner) {
        return owner is IHasHealth healthable && healthable.GetHealth().IsDamaged();
    }
}
