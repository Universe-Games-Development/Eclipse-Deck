using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

public abstract class GameOperation {
    [Inject] protected readonly IVisualManager visualManager;
    [Inject] protected readonly IVisualTaskFactory visualTaskFactory;

    protected Dictionary<string, TypedTargetBase> typedTargets = new();
    public bool IsMandatory { get; set; } = false;
    public UnitModel Source { get; set; }

    public abstract bool Execute();

    // Generic метод для додавання typed targets
    protected void AddTarget<T>(string key, IGenericRequirement<T> requirement) {
        var typedTarget = new TypedTarget<T>(key, requirement);
        typedTargets[key] = typedTarget;
    }

    // Type-safe метод для отримання targets
    protected bool TryGetTypedTarget<T>(string key, out T result) {
        if (typedTargets.TryGetValue(key, out var targetBase) &&
            targetBase is TypedTarget<T> typedTarget) {
            result = typedTarget.Unit;
            return typedTarget.HasTarget;
        }

        result = default;
        return false;
    }

    public void SetTypedTarget<T>(string key, T target) {
        if (typedTargets.TryGetValue(key, out var targetBase)) {
            if (targetBase.TargetType != typeof(T)) {
                throw new ArgumentException($"Type mismatch for target '{key}'. " +
                    $"Expected {targetBase.TargetType}, got {typeof(T)}");
            }
            targetBase.SetTarget(target);
        } else {
            throw new KeyNotFoundException($"Target with key '{key}' not found");
        }
    }
    public void SetTargets(IReadOnlyDictionary<string, object> filledTargets) {
        foreach (var kvp in filledTargets) {
            if (typedTargets.ContainsKey(kvp.Key)) {
                typedTargets[kvp.Key].SetTarget(kvp.Value);
            } else {
                throw new KeyNotFoundException($"Target with key '{kvp.Key}' not found");
            }
        }
    }

    public bool IsReady() => !HasUnfilledTargets();

    public bool HasUnfilledTargets() {
        if (typedTargets == null || typedTargets.Count == 0)
            return false;

        return typedTargets.Values.Any(target => !target.HasTarget);
    }

    public void SetSource(UnitModel source) {
        Source = source;
    }

    public IEnumerable<string> GetTargetKeys() => typedTargets.Keys;
    public List<TypedTargetBase> GetTargets() => typedTargets.Values.ToList();

    public Type GetTargetType(string key) {
        return typedTargets.TryGetValue(key, out var target)
            ? target.TargetType
            : null;
    }

    public bool IsTargetValid(string key) {
        return typedTargets.TryGetValue(key, out var target) && target.IsValid(target.GetTarget());
    }
}

public abstract class TypedTargetBase {
    public string Key { get; }
    public abstract Type TargetType { get; }
    public abstract bool HasTarget { get; }

    protected TypedTargetBase(string key) {
        Key = key;
    }

    public abstract void SetTarget(object target);
    public abstract object GetTarget();
    public abstract ValidationResult IsValid(object value = null, ValidationContext context = null);
    public abstract bool CanTarget(object potentialTarget, ValidationContext context = null);
    public abstract TargetSelector GetTargetSelector();

    public abstract string GetInstruction();
}

public class TypedTarget<T> : TypedTargetBase {
    public IGenericRequirement<T> Requirement { get; }
    public T Unit { get; private set; }

    public override Type TargetType => typeof(T);
    public override bool HasTarget => Unit != null;

    public TypedTarget(string key, IGenericRequirement<T> requirement) : base(key) {
        Requirement = requirement;
    }

    public override void SetTarget(object target) {
        if (target is T typedTarget) {
            Unit = typedTarget;
        } else if (target == null) {
            Unit = default;
        } else {
            throw new ArgumentException($"Invalid target type. Expected {typeof(T)}, got {target?.GetType()}");
        }
    }

    public override object GetTarget() => Unit;

    public override ValidationResult IsValid(object value = null, ValidationContext context = null) {
        T targetToValidate = value is T typedValue ? typedValue : Unit;
        return Requirement.IsValid(targetToValidate, context);
    }

    public override bool CanTarget(object potentialTarget, ValidationContext context = null) {
        return potentialTarget is T typedTarget &&
               Requirement.IsValid(typedTarget, context).IsValid;
    }

    public override TargetSelector GetTargetSelector() {
        return Requirement.GetTargetSelector();
    }

    public override string GetInstruction() {
        return Requirement.GetInstruction();
    }
}