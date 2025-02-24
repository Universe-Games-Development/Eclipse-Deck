using System;
using Zenject;
using UnityEngine;

// TO DO: Generic Handlers to handle attack and movement?
public class CraetureBehaviour {
    [Inject] private CreatureNavigator navigator;

    private CreatureMovementHandler _movementHandler;
    private CreatureAttackHandler _attackHandler;
    public IMoveStrategy GetMovementStrategy(Field currentField) {
        return _movementHandler.GetStrategy(currentField);
    }

    public IAttackStrategy GetAttackStrategy(Field currentField) {
        return _attackHandler.GetStrategy(currentField);
    }

    internal void InitStrategies(Creature creature, CreatureCardData data) {
        _movementHandler = new CreatureMovementHandler(creature, data.movementData, navigator);
        _attackHandler = new CreatureAttackHandler(creature, data.attackData, navigator);
    }
}

public class CreatureMovementHandler {
    private IMoveStrategy _attackStrategy;
    private IMoveStrategy _supportStrategy;
    CreatureNavigator navigator;
    public CreatureMovementHandler(Creature creature, CreatureMovementData movementData, CreatureNavigator navigator) {
        this.navigator = navigator;
        _attackStrategy = CreateStrategy(creature, movementData.attackStrategy);
        _supportStrategy = CreateStrategy(creature, movementData.supportStrategy);
    }

    private IMoveStrategy CreateStrategy(Creature creature, MovementStrategyProvider config) {
        MovementStrategy movementStrategy;
        if (config == null) {
            movementStrategy = new NoneMoveStrategy();
            Debug.LogWarning("movementData is null generating none strategy for : " + creature);
        } else {
            movementStrategy = config.GetInstance();
        }

        movementStrategy.Initialize(creature, navigator);
        return movementStrategy;
    }

    public IMoveStrategy GetStrategy(Field currentField) {
        return currentField.FieldType switch {
            FieldType.Attack => _attackStrategy,
            FieldType.Support => _supportStrategy,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
public class CreatureAttackHandler {
    private IAttackStrategy _attackStrategy;
    private IAttackStrategy _supportStrategy;
    CreatureNavigator navigator;
    public CreatureAttackHandler(Creature creature, CreatureAttackData attackData, CreatureNavigator navigator) {
        this.navigator = navigator;
        _attackStrategy = CreateStrategy(creature, attackData.attackStrategy);
        _supportStrategy = CreateStrategy(creature, attackData.supportStrategy);
    }

    private IAttackStrategy CreateStrategy(
        Creature creature,
        AttackStrategyProvider config
    ) {
        AttackStrategy attackStrategy;
        if (config == null) {
            attackStrategy = new NoneAttackStrategy();
            Debug.LogWarning("movementData is null generating none strategy for : " + creature);
        } else {
            attackStrategy = config.GetInstance();
        }
        attackStrategy.Initialize(creature, navigator);
        return attackStrategy;
    }

    public IAttackStrategy GetStrategy(Field currentField) {
        return currentField.FieldType switch {
            FieldType.Attack => _attackStrategy,
            FieldType.Support => _supportStrategy,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
