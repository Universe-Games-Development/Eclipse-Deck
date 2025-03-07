using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurnDialogue", menuName = "Dialogues/TurnDialogue")]
public class RoundDialogueData : DialogueData<OnRoundtart> {
    public int roundCount = 1;

    public override Dictionary<string, string> GetReplacements(OnRoundtart eventData) {
        return new Dictionary<string, string> {
            { "turnCount", eventData.RoundCount.ToString() }
        };
    }

    public override bool IsMet(OnRoundtart eventData) {
        return eventData.RoundCount == roundCount;
    }
}
