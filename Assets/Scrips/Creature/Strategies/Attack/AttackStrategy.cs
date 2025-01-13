using UnityEngine;

[CreateAssetMenu(menuName = "Strategies/AttackStrategy")]
public abstract class AttackStrategy : ScriptableObject {
    public abstract void Attack(GameContext context);
}
