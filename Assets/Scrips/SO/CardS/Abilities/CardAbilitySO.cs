using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Cards/Abilities/BaseAbility")]
public abstract class CardAbilitySO : ScriptableObject  {
    public string abilityName;
    public string abilityDescription;
    public CardState activationState;
    public CardState deactivationState;

    public virtual void ActivateAbility(GameContext gameContext) {
        Debug.Log("Base ability activation");
    }
}
