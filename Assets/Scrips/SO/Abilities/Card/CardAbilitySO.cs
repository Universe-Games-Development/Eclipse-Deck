using UnityEngine;

public abstract class CardAbilitySO : AbilitySO {
    public override bool ActivateAbility(GameContext gameContext) {
        Debug.Log("Card ability activation");

        return true;
    }
}
