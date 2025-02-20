using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityData: ScriptableObject {
    public string Name;
    public string Description;
    public Sprite Sprite;


    protected void OnValidate() {
        if (string.IsNullOrEmpty(Name)) {
            Name = GetType().Name;
        }
    }
}

public abstract class CardAbilityData : AbilityData {
    public List<CardState> ActiveStates = new();
    public abstract CardAbility GenerateAbility(Card card, GameEventBus eventBus);
}

public abstract class CreatureAbilityData: AbilityData {
    public abstract CreatureAbility GenerateAbility(Creature creature, GameEventBus eventBus);
}