using UnityEngine;

[CreateAssetMenu(fileName = "TestAbility", menuName = "Cards/Abilities/TestAbility")]
public class TestAbilitySO : CardAbilitySO {
    public override ICommand GenerateAbility(object eventData) {
        Debug.Log("Card is playing ability");
        return null;
    }
}
