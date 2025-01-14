using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Creature", menuName = "TGE/Creature")]
public class CreatureSO : TableGameEntitySO {

    [Header("Strategy")]
    public CreatureMovementDataSO movementData;
    public AttackStrategy attackStrategy;

    [Header("Abilities")]
    public List<CreatureAbilitySO> creatureAbilities;
}
