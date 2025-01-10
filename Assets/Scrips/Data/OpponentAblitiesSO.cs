using UnityEngine;

public abstract class OpponentPerkSO : ScriptableObject {
    public string perkName;
    public string perkDescription;
    public EventType activationState;

    public virtual void ActivateAbility(GameContext gameContext) {
        Debug.Log("Base perk activation");
    }
}
