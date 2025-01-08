using UnityEngine;

[CreateAssetMenu(fileName = "NewPerk", menuName = "Abilities/Perk")]
public class PerkSO : AbilitySO {
    public int Tier; // г���� �����

    public override bool ActivateAbility(GameContext gameContext) {
        Debug.Log("Perk activation");
        // ����� ��� ��������� �����
        return true;
    }
}
