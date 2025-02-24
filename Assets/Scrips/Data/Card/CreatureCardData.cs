using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureCard", menuName = "TGE/Cards/CreatureCard")]
public class CreatureCardData : CardData {
    public CreatureView viewPrefab;

    [Header ("Creature Data")]
    public int MAX_CARD_ATTACK = 100;
    public int MAX_CARD_HEALTH = 100;

    public int Attack;
    public int Health;

    [Header("Strategy")]
    public CreatureMovementData movementData;
    public CreatureAttackData attackData;
    [Header("Creature Abilities")]
    private List<Ability<CreatureAbilityData, Creature>> creatureAbilities = new();
}
