using UnityEngine;

[CreateAssetMenu(fileName = "ReflectDamage", menuName = "Cards/Abilities/ReflectDamage")]
public class ReflectDamageAbilitySO : AbilitySO {
    public override void ActivateAbility(GameContext gameContext) {
        if (gameContext.sourceCard != null && gameContext.targetCard != null) {
            // Відбиваємо шкоду назад до нападника
            int reflectedDamage = gameContext.damage;
            Debug.Log($"Reflecting {reflectedDamage} damage back to {gameContext.sourceCard.Name}");

            // Викликаємо метод TakeDamage для нанесення шкоди назад до карти-нападника
            gameContext.sourceCard.TakeDamage(reflectedDamage);
        }
    }
}
