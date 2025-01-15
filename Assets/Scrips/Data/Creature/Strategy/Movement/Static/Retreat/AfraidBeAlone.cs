using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "RetreatSurroundedSO", menuName = "Strategies/Movement/RetreatSurrounded")]
public class RetreatSurroundedSO : RetreatStrategySO {
    public Direction assistAlly1Direction = Direction.West;
    public Direction assistAlly2Direction = Direction.East;
    private int checkDistance = 1;
    protected override bool ConditionToEscape() {
        var eastAllies = navigator.GetFieldsInDirection(checkDistance, Direction.East).Where(Field => Field.Owner == navigator.CurrentField.Owner);
        var westAllies = navigator.GetFieldsInDirection(checkDistance, Direction.West).Where(Field => Field.Owner == navigator.CurrentField.Owner);
        var frontEnemies = navigator.GetFieldsInDirection(checkDistance, checkDirection).Where(Field => Field.Owner != navigator.CurrentField.Owner);
        return !eastAllies.Any() && !westAllies.Any() && frontEnemies.Any();
    }
}
