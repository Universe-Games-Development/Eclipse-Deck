using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Creature", menuName = "Creatures")]
public class CreatureSO : ScriptableObject {
    public string creatureName;
    public Sprite creatureArt;
    public int baseAttack;
    public int baseHealth;

    public MovementStrategy movementStrategy;
    public AttackStrategy attackStrategy;

    public List<CreatureAbilitySO> creatureAbilities;
}
