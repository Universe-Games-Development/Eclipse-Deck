using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Creature", menuName = "TGE/Creature")]
public class CreatureSO : TableGameEntitySO {

    [Header("Strategy")]
    public CreatureMovementDataSO movementStrategy;
    public AttackStrategySO attackStrategy;

    [Header("Abilities")]
    public List<CreatureAbilitySO> creatureAbilities;
}
