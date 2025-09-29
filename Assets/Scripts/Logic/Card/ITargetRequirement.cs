using System.Collections.Generic;
using System.Linq;

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
    string Instruction { get; }
    public void ChangeInstruction(string newInstruction);

    ValidationResult CheckType(object selected);
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
    private List<ICondition> _conditions = new();
    public IReadOnlyList<ICondition> Conditions => _conditions;

    public TargetSelector RequiredSelector { get; protected set; }
    public bool AllowSameTargetMultipleTimes { get; set; } = false;

    public string Instruction { get; private set;  }

    public TargetRequirement(params ICondition[] conditions) {
        if (conditions != null) {
            _conditions.AddRange(conditions);
        }
        RequiredSelector = TargetSelector.Initiator;
        Instruction = $"Select {typeof(T).Name}";
    }

    public TargetRequirement<T> AddCondition(ICondition condition) {
        if (condition != null) {
            _conditions.Add(condition);
        }
        return this;
    }

    public TargetRequirement<T> WithSelector(TargetSelector selector) {
        RequiredSelector = selector;
        return this;
    }



    public ValidationResult CheckType(object selected) {
        if (selected == null) {
            return ValidationResult.Error($"Nothing was selected");
        }

        if (selected is T typedSelected) {
            return ValidationResult.Success;
        }
        return ValidationResult.Error($"Expected {typeof(T).Name}, got {selected?.GetType().Name}");
    }

    public ValidationResult IsValid(object selected, ValidationContext context = null) {
        ValidationResult typeValidation = CheckType(selected);
        if (!typeValidation) {
            return typeValidation;
        }

        foreach (var condition in _conditions) {
            var result = condition.Validate(selected, context);
            if (!result.IsValid) {
                return result;
            }
        }
        return ValidationResult.Success;
    }

    public void ChangeInstruction(string newInstruction) {
        Instruction = newInstruction;
    }
}

public class CompositeTargetRequirement : ITargetRequirement {
    private readonly ITargetRequirement[] _requirements;
    private readonly CompositeType _compositeType;

    public TargetSelector RequiredSelector { get; }
    public string Instruction { get; private set; }

    public CompositeTargetRequirement(CompositeType compositeType, string instruction, params ITargetRequirement[] requirements) {
        _compositeType = compositeType;
        _requirements = requirements;
        Instruction = instruction;
        RequiredSelector = TargetSelector.Initiator; // Або логіка для вибору
    }

    public ValidationResult CheckType(object selected) {
        // Для OR - достатньо одного валідного типу
        if (_compositeType == CompositeType.Or) {
            foreach (var req in _requirements) {
                if (req.CheckType(selected))
                    return ValidationResult.Success;
            }
            return ValidationResult.Error(Instruction);
        }

        // Для AND - всі типи мають бути валідними
        foreach (var req in _requirements) {
            var result = req.CheckType(selected);
            if (!result) return result;
        }
        return ValidationResult.Success;
    }

    public ValidationResult IsValid(object selected, ValidationContext context = null) {
        if (_compositeType == CompositeType.Or) {
            foreach (var req in _requirements) {
                if (req.CheckType(selected) && req.IsValid(selected, context))
                    return ValidationResult.Success;
            }
            return ValidationResult.Error("No valid requirement met");
        }

        // AND logic
        foreach (var req in _requirements) {
            var result = req.IsValid(selected, context);
            if (!result) return result;
        }
        return ValidationResult.Success;
    }

    public void ChangeInstruction(string newInstruction) {
        Instruction = newInstruction;
    }
}

public enum CompositeType { And, Or }

// Extension methods для зручності
public static class CompositeRequirementExtensions {
    public static CompositeTargetRequirement Or(this ITargetRequirement first, ITargetRequirement second, string instruction = null) {
        return new CompositeTargetRequirement(CompositeType.Or, instruction ?? $"Select {first.Instruction} OR {second.Instruction}", first, second);
    }

    public static CompositeTargetRequirement And(this ITargetRequirement first, ITargetRequirement second, string instruction = null) {
        return new CompositeTargetRequirement(CompositeType.And, instruction ?? $"Select {first.Instruction} AND {second.Instruction}", first, second);
    }
}
public class RequirementBuilder<T> {
    private readonly List<ICondition> _conditions = new();
    private TargetSelector _selector = TargetSelector.Initiator;
    private string _message;

    public RequirementBuilder<T> WithOwnership(OwnershipType type) {
        _conditions.Add(new OwnershipCondition(type));
        return this;
    }

    public RequirementBuilder<T> WithCondition(ICondition condition) {
        _conditions.Add(condition);
        return this;
    }

    public RequirementBuilder<T> WithSelector(TargetSelector selector) {
        _selector = selector;
        return this;
    }

    public RequirementBuilder<T> WithDescription(string message) {
        _message = message;
        return this;
    }

    public TargetRequirement<T> Build() {
        var requirement = new TargetRequirement<T>(_conditions.ToArray());
        requirement.WithSelector(_selector);
        if (_message != null) requirement.ChangeInstruction(_message);
        return requirement;
    }
}

public static class TargetRequirements {
    private static readonly TargetRequirement<Creature> _enemyCreature =
        new RequirementBuilder<Creature>()
            .WithOwnership(OwnershipType.Enemy)
            .WithCondition(new AliveCondition())
            .Build();
    private static readonly TargetRequirement<Creature> _allyCreature =
        new RequirementBuilder<Creature>()
            .WithOwnership(OwnershipType.Enemy)
            .WithCondition(new AliveCondition())
            .Build();

    private static readonly TargetRequirement<Creature> _anyCreature =
        new RequirementBuilder<Creature>()
            .WithCondition(new AliveCondition())
            .WithOwnership(OwnershipType.Any)
            .Build();

    private static readonly TargetRequirement<Zone> _allyPlace =
        new RequirementBuilder<Zone>()
            .WithOwnership(OwnershipType.Ally)
            .Build();

    private static readonly TargetRequirement<Zone> _enemyPlace =
        new RequirementBuilder<Zone>()
            .WithOwnership(OwnershipType.Enemy)
            .Build();
    private static readonly TargetRequirement<IHealthable> _enemyHealthable =
       new RequirementBuilder<IHealthable>()
           .WithOwnership(OwnershipType.Enemy)
           .Build();

    public static TargetRequirement<Creature> EnemyCreature => _enemyCreature;

    public static TargetRequirement<Creature> AllyCreature => _allyCreature;

    public static TargetRequirement<Creature> AnyCreature => _anyCreature;

    public static TargetRequirement<Zone> AllyPlace = _allyPlace;

    public static TargetRequirement<Zone> EnemyPlace = _enemyPlace;

    public static TargetRequirement<IHealthable> EnemyHealthable = _enemyHealthable;
}
