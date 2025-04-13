using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

public enum ReflectMode {
    Percentage,
    KillAttacker
}

[CreateAssetMenu(fileName = "Summon Creature Ability", menuName = "Abilities/CardAbilities/Summon")]
public class DealDamageAbilityData : CardAbilityData {
    public int damage = 1;

    public override Ability<CardAbilityData, Card> CreateAbility(Card owner, DiContainer diContainer) {
        throw new NotImplementedException();
    }
}
