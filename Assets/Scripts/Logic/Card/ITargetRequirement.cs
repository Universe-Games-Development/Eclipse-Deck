using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public enum OwnershipType {
    Ally,
    Enemy,
    Any
}

public struct ValidationResult {
    public bool IsValid;
    public string ErrorMessage;

    public static ValidationResult Success => new ValidationResult { IsValid = true };
    public static ValidationResult Error(string message = default) => new ValidationResult { IsValid = false, ErrorMessage = message };
}

public interface ITargetRequirement {
    ValidationResult IsValid(object selected, BoardPlayer initiator = null);
    string GetInstruction();
    TargetSelector GetTargetSelector();
    bool AllowSameTargetMultipleTimes { get; } // Чи можна вибирати ту саму ціль кілька разів
}

public class TargetRequirement<T> : ITargetRequirement where T : UnitPresenter {
    public IEnumerable<Condition<T>> Conditions { get; private set; }
    public TargetSelector requiredSelector = TargetSelector.Initiator;

    public bool AllowSameTargetMultipleTimes { get; set; } = false;

    public TargetRequirement(params Condition<T>[] conditions) {
        Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
    }

    public static TargetRequirement<T> Single(params Condition<T>[] conditions) {
        return new TargetRequirement<T>(conditions);
    }

    public static TargetRequirement<T> Optional(params Condition<T>[] conditions) {
        return new TargetRequirement<T>(conditions);
    }

    public static TargetRequirement<T> AnyAmount(params Condition<T>[] conditions) {
        return new TargetRequirement<T>(conditions);
    }

    public ValidationResult IsValid(object selected, BoardPlayer initiator = null) {
        if (!TryConvertToRequired(selected, out T defined)) {
            Debug.Log($"Wrong type selected: {selected}");
            return ValidationResult.Error($"Expected {typeof(T).Name}, got {selected?.GetType().Name}");
        }

        foreach (var condition in Conditions) {
            condition.SetInitiator(initiator);
        }

        return ValidateConditions(defined);
    }

    private ValidationResult ValidateConditions(T defined) {
        if (!Conditions.Any()) {
            Debug.LogWarning("No conditions defined. Defaulting to valid.");
            return ValidationResult.Success;
        }

        foreach (var condition in Conditions) {
            var result = condition.Validate(defined);
            if (!result.IsValid) {
                return result;
            }
        }

        return ValidationResult.Success;
    }

    protected bool TryConvertToRequired(object something, out T defined) {
        if (something is T tryDefine) {
            defined = tryDefine;
            return true;
        }
        defined = null;
        return false;
    }

    public void AddCondition(Condition<T> condition) {
        if (condition == null) {
            throw new ArgumentNullException(nameof(condition));
        }
        Conditions = Conditions.Append(condition);
    }

    public virtual string GetInstruction() {
        return "Select target(s)";
    }

    public TargetSelector GetTargetSelector() {
        return requiredSelector;
    }

    public TargetRequirement<T> Clone() {
        return new TargetRequirement<T>(Conditions.ToArray()) {
            requiredSelector = this.requiredSelector,
            AllowSameTargetMultipleTimes = this.AllowSameTargetMultipleTimes
        };
    }
}

public abstract class Condition<T> where T : UnitPresenter {
    protected BoardPlayer Initiator;

    public void SetInitiator(BoardPlayer opponent) {
        Initiator = opponent;
    }

    public ValidationResult Validate(T model) {
        if (model == null)
            return ValidationResult.Error("Wrong item selected");

        return CheckCondition(model);
    }

    protected abstract ValidationResult CheckCondition(T model);
}

public class ZoneRequirement : TargetRequirement<ZonePresenter> {
    public ZoneRequirement(params Condition<ZonePresenter>[] conditions) : base(conditions) {
    }
    public override string GetInstruction() {
        return "Select a zone";
    }
}

public class CreatureRequirement : TargetRequirement<CreaturePresenter> {
    public CreatureRequirement(params Condition<CreaturePresenter>[] conditions) : base(conditions) {
    }
    public override string GetInstruction() {
        return "Select a creature";
    }
}

public class OwnershipCondition<T> : Condition<T> where T : UnitPresenter {
    private readonly OwnershipType ownershipType;

    public OwnershipCondition(OwnershipType ownershipType) {
        this.ownershipType = ownershipType;
    }

    protected override ValidationResult CheckCondition(T presenter) {
        bool isFriendly = presenter.GetPlayer() == Initiator;

        return ownershipType switch {
            OwnershipType.Ally when !isFriendly =>
                ValidationResult.Error("You can only select your own units"),
            OwnershipType.Enemy when isFriendly =>
                ValidationResult.Error("You cannot select your own units"),
            _ => ValidationResult.Success
        };
    }
}

public class IsDamageableCondition<T> : Condition<T> where T : UnitPresenter {
    protected override ValidationResult CheckCondition(T presenter) {
        if (presenter is IHealthable) {
            return ValidationResult.Error("Target without health");
        }
        return ValidationResult.Success;
    }
}

public static class TargetRequirements {
    public static TargetRequirement<UnitPresenter> AllyUnit =>
        new TargetRequirement<UnitPresenter>(new OwnershipCondition<UnitPresenter>(OwnershipType.Ally));

    public static CreatureRequirement EnemyCreature =>
        new CreatureRequirement(new OwnershipCondition<CreaturePresenter>(OwnershipType.Enemy));

    public static CreatureRequirement AllyCreature =>
        new CreatureRequirement(new OwnershipCondition<CreaturePresenter>(OwnershipType.Ally));

    public static CreatureRequirement AnyCreature =>
        new CreatureRequirement();

    public static CreatureRequirement DamageableCreature =>
        new CreatureRequirement(new IsDamageableCondition<CreaturePresenter>());

    public static CreatureRequirement EnemyDamageable =>
        new CreatureRequirement(
            new OwnershipCondition<CreaturePresenter>(OwnershipType.Enemy),
            new IsDamageableCondition<CreaturePresenter>()
        );

    public static CreatureRequirement AllyDamageable =>
        new CreatureRequirement(
            new OwnershipCondition<CreaturePresenter>(OwnershipType.Ally),
            new IsDamageableCondition<CreaturePresenter>()
        );


    public static ZoneRequirement AllyZone =>
        new ZoneRequirement(new OwnershipCondition<ZonePresenter>(OwnershipType.Ally));

    public static ZoneRequirement EnemyZone =>
        new ZoneRequirement(new OwnershipCondition<ZonePresenter>(OwnershipType.Enemy));
}

public static class TargetRequirementExtensions {
    public static T WithCondition<T>(this T requirement, Condition<UnitPresenter> condition)
        where T : TargetRequirement<UnitPresenter> {
        var cloned = requirement.Clone() as T;
        cloned.AddCondition(condition);
        return cloned;
    }

    public static T AsOptional<T>(this T requirement) where T : ITargetRequirement {
        if (requirement is TargetRequirement<UnitPresenter> generic) {
            var cloned = generic.Clone();
            return (T)(object)cloned;
        }
        return requirement;
    }
}

