using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDamagedDialogueData", menuName = "Dialogues/PlayerDamagedDialogueData")]
public class PlayerDamagedDialogueData : RandomEventDialogueData<OnDamageTaken> {
    public int minReactionDamage = 3;

    public override Dictionary<string, string> GetReplacements(OnDamageTaken eventData) {
        return new Dictionary<string, string> {
            { "damageAmount", $"{eventData.Amount} damage"}
        };
    }

    public override bool IsMet(OnDamageTaken eventData) {
        return eventData.Amount >= minReactionDamage;
    }
}
