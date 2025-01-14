using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public static class MovementStrategyFactory {
    public static MovementStrategy GetAttackStrategy(AttackMovementStrategyType strategyType, CreatureMovementDataSO creatureMovement) {
        return strategyType switch {
            AttackMovementStrategyType.None => new NoneMoveStrategy(creatureMovement),
            AttackMovementStrategyType.SimpleMove => new SimpleMoveStrategy(creatureMovement),
            AttackMovementStrategyType.Retreat => new RetreatStrategy(creatureMovement),
            _ => throw new ArgumentException("Invalid attack strategy type")
        };
    }

    public static MovementStrategy GetSupportStrategy(SupportMovementStrategyType strategyType, CreatureMovementDataSO creatureMovement) {
        return strategyType switch {
            SupportMovementStrategyType.None => new NoneMoveStrategy(creatureMovement),
            SupportMovementStrategyType.SimpleMove => new SimpleMoveStrategy(creatureMovement),
            _ => throw new ArgumentException("Invalid support strategy type")
        };
    }
}
