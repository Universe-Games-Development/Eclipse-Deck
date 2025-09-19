using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public readonly struct ValidationResult {
    public bool IsValid { get; }
    public string ErrorMessage { get; }

    private ValidationResult(bool isValid, string errorMessage = null) {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static implicit operator bool(ValidationResult result) => result.IsValid;

    public static implicit operator ValidationResult(bool isValid) =>
        new ValidationResult(isValid);

    public static ValidationResult Success => true;
    public static ValidationResult Error(string message) =>
        new ValidationResult(false, message);
}

public interface ITargetRequirement {
    ValidationResult IsValid(object selected, Opponent initiator = null);
    string GetInstruction();
    TargetSelector GetTargetSelector();
    bool AllowSameTargetMultipleTimes { get; } // Чи можна вибирати ту саму ціль кілька разів
}

public interface IGenericRequirement<T> : ITargetRequirement {
    ValidationResult IsValid(T selected, Opponent initiator = null);
}

public class TargetRequirement<T> : IGenericRequirement<T> {
    public IEnumerable<Condition<T>> Conditions { get; private set; }
    public TargetSelector RequiredSelector { get; set; } = TargetSelector.Initiator;
    public bool AllowSameTargetMultipleTimes { get; set; } = false;

    public TargetRequirement(params Condition<T>[] conditions) {
        Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
    }

    public ValidationResult IsValid(object selected, Opponent initiator = null) {
        if (selected is T typedSelected) {
            return IsValid(typedSelected, initiator);
        }
        return ValidationResult.Error($"Expected {typeof(T).Name}, got {selected?.GetType().Name}");
    }

    public ValidationResult IsValid(T selected, Opponent initiator = null) {
        foreach (var condition in Conditions) {
            condition.SetInitiator(initiator);
            var result = condition.Validate(selected);
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
public abstract class Condition<T> {
    protected Opponent Initiator;

    public void SetInitiator(Opponent opponent) {
        Initiator = opponent;
    }

    public ValidationResult Validate(T model) {
        if (model == null)
            return ValidationResult.Error("Wrong item selected");

        return CheckCondition(model);
    }

    protected abstract ValidationResult CheckCondition(T model);
}

public class ZoneRequirement : TargetRequirement<Zone> {
    public ZoneRequirement(params Condition<Zone>[] conditions) : base(conditions) {
    }
    public override string GetInstruction() {
        return "Select a zone";
    }
}

public class CreatureRequirement : TargetRequirement<Creature> {
    public CreatureRequirement(params Condition<Creature>[] conditions) : base(conditions) {
    }
    public override string GetInstruction() {
        return "Select a creature";
    }
}

public class OwnershipCondition<T> : Condition<T> where T : UnitModel {
    private readonly OwnershipType ownershipType;

    public OwnershipCondition(OwnershipType ownershipType) {
        this.ownershipType = ownershipType;
    }

    protected override ValidationResult CheckCondition(T model) {
        bool isFriendly = model.GetPlayer() == Initiator;

        return ownershipType switch {
            OwnershipType.Ally when !isFriendly =>
                ValidationResult.Error("You can only select your own units"),
            OwnershipType.Enemy when isFriendly =>
                ValidationResult.Error("You cannot select your own units"),
            _ => ValidationResult.Success
        };
    }
}

public class HealthableOwnershipCondition : Condition<IHealthable> {
    private readonly OwnershipType ownershipType;

    public HealthableOwnershipCondition(OwnershipType ownershipType) {
        this.ownershipType = ownershipType;
    }

    protected override ValidationResult CheckCondition(IHealthable model) {
        if (model is not UnitModel unitModel) {
            return ValidationResult.Error("Target must be a unit");
        }

        bool isFriendly = unitModel.GetPlayer() == Initiator;

        return ownershipType switch {
            OwnershipType.Ally when !isFriendly =>
                ValidationResult.Error("You can only select your own units"),
            OwnershipType.Enemy when isFriendly =>
                ValidationResult.Error("You cannot select your own units"),
            _ => ValidationResult.Success
        };
    }
}

public static class TargetRequirements {
    public static CreatureRequirement EnemyCreature =>
        new CreatureRequirement(new OwnershipCondition<Creature>(OwnershipType.Enemy));

    public static CreatureRequirement AllyCreature =>
        new CreatureRequirement(new OwnershipCondition<Creature>(OwnershipType.Ally));

    public static CreatureRequirement AnyCreature =>
        new CreatureRequirement();

    public static ZoneRequirement AllyZone =>
        new ZoneRequirement(new OwnershipCondition<Zone>(OwnershipType.Ally));

    public static ZoneRequirement EnemyZone =>
        new ZoneRequirement(new OwnershipCondition<Zone>(OwnershipType.Enemy));

    public static IGenericRequirement<IHealthable> EnemyHealthable =>
        new TargetRequirement<IHealthable>(
            new HealthableOwnershipCondition(OwnershipType.Enemy)
        );

    public static IGenericRequirement<IHealthable> AllyHealthable =>
        new TargetRequirement<IHealthable>(
            new HealthableOwnershipCondition(OwnershipType.Ally)
        );

    public static IGenericRequirement<IHealthable> AnyHealthable =>
        new TargetRequirement<IHealthable>();

    public static IGenericRequirement<IHealthable> HealthableWithMinHealth(int minHealth) =>
        new TargetRequirement<IHealthable>(
            new MinHealthCondition(minHealth)
        );

    public static IGenericRequirement<IHealthable> EnemyWithMinHealth(int minHealth) =>
        new TargetRequirement<IHealthable>(
            new HealthableOwnershipCondition(OwnershipType.Enemy),
            new MinHealthCondition(minHealth)
        );
}


public class MinHealthCondition : Condition<IHealthable> {
    private readonly int minHealth;

    public MinHealthCondition(int minHealth) {
        this.minHealth = minHealth;
    }

    protected override ValidationResult CheckCondition(IHealthable model) {
        if (model.Health.Current < minHealth) {
            return ValidationResult.Error($"Target must have at least {minHealth} health");
        }
        return ValidationResult.Success;
    }
}

