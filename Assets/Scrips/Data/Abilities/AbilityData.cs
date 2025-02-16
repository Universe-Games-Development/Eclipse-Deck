using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityData: ScriptableObject {
    public string Name;
    public string Description;
    public Sprite Sprite;

    public abstract Ability GenerateAbility(IAbilitiesCaster owner, GameEventBus eventBus);

    protected void OnValidate() {
        if (string.IsNullOrEmpty(Name)) {
            Name = GetType().Name;
        }
    }
}

public abstract class CardAbilityData : AbilityData {
    public List<CardState> ActiveStates = new();
}

public abstract class EntityAbilityData: AbilityData {
}