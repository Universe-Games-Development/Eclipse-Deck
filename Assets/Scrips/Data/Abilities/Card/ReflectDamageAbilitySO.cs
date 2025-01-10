using UnityEngine;

[CreateAssetMenu(fileName = "ReflectDamage", menuName = "Cards/Abilities/ReflectDamage")]
public class ReflectDamageAbilitySO : CardAbilitySO {
    public enum ReflectMode {
        FixedAmount,    
        Percentage,     
        FullDamage,     
        KillAttacker
    }

    [Header("Reflect Damage Settings")]
    public ReflectMode reflectMode = ReflectMode.FullDamage; // ����� �����������
    public int fixedDamage = 0;     // Գ������� ������� ����� (��� FixedAmount)
    [Range(0, 1)] public float damagePercentage = 0.5f; // ³������ �������� ����� (��� Percentage)

    public override bool ActivateAbility(GameContext gameContext) {
        var attacker = gameContext.sourceCard;
        var defender = gameContext.targetCard;

        if (attacker == null || defender == null) return false;

        int reflectedDamage = 0;

        switch (reflectMode) {
            case ReflectMode.FixedAmount:
                reflectedDamage = fixedDamage;
                break;
            case ReflectMode.Percentage:
                reflectedDamage = Mathf.CeilToInt(gameContext.damage * damagePercentage);
                break;
            case ReflectMode.FullDamage:
                reflectedDamage = gameContext.damage;
                break;
            case ReflectMode.KillAttacker:
                // ������� ���������� ���� ���������
                attacker.Health.ApplyDamage(attacker.Health.CurrentValue);
                // ������ ��������� ����� �����
                // ³���������� ���� ��� ������ ���������
                return true;
        }

        // �������� ������ ����� ���������
        attacker.Health.ApplyDamage(reflectedDamage);
        // ������ ��������� ����� ������� �����
        // ³���������� ���� ��� ������� �����

        return true;
    }
}
