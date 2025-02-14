using System.Collections.Generic;
using UnityEngine;

public abstract class ActivationConditionSO : ScriptableObject {
    public abstract bool IsConditionMet(IAbilityOwner owner);
}

public abstract class AbilitySO : ScriptableObject {
    public string Name;
    public string Description;
    public Sprite Sprite;

    [SerializeField]
    protected List<ActivationConditionSO> _activationConditions;

    public bool ShouldBeActive(IAbilityOwner owner) {
        foreach (var condition in _activationConditions) {
            if (!condition.IsConditionMet(owner)) return false;
        }
        return true;
    }

    // Абстрактний метод без EventBus
    public abstract Ability GenerateAbility(IAbilityOwner owner, GameEventBus eventBus);

    protected void OnValidate() {
        if (string.IsNullOrEmpty(Name)) {
            Name = GetType().Name;
        }
    }
}
