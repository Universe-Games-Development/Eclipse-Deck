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
    public ReflectMode reflectMode = ReflectMode.FullDamage; // Режим відображення
    public int fixedDamage = 0;     // Фіксована кількість шкоди (для FixedAmount)
    [Range(0, 1)] public float damagePercentage = 0.5f; // Відсоток отриманої шкоди (для Percentage)

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
                // Завдаємо смертельну рану нападнику
                attacker.Health.ApplyDamage(attacker.Health.CurrentValue);
                // Додаємо візуальний ефект смерті
                // Відправляємо подію про смерть нападника
                return true;
        }

        // Наносимо відбиту шкоду нападнику
        attacker.Health.ApplyDamage(reflectedDamage);
        // Додаємо візуальний ефект відбиття шкоди
        // Відправляємо подію про відбиття шкоди

        return true;
    }
}
