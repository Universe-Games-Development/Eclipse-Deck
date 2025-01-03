using UnityEngine;

[CreateAssetMenu(fileName = "ReflectDamage", menuName = "Cards/Abilities/ReflectDamage")]
public class ReflectDamageAbilitySO : AbilitySO {
    public override void ActivateAbility(GameContext gameContext) {
        if (gameContext.sourceCard != null && gameContext.targetCard != null) {
            // ³������� ����� ����� �� ���������
            int reflectedDamage = gameContext.damage;
            Debug.Log($"Reflecting {reflectedDamage} damage back to {gameContext.sourceCard.Name}");

            // ��������� ����� TakeDamage ��� ��������� ����� ����� �� �����-���������
            gameContext.sourceCard.TakeDamage(reflectedDamage);
        }
    }
}
