using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ============================================================================
// VALIDATION
// ============================================================================

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
    public IReadOnlyDictionary<string, object> PreviouslySelectedTargets { get; }
    public string InitiatorId { get; }
    public object Extra { get; }

    public ValidationContext(string initiatorId, object extra = null) {
        InitiatorId = initiatorId;
        Extra = extra;
    }
}

public enum CompositeType { And, Or }


#region RUNTIME LAYER

public interface ITargetRequirement {
    TargetSelector RequiredSelector { get; }
    ValidationResult IsValid(object selected, ValidationContext context);
}

public class TargetRequirement<T> : ITargetRequirement {
    private readonly List<ITargetCondition<T>> _conditions = new();
    public TargetSelector RequiredSelector { get; }
    public bool AllowSameTargetMultipleTimes { get; }

    public TargetRequirement(
        TargetSelector selector,
        List<ITargetCondition<T>> conditions,
        bool allowSameTargetMultipleTimes = false) {
        RequiredSelector = selector;
        AllowSameTargetMultipleTimes = allowSameTargetMultipleTimes;
        if (conditions != null) {
            _conditions.AddRange(conditions);
        }
    }

    public ValidationResult IsValid(object selected, ValidationContext context) {
        if (selected == null) {
            return ValidationResult.Error("Nothing was selected");
        }

        if (!(selected is T casted)) {
            return ValidationResult.Error($"Expected {typeof(T).Name}, got {selected.GetType().Name}");
        }

        foreach (var condition in _conditions) {
            var result = condition.Validate(casted, context);
            if (!result.IsValid) return result;
        }

        return ValidationResult.Success;
    }
}

// ============================================================================
// RUNTIME CONDITIONS
// ============================================================================

public interface ITargetCondition {
    ValidationResult Validate(object target, ValidationContext context);
    Type TargetType { get; }
}

public interface ITargetCondition<in T> : ITargetCondition {
    ValidationResult Validate(T target, ValidationContext context);
}

public abstract class TargetCondition<T> : ITargetCondition<T> {
    public Type TargetType => typeof(T);

    public ValidationResult Validate(object target, ValidationContext context) {
        if (target == null) return ValidationResult.Error("Target is null");
        if (target is not T typed) {
            return ValidationResult.Error($"Expected {typeof(T).Name}, got {target.GetType().Name}");
        }
        return Validate(typed, context);
    }

    public abstract ValidationResult Validate(T target, ValidationContext context);
}

// Concrete Runtime Conditions

public class OwnershipTargetCondition : TargetCondition<UnitModel> {
    private readonly OwnershipType ownershipType;

    public OwnershipTargetCondition(OwnershipType ownershipType) {
        this.ownershipType = ownershipType;
    }

