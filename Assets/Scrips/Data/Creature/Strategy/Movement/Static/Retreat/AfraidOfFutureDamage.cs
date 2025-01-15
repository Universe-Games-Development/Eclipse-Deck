using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AfraidStrongEnemiesSO", menuName = "Strategies/Movement/Retreat/PossibleDamage")]
public class RetreatDamagedEnemiesSO : RetreatStrategySO {
    protected override bool ConditionToEscape() {
        var enemies = navigator.GetCreaturesInDirection(retreatAmount, checkDirection);
        return enemies.Any(enemy => enemy.Attack.CurrentValue > 0);
    }
}
