
using System.Collections.Generic;
using UnityEngine;


public abstract class CreatureAbilitySO : AbilitySO {

}

public abstract class CardAbilitySO : AbilitySO {
    public List<CardState> activationStates;
}

public abstract class AbilitySO : ScriptableObject {
    public string Name;
    public string Description;
    public Sprite Sprite;

    protected void OnValidate() {
        if (string.IsNullOrEmpty(Name)) {
            Name = GetType().Name;
        }
    }
}