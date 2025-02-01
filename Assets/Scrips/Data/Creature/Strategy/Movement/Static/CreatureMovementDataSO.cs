using UnityEngine;

[CreateAssetMenu(fileName = "CreatureMovementData", menuName = "Strategies/CreatureMovementData/Behaviour")]
public class CreatureMovementDataSO : ScriptableObject {
    [Header("UI")]
    public string Name;
    public Sprite strategyIcon;
    [Space]
    [Header("Attack Strategy")]
    public MovementStrategyData attackStrategy;
    [Space]
    [Header("Support Strategy")]
    public MovementStrategyData supportStrategy;
}
