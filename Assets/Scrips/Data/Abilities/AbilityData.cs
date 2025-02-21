using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityData<TSelf, TOwner> : ScriptableObject
    where TSelf : AbilityData<TSelf, TOwner>
    where TOwner : IAbilityOwner {
    public string Name;
    public string Description;
    public Sprite Icon;

    public abstract Ability<TSelf, TOwner> CreateAbility(TOwner owner, GameEventBus eventBus);
}


public abstract class CardAbilityData : AbilityData<CardAbilityData, Card> {
    public List<CardState> ActiveStates = new();
}

public abstract class CreatureAbilityData: AbilityData<CreatureAbilityData, Creature> {
}