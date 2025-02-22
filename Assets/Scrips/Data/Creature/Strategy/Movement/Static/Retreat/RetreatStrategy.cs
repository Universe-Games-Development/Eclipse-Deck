using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RetreatStrategyType {
    Default,
    StrongEnemies,
    HeroAmbitions
}

[CreateAssetMenu(fileName = "RetreatMovementData", menuName = "Strategies/Movement/Retreat")]
public class RetreatStrategyData : SimpleMoveStrategyData {
    public RetreatStrategyType strategyType = RetreatStrategyType.Default;
    public Direction _checkDirection = Direction.East;
    public int _retreatAmount = 1;
    public int _flankCheck = 1;
    public int _forwardCheck = 1;
    public int minDamagedScared = 1;

    public override MovementStrategy GetInstance() {
        return strategyType switch {
            RetreatStrategyType.Default => new RetreatStrategy(_retreatAmount, _checkDirection, moveAmount, moveDirection),
            RetreatStrategyType.StrongEnemies => new RetreatStrongEnemies(_retreatAmount, _checkDirection, moveAmount, moveDirection),
            RetreatStrategyType.HeroAmbitions => new RetreatSurrounded(_flankCheck, _forwardCheck, _retreatAmount, _checkDirection, moveAmount, moveDirection),
            _ => throw new System.ArgumentOutOfRangeException(),
        };
    }
}

public class RetreatStrategy : SimpleMoveStrategy {
    public int retreatAmount;
    public Direction checkDirection;
    public RetreatStrategy(int retreatAmount, Direction checkDirection, int defaultMoveAmount, Direction defaultMoveDirection)
        : base(defaultMoveAmount, defaultMoveDirection) {
        this.retreatAmount = retreatAmount;
        this.checkDirection = checkDirection;
    }

    public override List<Path> CalculatePath(Field currentField) {
        List<Path> paths = new();
        if (ConditionToEscape(currentField)) {
            paths.Add(CalculateEscape(currentField));
        } else {
            paths = base.CalculatePath(currentField);
        }

        return paths;
    }

    protected virtual bool ConditionToEscape(Field currentField) {
        return false;
    }

    protected Path CalculateEscape(Field currentField) {
        Path escapePath = navigator.GenerateSimplePath(currentField, retreatAmount, checkDirection);

        if (escapePath.isInterrupted) {
            List<Field> freeFields = navigator.GetAdjacentFields(currentField)
                .Where(field => field.Owner == currentField.Owner && field.OccupiedCreature == null)
                .ToList();

            if (freeFields.Count == 0) {
                return escapePath;
            }

            Field fieldToEscape = freeFields.GetRandomElement();
            Direction directionToEscape = navigator.GetDirectionToField(currentField, fieldToEscape);
            escapePath = navigator.GenerateSimplePath(currentField, retreatAmount, directionToEscape);
        }

        return escapePath;
    }
}

public class RetreatStrongEnemies : RetreatStrategy {
    public RetreatStrongEnemies(int retreatAmount, Direction checkDirection, int defaultMoveAmount, Direction defaultMoveDirection) 
        : base(retreatAmount, checkDirection, defaultMoveAmount, defaultMoveDirection) {
    }

    protected override bool ConditionToEscape(Field currentField) {
        var enemies = navigator.GetCreaturesInDirection(currentField,retreatAmount, checkDirection);
        return enemies.Any(enemy => enemy.GetAttack().CurrentValue > currentField.OccupiedCreature.GetAttack().CurrentValue);
    }
}

public class RetreatSurrounded : RetreatStrategy {
    public int _flankCheck = 1;
    public int _forwardCheck = 1;

    public RetreatSurrounded(int flankCheck, int forwardCheck, int retreatAmount, Direction checkDirection, int defaultMoveAmount, Direction defaultMoveDirection) 
        : base(retreatAmount, checkDirection, defaultMoveAmount, defaultMoveDirection) {
        _flankCheck = flankCheck;
        _forwardCheck = forwardCheck;
    }

    protected override bool ConditionToEscape(Field currentField) {
        List<Field> flankFields = navigator.GetFlankFields(currentField, _flankCheck);

        bool allFlankFieldsHaveAllies = flankFields.All(field => field.HasCreature);

        var frontEnemies = navigator.GetFieldsInDirection(currentField, _forwardCheck, checkDirection)
                                    .Where(field => field.Owner != currentField.Owner);

        return allFlankFieldsHaveAllies && frontEnemies.Any();
    }
}

public class RetreatWillDamaged : RetreatStrategy {
    public int _minDamagedScared = 1;

    public RetreatWillDamaged(int minDamagedScared, int retreatAmount, Direction checkDirection, int defaultMoveAmount, Direction defaultMoveDirection)
        : base(retreatAmount, checkDirection, defaultMoveAmount, defaultMoveDirection) {
        _minDamagedScared = minDamagedScared;
    }

    protected override bool ConditionToEscape(Field currentField) {
        var enemies = navigator.GetCreaturesInDirection(currentField, retreatAmount, checkDirection);
        return enemies.Any(enemy => enemy.GetAttack().CurrentValue > _minDamagedScared);
    }
}

