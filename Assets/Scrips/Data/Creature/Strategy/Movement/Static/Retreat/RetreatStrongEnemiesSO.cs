using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AfraidStrongEnemiesSO", menuName = "Strategies/Movement/Retreat/StrongerMe")]
public class RetreatStrongEnemiesSO : RetreatStrategySO {
    protected override bool ConditionToEscape() {
        var enemies = navigator.GetCreaturesInDirection(retreatAmount, checkDirection);
        return enemies.Any(enemy => enemy.Attack.CurrentValue > navigator.CurrentCreature.Attack.CurrentValue);
    }
}
