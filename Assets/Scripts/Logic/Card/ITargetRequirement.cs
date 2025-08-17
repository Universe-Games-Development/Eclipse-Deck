using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public interface ITargetRequirement {
    ValidationResult IsValid(object selected, BoardPlayer initiator);
    string GetInstruction();
    TargetSelector GetTargetSelector();
}

public abstract class TargetRequirement<T> : ITargetRequirement where T : GameUnit {
    public IEnumerable<Condition<T>> Conditions { get; private set; }

    public TargetSelector requiredSelector = TargetSelector.Initiator;

    protected TargetRequirement(params Condition<T>[] conditions) {
        Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
    }

    public ValidationResult IsValid(object selected, BoardPlayer initiator) {
        if (!TryConvertToRequired(selected, out T defined)) {
            Debug.Log($"Wrong type selected: {selected}");
            return ValidationResult.Fail();
        }
        foreach (var item in Conditions) {
            item.SetInitiator(initiator);
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
        // Додаємо нову умову до списку
        Conditions = Conditions.Append(condition);
    }
    public virtual string GetInstruction() {
        return "Select a target for the card.";
    }

    public TargetSelector GetTargetSelector() {
        return requiredSelector;
    }
}

public class ZoneRequirement : TargetRequirement<Zone> {
    public ZoneRequirement(params Condition<Zone>[] conditions) : base(conditions) {
    }
    public override string GetInstruction() {
        return "Select a zone to place the card.";
    }
}

public struct ValidationResult {
    public bool IsValid;
    public string ErrorMessage;

    public static ValidationResult Success => new ValidationResult { IsValid = true };
    public static ValidationResult Fail(string message = default) => new ValidationResult { IsValid = false, ErrorMessage = message };
}

public abstract class Condition<T> where T : GameUnit {
    protected BoardPlayer Initiator;

    public void SetInitiator(BoardPlayer opponent) {
        Initiator = opponent;
    }

    public ValidationResult Validate(T model) {
        if (model == null)
            return ValidationResult.Fail("Wrong item selected");

        return CheckCondition(model);
    }

    protected abstract ValidationResult CheckCondition(T model);
}

public enum OwnershipType {
    Friendly,
    Enemy,
    Any
}

public class OwnershipCondition<T> : Condition<T> where T : GameUnit {
    private readonly OwnershipType ownershipType;

    public OwnershipCondition(OwnershipType ownershipType) {
        this.ownershipType = ownershipType;
    }

    protected override ValidationResult CheckCondition(T model) {
        bool isFriendly = model.Owner == Initiator;

        return ownershipType switch {
            OwnershipType.Friendly when !isFriendly =>
                ValidationResult.Fail("You can only select your own units"),
            OwnershipType.Enemy when isFriendly =>
                ValidationResult.Fail("You cannot select your own units"),
            _ => ValidationResult.Success
        };
    }
}

