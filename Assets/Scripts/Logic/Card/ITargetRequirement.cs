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

public interface ITargetRequirement {
    TargetSelector RequiredSelector { get; }
    ValidationResult IsValid(object selected, ValidationContext context = null);
}

public class ValidationContext {
    // Soon be used
    public IReadOnlyDictionary<string, object> PreviouslySelectedTargets { get; }
    public string InitiatorId { get; }
    public object Extra { get; }

    public ValidationContext(string initiatorId, object extra = null) {
        InitiatorId = initiatorId;
        Extra = extra;
    }
}

public class TargetRequirement<T> : ITargetRequirement {
    private List<ITargetCondition<T>> _targetConditions = new();

    public TargetSelector RequiredSelector { get; protected set; }
    public bool AllowSameTargetMultipleTimes { get; set; } = false;

    public TargetRequirement(params ITargetCondition<T>[] conditions) {
        if (conditions != null) {
            _targetConditions.AddRange(conditions);
        }
        RequiredSelector = TargetSelector.Initiator;
    }

    public TargetRequirement<T> AddTargetCondition(ITargetCondition<T> condition) {
        if (condition != null) {
            _targetConditions.Add(condition);
        }
        return this;
    }

    public TargetRequirement<T> WithSelector(TargetSelector selector) {
        RequiredSelector = selector;
        return this;
    }


    public ValidationResult IsValid(object selected, ValidationContext context = null) {
        if (selected == null) {
            return ValidationResult.Error("Nothing was selected");
        }

        if (!(selected is T casted)) {
            return ValidationResult.Error($"Expected {typeof(T).Name}, got {selected.GetType().Name}");
        }

        foreach (var condition in _targetConditions) {
            var result = condition.Validate(casted, context);
            if (!result.IsValid) return result;
        }

        return ValidationResult.Success;
    }
}

public class CompositeTargetRequirement : ITargetRequirement {
    private readonly ITargetRequirement[] _requirements;
    private readonly CompositeType _compositeType;

    public TargetSelector RequiredSelector { get; }

    public CompositeTargetRequirement(CompositeType compositeType, params ITargetRequirement[] requirements) {
        _compositeType = compositeType;
        _requirements = requirements;
        RequiredSelector = TargetSelector.Initiator;
    }

    public ValidationResult IsValid(object selected, ValidationContext context = null) {
        if (_compositeType == CompositeType.Or) {
            foreach (var req in _requirements) {
                if (req.IsValid(selected, context))
                    return ValidationResult.Success;
            }
            return ValidationResult.Error("No valid requirement met");
        }

        foreach (var req in _requirements) {
            var result = req.IsValid(selected, context);
            if (!result) return result;
        }
        return ValidationResult.Success;
    }
}

public enum CompositeType { And, Or }

// Extension methods для зручності
public static class CompositeRequirementExtensions {
    public static CompositeTargetRequirement Or(this ITargetRequirement first, ITargetRequirement second, string instruction = null) {
        return new CompositeTargetRequirement(CompositeType.Or, first, second);
    }

    public static CompositeTargetRequirement And(this ITargetRequirement first, ITargetRequirement second, string instruction = null) {
        return new CompositeTargetRequirement(CompositeType.And, first, second);
    }
}
public class RequirementBuilder<T> {
    private readonly List<ITargetCondition<T>> _targetConditions = new();
    private readonly List<ICondition> _globalConditions = new();
    private TargetSelector _selector = TargetSelector.Initiator;

    // ✅ Compile-time перевірка типів
    public RequirementBuilder<T> WithTargetCondition(ITargetCondition<T> condition) {
        _targetConditions.Add(condition);
        return this;
    }

    public RequirementBuilder<T> WithGlobalCondition(ICondition condition) {
        _globalConditions.Add(condition);
        return this;
    }

    // Для зручності - автоматично визначає тип умови
    public RequirementBuilder<T> WithCondition(ITargetCondition<T> condition) {
        return WithTargetCondition(condition);
    }
    public RequirementBuilder<T> WithCondition(ICondition condition) {
        return WithGlobalCondition(condition);
    }


    public RequirementBuilder<T> WithSelector(TargetSelector selector) {
        _selector = selector;
        return this;
    }

    public TargetRequirement<T> Build() {
        var requirement = new TargetRequirement<T>();
        requirement.WithSelector(_selector);

        foreach (var condition in _targetConditions)
            requirement.AddTargetCondition(condition);

        return requirement;
    }

    
}

public static class TargetRequirements {
    private static readonly TargetRequirement<Creature> _enemyCreature =
        new RequirementBuilder<Creature>()
            .WithCondition(new OwnershipCondition(OwnershipType.Enemy))
            .WithCondition(new AliveCondition())
            .Build();

    private static readonly TargetRequirement<Creature> _allyCreature =
        new RequirementBuilder<Creature>()
            .WithCondition(new OwnershipCondition(OwnershipType.Ally))
            .WithCondition(new AliveCondition())
            .Build();

    private static readonly TargetRequirement<Creature> _anyCreature =
        new RequirementBuilder<Creature>()
            .WithCondition(new AliveCondition())
            .WithCondition(new OwnershipCondition(OwnershipType.Any))
            .Build();

    private static readonly TargetRequirement<Zone> _allyPlace =
        new RequirementBuilder<Zone>()
            .WithCondition(new OwnershipCondition(OwnershipType.Ally))
            .Build();

    private static readonly TargetRequirement<Zone> _enemyPlace =
        new RequirementBuilder<Zone>()
            .WithCondition(new OwnershipCondition(OwnershipType.Enemy))
            .Build();

    public static TargetRequirement<Creature> EnemyCreature => _enemyCreature;

    public static TargetRequirement<Creature> AllyCreature => _allyCreature;

    public static TargetRequirement<Creature> AnyCreature => _anyCreature;

    public static TargetRequirement<Zone> AllyPlace = _allyPlace;

    public static TargetRequirement<Zone> EnemyPlace = _enemyPlace;
}