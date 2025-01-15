using UnityEngine;

[CreateAssetMenu(fileName = "TargetSilenceAbility", menuName = "Cards/Abilities/TargetSilenceAbility")]
public class TargetSilenceAbilitySO : CardAbilitySO {
    public Card targetCard;

    public override bool ActivateAbility(GameContext gameContext) {
        Debug.Log($"Removing ability from card: {targetCard.data.Name}");
        return false;
    }
}
