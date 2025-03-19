using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDamagedDialogueData", menuName = "Dialogues/PlayerDamagedDialogueData")]
public class PlayerDamagedDialogueData : DialogueData<OnDamageTaken> {
    public int minReactionDamage = 3;
    public override Dictionary<string, string> GetReplacements(OnDamageTaken eventData) {
        return new Dictionary<string, string> {
            { "damageAmount", eventData.Amount.ToString() }
        };
    }

    public override bool IsMet(OnDamageTaken eventData) {
        bool isEnemyDamaged = eventData.Target is Enemy;
        return isEnemyDamaged && eventData.Amount >= minReactionDamage;
    }
}
