using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Abilities/BaseAbility")]
public abstract class OpponentAblitiesSO : ScriptableObject {
    public string abilityName;
    public string abilityDescription;
    public EventType activationState;

    public virtual void ActivateAbility(GameContext gameContext) {
        Debug.Log("Base ability activation");
    }
}
