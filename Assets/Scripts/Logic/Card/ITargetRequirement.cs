using System;
using System.Collections.Generic;

public readonly struct ValidationResult {
    public bool IsValid { get; }
    public string ErrorMessage { get; }

    private ValidationResult(bool isValid, string errorMessage = null) {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static implicit operator bool(ValidationResult result) => result.IsValid;
    public static implicit operator ValidationResult(bool isValid) => new(isValid);

    public static ValidationResult Success => true;
    public static ValidationResult Error(string message) => new(false, message);
}

public class ValidationContext {
    public string InitiatorId { get; }
    public object Extra { get; }

    public ValidationContext(string initiatorId, object extra = null) {
        InitiatorId = initiatorId;
        Extra = extra;
    }
}


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


public interface ITargetRequirement {
    ValidationResult CheckType(object selected, ValidationContext context = null);
    string GetInstruction();
    TargetSelector GetTargetSelector();
}

public interface IGenericRequirement<T> : ITargetRequirement {
    ValidationResult IsValid(T selected, ValidationContext context = null);
}

public class TargetRequirement<T> : IGenericRequirement<T> {
    public IEnumerable<ICondition> Conditions { get; private set; }
    public TargetSelector RequiredSelector { get; set; } = TargetSelector.Initiator;
    public bool AllowSameTargetMultipleTimes { get; set; } = false;

    public TargetRequirement(params ICondition[] conditions) {
        Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
    }

    public ValidationResult CheckType(object selected, ValidationContext context = null) {
        if (selected == null) {
            return ValidationResult.Error($"Nothing was selected");
        }

        if (selected is T typedSelected) {
            return ValidationResult.Success;
        }
        return ValidationResult.Error($"Expected {typeof(T).Name}, got {selected?.GetType().Name}");
    }

    public ValidationResult IsValid(T selected, ValidationContext context = null) {
        ValidationResult typeValidation = CheckType(selected, context);
        if (!typeValidation) {
            return typeValidation;
        }

        foreach (var condition in Conditions) {
            var result = condition.Validate(selected, context);
            if (!result.IsValid) {
                return result;
            }
        }
        return ValidationResult.Success;
    }

    public virtual string GetInstruction() {
        return $"Select {typeof(T).Name}";
    }

    public TargetSelector GetTargetSelector() => RequiredSelector;
}

// Атомарні умови
public class HealthableCondition : Condition<IHealthable> {
    protected override ValidationResult CheckCondition(IHealthable model, ValidationContext context) {
        return ValidationResult.Success; // Просто перевіряємо що це IHealthable
    }
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

// Типізовані Requirements
public class PlaceRequirement : TargetRequirement<Zone> {
    public PlaceRequirement(params ICondition[] conditions) : base(conditions) {
    }
    public override string GetInstruction() {
        return "Select a place";
    }
}
public class CreatureRequirement : TargetRequirement<Creature> {
    public CreatureRequirement(params ICondition[] conditions) : base(conditions) {
    }
    public override string GetInstruction() {
        return "Select a creature";
    }
}

public static class TargetRequirements {
    public static CreatureRequirement EnemyCreature =>
        new(new OwnershipCondition(OwnershipType.Enemy));

    public static CreatureRequirement AllyCreature =>
        new(new OwnershipCondition(OwnershipType.Ally));

    public static CreatureRequirement AnyCreature => new();

    public static PlaceRequirement AllyPlace =>
        new(new OwnershipCondition(OwnershipType.Ally));

    public static PlaceRequirement EnemyPlace =>
        new(new OwnershipCondition(OwnershipType.Enemy));

    // Тепер можемо комбінувати умови різних типів
    public static TargetRequirement<IHealthable> EnemyHealthable =>
        new(
            new HealthableCondition(),
            new OwnershipCondition(OwnershipType.Enemy)
        );

    public static TargetRequirement<IHealthable> AllyHealthable =>
        new(
            new HealthableCondition(),
            new OwnershipCondition(OwnershipType.Ally)
        );

    public static TargetRequirement<IHealthable> AnyHealthable => new();

    public static TargetRequirement<IHealthable> HealthableWithMinHealth(int minHealth) =>
        new(new MinHealthCondition(minHealth));

    public static TargetRequirement<IHealthable> EnemyWithMinHealth(int minHealth) =>
        new(
            new HealthableCondition(),
            new OwnershipCondition(OwnershipType.Enemy),
            new MinHealthCondition(minHealth)
        );

    // Приклад для конкретного типу з різними умовами
    public static CreatureRequirement EnemyCreatureWithMinHealth(int minHealth) =>
        new(
            new OwnershipCondition(OwnershipType.Enemy),
            new MinHealthCondition(minHealth)
        );
}