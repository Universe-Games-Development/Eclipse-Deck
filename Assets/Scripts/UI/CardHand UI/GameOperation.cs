using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

public abstract class GameOperation : IExecutableTask {
    [Inject] protected readonly IVisualManager visualManager;
    [Inject] protected readonly IVisualTaskFactory visualTaskFactory;

    protected Dictionary<string, TargetInfo> typedTargets = new();
    public bool IsMandatory { get; set; } = false;
    public UnitModel Source { get; set; }

    public abstract UniTask<bool> Execute();

    // Generic метод для додавання typed targets
    protected void AddTarget(string key, ITargetRequirement requirement) {
        var typedTarget = new TargetInfo(key, requirement);
        typedTargets[key] = typedTarget;
    }

    // Type-safe метод для отримання targets
    protected bool TryGetTypedTarget<T>(string key, out T result) {
        if (typedTargets.TryGetValue(key, out var targetBase) &&  targetBase is TargetInfo typedTarget) {
            result = (T) typedTarget.Unit;
            return typedTarget.HasTarget;
        }

        result = default;
        return false;
    }

    public void SetTarget(string key, object target) {
        if (typedTargets.TryGetValue(key, out var targetBase)) {
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
    public List<TargetInfo> GetTargets() => typedTargets.Values.ToList();


    public bool IsTargetValid(string key) {
        return typedTargets.TryGetValue(key, out var target) && target.IsValid(target.GetTarget());
    }
}


public class TargetInfo {
    public ITargetRequirement Requirement { get; }
    public object Unit { get; private set; }

    public bool HasTarget => Unit != null;

    public string Key { get; }

    public TargetInfo(string key, ITargetRequirement requirement) {
        Requirement = requirement;
        Key = key;
    }

    public void SetTarget(object target) {
        Unit = target;
    }

    public object GetTarget() => Unit;

    public ValidationResult IsValid(object value = null, ValidationContext context = null) {
        return Requirement.IsValid(value, context);
    }

    public TargetSelector GetTargetSelector() {
        return Requirement.RequiredSelector;
    }

    public string GetInstruction() {
        return Requirement.Instruction;
    }
}