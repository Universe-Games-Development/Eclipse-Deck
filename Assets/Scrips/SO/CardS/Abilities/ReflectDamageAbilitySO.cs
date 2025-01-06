using UnityEngine;

[CreateAssetMenu(fileName = "ReflectDamage", menuName = "Cards/Abilities/ReflectDamage")]
public class ReflectDamageAbilitySO : CardAbilitySO {
    public override bool ActivateAbility(GameContext gameContext) {
        if (gameContext.sourceCard != null && gameContext.targetCard != null) {
            // ³������� ����� ����� �� ���������
            int reflectedDamage = gameContext.damage;
            Debug.Log($"Reflecting {reflectedDamage} damage back to {gameContext.sourceCard.Name}");

            // ��������� ����� TakeDamage ��� ��������� ����� ����� �� �����-���������
            gameContext.sourceCard.Health.ApplyDamage(reflectedDamage);
        }
        return true;
    }

}
