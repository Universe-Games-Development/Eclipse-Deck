using UnityEngine;

[CreateAssetMenu(fileName = "Alive Activation Condition", menuName = "Abilities/ActiveConditions/Entity")]
public class EntityAliveCondition : ActivationConditionSO {
    public override bool IsConditionMet(IAbilityOwner owner) {
        return owner is IHasHealth healthable && healthable.GetHealth().IsAlive();
    }
}
