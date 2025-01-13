using System.Collections.Generic;
using UnityEngine;

public abstract class AbilitySO : ScriptableObject {
    public string Name;
    public string Description;
    public Sprite Sprite;
    public CardState activationState;
    public List<EventType> eventTriggers;

    public virtual bool ActivateAbility(GameContext gameContext) {
        Debug.Log("Base ability activation");
        return true;
    }

    protected void OnValidate() {
        if (string.IsNullOrEmpty(Name)) {
            Name = GetType().Name;
        }
    }
}
