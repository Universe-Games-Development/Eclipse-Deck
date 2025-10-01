using System.Collections.Generic;

public interface ICondition {
    ValidationResult Validate(ValidationContext context = null);
}

public interface ITargetCondition<in T> {
    ValidationResult Validate(T model, ValidationContext context);
}

public class AndTargetCondition<T> : ITargetCondition<T> {
    private readonly ITargetCondition<T>[] _conditions;

    public AndTargetCondition(params ITargetCondition<T>[] conditions) {
        _conditions = conditions;
    }

    public ValidationResult Validate(T target, ValidationContext context) {
        foreach (var condition in _conditions) {
            var result = condition.Validate(target, context);
            if (!result) return result;
        }
        return ValidationResult.Success;
    }
}

public class OrTargetCondition<T> : ITargetCondition<T> {
    private readonly ITargetCondition<T>[] _conditions;

    public OrTargetCondition(params ITargetCondition<T>[] conditions) {
        _conditions = conditions;
    }

    public ValidationResult Validate(T target, ValidationContext context) {
        var errors = new List<string>();
        foreach (var condition in _conditions) {
            var result = condition.Validate(target, context);
            if (result) return ValidationResult.Success;
            errors.Add(result.ErrorMessage);
        }
        return ValidationResult.Error($"None met: {string.Join(", ", errors)}");
    }
}

public static class ConditionExtensions {
    public static ITargetCondition<T> And<T>(this ITargetCondition<T> first, ITargetCondition<T> second)
        => new AndTargetCondition<T>(first, second);

    public static ITargetCondition<T> Or<T>(this ITargetCondition<T> first, ITargetCondition<T> second)
        => new OrTargetCondition<T>(first, second);
}

public class OwnershipCondition : ITargetCondition<UnitModel> {
    private readonly OwnershipType ownershipType;

    public OwnershipCondition(OwnershipType ownershipType) {
        this.ownershipType = ownershipType;
    }

    public ValidationResult Validate(UnitModel model, ValidationContext context) {
        bool isFriendly = model.OwnerId == context.InitiatorId;

        return ownershipType switch {
            OwnershipType.Ally when !isFriendly =>
                ValidationResult.Error("You can only select your own units"),
            OwnershipType.Enemy when isFriendly =>
                ValidationResult.Error("You cannot select your own units"),
            _ => ValidationResult.Success
        };
    }
}
public class MinHealthCondition : ITargetCondition<IHealthable> {
    private readonly int minHealth;

    public MinHealthCondition(int minHealth) {
        this.minHealth = minHealth;
    }

    public ValidationResult Validate(IHealthable model, ValidationContext context) {
        if (model.Health.Current < minHealth) {
            return ValidationResult.Error($"Target must have at least {minHealth} health");
        }
        return ValidationResult.Success;
    }
}
public class AliveCondition : ITargetCondition<IHealthable> {

    public ValidationResult Validate(IHealthable model, ValidationContext context) {
        if (model.Health.IsDead) {
            return ValidationResult.Error($"Target must have alive");
        }
        return ValidationResult.Success;
    }
}

public class ZoneCondition : ITargetCondition<Creature> {
    Zone zone;

    public ZoneCondition(Zone requiredZone) {
        zone = requiredZone;
    }

    public ValidationResult Validate(Creature creature, ValidationContext context) {
        bool creatureZone = zone.Contains(creature);
        return creatureZone
            ? ValidationResult.Success
            : ValidationResult.Error($"Creature must be in {zone}");
    }
}