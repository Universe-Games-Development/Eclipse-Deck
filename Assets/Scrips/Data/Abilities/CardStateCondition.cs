using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card State Activation Condition", menuName = "Abilities/ActiveConditions/Card")]
public class CardStateCondition : ActivationConditionSO {
    public List<CardState> validStates = new();

    public override bool IsConditionMet(IAbilityOwner owner) {
        return owner is Card card && validStates.Contains(card.CurrentState);
    }
}
