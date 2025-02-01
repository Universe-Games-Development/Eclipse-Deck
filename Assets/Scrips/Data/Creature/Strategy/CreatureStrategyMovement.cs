using UnityEngine;

public class CreatureStrategyMovement {
    private readonly IMoveStrategy attackMovementStrategy;
    private readonly IMoveStrategy supportMovementStrategy;

    public CreatureStrategyMovement(CreatureMovementDataSO movementData, Creature creature) {
        // Thinking to make just GetMoveStrategy method but there will be much enums
        attackMovementStrategy = movementData.attackStrategy?.GetInstance();
        supportMovementStrategy = movementData.supportStrategy?.GetInstance();

        ValidateStrategies();
    }

    private void ValidateStrategies() {
        if (attackMovementStrategy == null) {
            Debug.LogWarning("Attack movement strategy is not defined!");
        }

        if (supportMovementStrategy == null) {
            Debug.LogWarning("Support movement strategy is not defined!");
        }
    }

    public IMoveStrategy GetStrategy(Field currentField) {
        if (currentField.Type == FieldType.Attack) {
            if (attackMovementStrategy == null) {
                Debug.LogError("Attempting to use undefined attack movement strategy!");
                return null;
            }
            return attackMovementStrategy;
        } else if (currentField.Type == FieldType.Support) {
            if (supportMovementStrategy == null) {
                Debug.LogError("Attempting to use undefined support movement strategy!");
                return null;
            }
            return supportMovementStrategy;
        }

        Debug.LogError("No movement strategy for this creature");
        return null;
    }
}
