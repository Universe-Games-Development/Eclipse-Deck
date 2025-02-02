using System.Collections.Generic;
using UnityEngine;

public abstract class AbilitySO : ScriptableObject {
    public string Name;
    public string Description;
    public Sprite Sprite;
    public CardState activationState;
    public List<EventType> eventTriggers;

    public virtual ICommand GenerateAbility(object eventData) {
        Debug.Log("Base ability activation");
        return null;
    }

    protected void OnValidate() {
        if (string.IsNullOrEmpty(Name)) {
            Name = GetType().Name;
        }
    }
}
