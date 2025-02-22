using System;
using Zenject;

public interface IMovementStrategyFactory {
    IMoveStrategy CreateStrategy(MovementStrategyData config);
}

public class MovementStrategyFactory : IMovementStrategyFactory {
    [Inject] private CreatureNavigator _navigator;

    public IMoveStrategy CreateStrategy(MovementStrategyData config) {
        if (config == null) return new NoneMoveStrategy();

        var strategy = config.GetInstance();
        strategy.Initialize(_navigator);
        return strategy;
    }
}

public class CreatureStrategyMovement {
    private readonly IMoveStrategy _attackStrategy;
    private readonly IMoveStrategy _supportStrategy;

    public CreatureStrategyMovement(
        IMovementStrategyFactory strategyFactory,
        CreatureMovementDataSO movementData
    ) {
        _attackStrategy = CreateStrategy(strategyFactory, movementData.attackStrategy);
        _supportStrategy = CreateStrategy(strategyFactory, movementData.supportStrategy);
    }

    private IMoveStrategy CreateStrategy(
        IMovementStrategyFactory factory,
        MovementStrategyData config
    ) {
        var strategy = factory.CreateStrategy(config);
        return strategy ?? new NoneMoveStrategy();
    }

    public IMoveStrategy GetStrategy(Field currentField) {
        return currentField.FieldType switch {
            FieldType.Attack => _attackStrategy,
            FieldType.Support => _supportStrategy,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
