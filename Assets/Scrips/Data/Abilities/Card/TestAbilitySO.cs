using UnityEngine;

[CreateAssetMenu(fileName = "TestAbility", menuName = "Cards/Abilities/TestAbility")]
public class TestAbilitySO : CardAbilitySO {
    public override bool ActivateAbility(GameContext gameContext) {
        //Debug.Log("Card is playing ability");
        return true;
    }
}
