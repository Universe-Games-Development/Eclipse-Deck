using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class OperationData : ScriptableObject {
    [SerializeReference] public VisualData visualData;

    [SerializeReference]
    public List<ITargetRequirementData> targetRequirements = new();

    public abstract GameOperation CreateOperation(IOperationFactory factory, TargetRegistry targetRegistry);

    protected virtual void Reset() {
        ResetDefaultsRequirements();
    }

    protected virtual void OnValidate() {
        if (targetRequirements == null || targetRequirements.Count == 0)
            ResetDefaultsRequirements();
    }


    protected void AddRequirement(ITargetRequirementData requirement) {
        targetRequirements.Add(requirement);
    }
   

    [ContextMenu("Reset to Default Requirements")] // Бонус: додає опцію в контекстне меню
    public void ResetDefaultsRequirements() {
        targetRequirements.Clear();
        BuildDefaultRequirements();
    }

    protected abstract void BuildDefaultRequirements();

    // Build всі runtime requirements
    public List<ITargetRequirement> BuildRuntimeRequirements() {
        List<ITargetRequirement> targetRequirements = new();
        foreach (var reqData in this.targetRequirements) {
            var req = reqData.BuildRuntime();
            targetRequirements.Add(req);
        }
        return targetRequirements;
    }
}

public class TargetRegistry {
    private readonly Dictionary<TargetKeys, object> _targets = new();

    public TargetRegistry(Dictionary<TargetKeys, object> targets) {
        _targets = targets;
    }

    public void Add(TargetKeys key, object target) {
        _targets[key] = target;
    }

    public bool TryGet<T>(TargetKeys key, out T result) {
        if (_targets.TryGetValue(key, out var value) && value is T casted) {
            result = casted;
            return true;
        }

        result = default;
        return false;
    }

    public T Get<T>(TargetKeys key) {
        if (!_targets.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Target with key '{key}' not found");

        return (T)value;
    }

    public bool HasUnfilledTargets() {
        foreach (var target in _targets.Values) {
            if (target == null) {
                return true;
            }
        }
        return false;
    }

}
