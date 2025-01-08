using UnityEngine;

[CreateAssetMenu(fileName = "NewPerk", menuName = "Abilities/Perk")]
public class PerkSO : AbilitySO {
    public int Tier; // Рівень перка

    public override bool ActivateAbility(GameContext gameContext) {
        Debug.Log("Perk activation");
        // Логіка для активації перка
        return true;
    }
}
