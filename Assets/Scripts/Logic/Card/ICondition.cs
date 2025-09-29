using System.Collections.Generic;

public interface ICondition {
    ValidationResult Validate(object model, ValidationContext context);
}

public abstract class Condition<T> : ICondition {
    public ValidationResult Validate(object model, ValidationContext context) {
        if (model is not T typedModel)
            return ValidationResult.Error($"Expected {typeof(T).Name}, got {model?.GetType().Name}");

        return CheckCondition(typedModel, context);
    }

    protected abstract ValidationResult CheckCondition(T model, ValidationContext context);
}

public class AndCondition : ICondition {
    private readonly ICondition[] _conditions;

    public AndCondition(params ICondition[] conditions) => _conditions = conditions;

    public ValidationResult Validate(object model, ValidationContext context) {
        foreach (var condition in _conditions) {
            var result = condition.Validate(model, context);
            if (!result) return result;
        }
        return ValidationResult.Success;
    }
}

public class OrCondition : ICondition {
    private readonly ICondition[] _conditions;

    public OrCondition(params ICondition[] conditions) => _conditions = conditions;

    public ValidationResult Validate(object model, ValidationContext context) {
        var errors = new List<string>();
        foreach (var condition in _conditions) {
            var result = condition.Validate(model, context);
            if (result) return ValidationResult.Success;
            errors.Add(result.ErrorMessage);
        }
        return ValidationResult.Error($"None of conditions met: {string.Join(", ", errors)}");
    }
}

public class NotCondition : ICondition {
    private readonly ICondition _condition;

    public NotCondition(ICondition condition) => _condition = condition;

    public ValidationResult Validate(object model, ValidationContext context) {
        var result = _condition.Validate(model, context);
        return result ? ValidationResult.Error("Condition must not be met") : ValidationResult.Success;
    }
}
public static class ConditionExtensions {
    public static ICondition And(this ICondition first, ICondition second)
        => new AndCondition(first, second);

    public static ICondition Or(this ICondition first, ICondition second)
        => new OrCondition(first, second);

    public static ICondition Not(this ICondition condition)
        => new NotCondition(condition);
}

public class OwnershipCondition : Condition<UnitModel> {
    private readonly OwnershipType ownershipType;

    public OwnershipCondition(OwnershipType ownershipType) {
        this.ownershipType = ownershipType;
    }

    protected override ValidationResult CheckCondition(UnitModel model, ValidationContext context) {
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
public class MinHealthCondition : Condition<IHealthable> {
    private readonly int minHealth;

    public MinHealthCondition(int minHealth) {
        this.minHealth = minHealth;
    }

    protected override ValidationResult CheckCondition(IHealthable model, ValidationContext context) {
        if (model.Health.Current < minHealth) {
            return ValidationResult.Error($"Target must have at least {minHealth} health");
        }
        return ValidationResult.Success;
    }
}
public class AliveCondition : Condition<IHealthable> {

    protected override ValidationResult CheckCondition(IHealthable model, ValidationContext context) {
        if (model.Health.IsDead) {
            return ValidationResult.Error($"Target must have alive");
        }
        return ValidationResult.Success;
    }
}