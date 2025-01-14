using UnityEngine;

[CreateAssetMenu(fileName = "CreatureMovementData", menuName = "Strategies/CreatureMovementData/Simple")]
public class CreatureMovementDataSO : ScriptableObject {
    [Header ("UI")]
    public string Name;
    public Sprite strategyIcon;
    [Space]
    [Header("Attack Strategy")]
    public AttackMovementStrategyType attackStrategyType;
    public Direction attackMoveDirection;
    public int attackMovesAmount = 0;
    [Space]
    [Header("Support Strategy")]
    public SupportMovementStrategyType supportStrategyType;
    public Direction supportMoveDirection;
    public int supportMoveAmount = 0;

    protected virtual void OnValidate() {
        ValidateAttackStrategy();
    }

    protected void ValidateAttackStrategy() {
        // Перевірка на помилкове обрання стратегії Retreat
        if (attackStrategyType == AttackMovementStrategyType.Retreat && this.GetType() != typeof(RetreatMovementDataSO)) {
            Debug.LogError($"Invalid attack strategy type 'Retreat' for {name}. Please use RetreatMovementDataSO instead.");
            attackStrategyType = AttackMovementStrategyType.None; // Змініть на тип за замовчуванням
        }
    }
}

[CreateAssetMenu(fileName = "RetreatMovementData", menuName = "Strategies/CreatureMovementData/Retreat")]
public class RetreatMovementDataSO : CreatureMovementDataSO {
    public int scarredForwardDistance = 1;
    public Direction escapeDirection;

    protected override void OnValidate() {
        base.OnValidate();
        ValidateRetreatData();
    }

    private void ValidateRetreatData() {
        // Додаткові перевірки для RetreatMovementDataSO
        if (attackStrategyType != AttackMovementStrategyType.Retreat) {
            Debug.LogError($"Attack strategy type for {name} must be 'Retreat'.");
            attackStrategyType = AttackMovementStrategyType.Retreat; // Змініть на відповідний тип
        }
    }
}