    public override ValidationResult Validate(UnitModel model, ValidationContext context) {
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

public class AliveTargetCondition : TargetCondition<IHealthable> {
    public override ValidationResult Validate(IHealthable target, ValidationContext context) {
        return target.IsDead
            ? ValidationResult.Error("Target must be alive")
            : ValidationResult.Success;
    }
}

public class MinHealthTargetCondition : TargetCondition<IHealthable> {
    private readonly int minHealth;

    public MinHealthTargetCondition(int minHealth) {
        this.minHealth = minHealth;
    }

    public override ValidationResult Validate(IHealthable target, ValidationContext context) {
        return target.CurrentHealth < minHealth
            ? ValidationResult.Error($"Target must have at least {minHealth} health")
            : ValidationResult.Success;
    }
}

public class MaxHealthTargetCondition : TargetCondition<IHealthable> {
    private readonly int maxHealth;

    public MaxHealthTargetCondition(int maxHealth) {
        this.maxHealth = maxHealth;
    }

    public override ValidationResult Validate(IHealthable target, ValidationContext context) {
        return target.CurrentHealth > maxHealth
            ? ValidationResult.Error($"Target must have at most {maxHealth} health")
            : ValidationResult.Success;
    }
}

public class ZoneTargetCondition : TargetCondition<Creature> {
    private readonly Zone zone;

    public ZoneTargetCondition(Zone zone) {
        this.zone = zone;
    }

    public override ValidationResult Validate(Creature creature, ValidationContext context) {
        if (zone == null) {
            return ValidationResult.Error("Zone not set");
        }

        return zone.Contains(creature)
            ? ValidationResult.Success
            : ValidationResult.Error($"Creature must be in {zone.UnitName}");
    }
}

// Composite Runtime Conditions

public class AndTargetCondition<T> : TargetCondition<T> {
    private readonly ITargetCondition<T>[] _conditions;

    public AndTargetCondition(params ITargetCondition<T>[] conditions) {
        _conditions = conditions;
    }

    public override ValidationResult Validate(T target, ValidationContext context) {
        foreach (var condition in _conditions) {
            var result = condition.Validate(target, context);
            if (!result) return result;
        }
        return ValidationResult.Success;
    }
}

public class OrTargetCondition<T> : TargetCondition<T> {
    private readonly ITargetCondition<T>[] _conditions;

    public OrTargetCondition(params ITargetCondition<T>[] conditions) {
        _conditions = conditions;
    }

    public override ValidationResult Validate(T target, ValidationContext context) {
        var errors = new List<string>();
        foreach (var condition in _conditions) {
            var result = condition.Validate(target, context);
            if (result) return ValidationResult.Success;
            errors.Add(result.ErrorMessage);
        }
        return ValidationResult.Error($"None met: {string.Join(", ", errors)}");
    }
}

#endregion


#region DATA LAYER

public interface ISerializableCondition {
    string GetDisplayName();
}

public interface ISerializableTargetCondition<in T> : ISerializableCondition {
    ITargetCondition<T> BuildRuntime();
}

public interface ISerializableTargetCondition : ISerializableCondition {
    Type TargetType { get; }
    ITargetCondition BuildBaseCondition();
}

[System.Serializable]
public abstract class SerializableTargetCondition<T> : ISerializableTargetCondition<T>, ISerializableTargetCondition {
    public Type TargetType => typeof(T);

    public abstract string GetDisplayName();

    public abstract ITargetCondition<T> BuildRuntime();

    public ITargetCondition BuildBaseCondition() => BuildRuntime();
}

// ============================================================================
// CONCRETE SERIALIZABLE CONDITIONS
// ============================================================================

[System.Serializable]
public class OwnershipConditionData : SerializableTargetCondition<UnitModel> {
    public OwnershipType ownershipType = OwnershipType.Enemy;

    public override string GetDisplayName() => $"Ownership: {ownershipType}";

    public override ITargetCondition<UnitModel> BuildRuntime()
        => new OwnershipTargetCondition(ownershipType);
}

[System.Serializable]
public class AliveConditionData : SerializableTargetCondition<IHealthable> {
    public override string GetDisplayName() => "Must be alive";

    public override ITargetCondition<IHealthable> BuildRuntime()
        => new AliveTargetCondition();
}

[System.Serializable]
public class MinHealthConditionData : SerializableTargetCondition<IHealthable> {
    public int minHealth = 1;

    public override string GetDisplayName() => $"Min Health: {minHealth}";

    public override ITargetCondition<IHealthable> BuildRuntime()
        => new MinHealthTargetCondition(minHealth);
}

[System.Serializable]
public class MaxHealthConditionData : SerializableTargetCondition<IHealthable> {
    public int maxHealth = 10;

    public override string GetDisplayName() => $"Max Health: {maxHealth}";

    public override ITargetCondition<IHealthable> BuildRuntime()
        => new MaxHealthTargetCondition(maxHealth);
}

//Dynamic Zone Condition
[System.Serializable]
public class ZoneConditionData : SerializableTargetCondition<Creature> {
    public Zone requiredZone;

    public ZoneConditionData(Zone requiredZone) {
        this.requiredZone = requiredZone;
    }

    public override string GetDisplayName() => $"Must be in zone: {requiredZone?.UnitName ?? "None"}";

    public override ITargetCondition<Creature> BuildRuntime()
        => new ZoneTargetCondition(requiredZone);
}

// ============================================================================
// COMPOSITE SERIALIZABLE CONDITIONS
// ============================================================================

[System.Serializable]
public class AndConditionData<T> : SerializableTargetCondition<T> {
    [SerializeReference]
    public List<ISerializableTargetCondition<T>> conditions = new();

    public override string GetDisplayName() => $"AND ({conditions.Count} conditions)";

    public override ITargetCondition<T> BuildRuntime() {
        var runtimeConditions = conditions
            .Select(c => c.BuildRuntime())
            .ToArray();
        return new AndTargetCondition<T>(runtimeConditions);
    }
}

[System.Serializable]
public class OrConditionData<T> : SerializableTargetCondition<T> {
    [SerializeReference]
    public List<ISerializableTargetCondition<T>> conditions = new();

    public override string GetDisplayName() => $"OR ({conditions.Count} conditions)";

    public override ITargetCondition<T> BuildRuntime() {
        var runtimeConditions = conditions
            .Select(c => c.BuildRuntime())
            .ToArray();
        return new OrTargetCondition<T>(runtimeConditions);
    }
}

// ============================================================================
// SERIALIZABLE TARGET REQUIREMENT - Тепер type-safe! ✅
// ============================================================================

public interface ITargetRequirementData {
    TargetKeys TargetKey { get; }
    string Instruction { get; }
    TargetSelector RequiredSelector { get; }
    ITargetRequirement BuildRuntime();
}

[System.Serializable]
public abstract class TargetRequirementData<T> : ITargetRequirementData {
    [SerializeField]
    public TargetKeys targetKey = TargetKeys.MainTarget;

    public TargetKeys TargetKey => targetKey;
    public TargetSelector selector = TargetSelector.Initiator;
    public bool allowSameTargetMultipleTimes = false;
    // ✅ Тепер список параметризований по типу T!
    [SerializeReference]
    public List<ISerializableTargetCondition<T>> conditions = new();
    public TargetSelector RequiredSelector => selector;

    public string Instruction { get; }

    public void AddConditionData(ISerializableTargetCondition<T> condition) {
        if (condition == null)
            throw new ArgumentNullException(nameof(condition));

        if (conditions.Contains(condition))
            return; // або кинути виняток, або просто ігнорувати

        conditions.Add(condition);
    }


    public ITargetRequirement BuildRuntime() {
        // ✅ Тепер все type-safe!
        var runtimeConditions = conditions
            .Select(c => c.BuildRuntime())
            .ToList();

        return new TargetRequirement<T>(
            selector,
            runtimeConditions,
            allowSameTargetMultipleTimes
        );
    }
}

// ============================================================================
// CONCRETE TARGET REQUIREMENTS
// ============================================================================

[System.Serializable]
public class OpponentTargetRequirementData : TargetRequirementData<Opponent> {
}

[System.Serializable]
public class CreatureTargetRequirementData : TargetRequirementData<Creature> {
}

[System.Serializable]
public class ZoneTargetRequirementData : TargetRequirementData<Zone> {
}

[System.Serializable]
public class HealthableTargetRequirementData : TargetRequirementData<IHealthable> {
}

[System.Serializable]
public class UnitModelTargetRequirementData : TargetRequirementData<UnitModel> {
}

#endregion

// ============================================================================
// PRESETS
// ============================================================================

public static class RequirementPresets {
    public static CreatureTargetRequirementData EnemyCreature(TargetKeys targetKey) => new() {
        targetKey = targetKey,
        selector = TargetSelector.Initiator,
        conditions = new List<ISerializableTargetCondition<Creature>> {
            new OwnershipConditionData { ownershipType = OwnershipType.Enemy },
            new AliveConditionData()
        }
    };

    public static CreatureTargetRequirementData AllyCreature(TargetKeys targetKey) => new() {
        targetKey = targetKey,
        selector = TargetSelector.Initiator,
        conditions = new List<ISerializableTargetCondition<Creature>> {
            new OwnershipConditionData { ownershipType = OwnershipType.Ally },
            new AliveConditionData()
        }
    };

    public static ZoneTargetRequirementData AllyZone(TargetKeys targetKey) => new() {
        targetKey    = targetKey,
        selector = TargetSelector.Initiator,
        conditions = new List<ISerializableTargetCondition<Zone>> {
            new OwnershipConditionData { ownershipType = OwnershipType.Ally }
        }
    };

    public static HealthableTargetRequirementData Damageble(TargetKeys targetKey) => new() {
        targetKey = targetKey,
        selector = TargetSelector.Initiator,
        conditions = new List<ISerializableTargetCondition<IHealthable>> {
            new AliveConditionData(),
            new MaxHealthConditionData { maxHealth = 99 }
        }
    };

    public static CreatureTargetRequirementData EnemyOrWeakAlly(TargetKeys targetKey) => new() {
        targetKey = targetKey,
        selector = TargetSelector.Initiator,
        conditions = new List<ISerializableTargetCondition<Creature>> {
            new AliveConditionData(),
            new OrConditionData<Creature> {
                conditions = new List<ISerializableTargetCondition<Creature>> {
                    new OwnershipConditionData { ownershipType = OwnershipType.Enemy },
                    new AndConditionData<Creature> {
                        conditions = new List<ISerializableTargetCondition<Creature>> {
                            new OwnershipConditionData { ownershipType = OwnershipType.Ally },
                            new MaxHealthConditionData { maxHealth = 5 }
                        }
                    }
                }
            }
        }
    };
}



public static class ConditionDataExtensions {
    // ✅ Type-safe extension methods
    public static AndConditionData<T> And<T>(
        this ISerializableTargetCondition<T> first,
        params ISerializableTargetCondition<T>[] others) {
        var all = new List<ISerializableTargetCondition<T>> { first };
        all.AddRange(others);

        return new AndConditionData<T> {
            conditions = all
        };
    }

    public static OrConditionData<T> Or<T>(
        this ISerializableTargetCondition<T> first,
        params ISerializableTargetCondition<T>[] others) {
        var all = new List<ISerializableTargetCondition<T>> { first };
        all.AddRange(others);

        return new OrConditionData<T> {
            conditions = all
        };
    }
}
