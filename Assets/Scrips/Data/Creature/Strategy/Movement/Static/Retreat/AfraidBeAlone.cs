using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RetreatSurroundedSO", menuName = "Strategies/Movement/RetreatSurrounded")]
public class RetreatSurroundedSO : RetreatStrategySO {
    public int flankCheck = 1;
    public int forwardCheck = 1;

    protected override bool ConditionToEscape() {
        List<Field> flankFields = navigator.GetFlankFields(flankCheck);

        bool allFlankFieldsHaveAllies = flankFields.All(field => field.HasCreature);

        var frontEnemies = navigator.GetFieldsInDirection(forwardCheck, checkDirection)
                                    .Where(field => field.Owner != navigator.CurrentField.Owner);

        return allFlankFieldsHaveAllies && frontEnemies.Any();
    }
}
