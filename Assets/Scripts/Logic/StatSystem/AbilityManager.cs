using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class AbilityManager<TAbilityData, TOwner> : IDisposable
    where TAbilityData : AbilityData<TAbilityData, TOwner>
    where TOwner : IAbilityOwner {

    private readonly List<Ability<TAbilityData, TOwner>> _abilities = new();
    private readonly TOwner _owner;
    private DiContainer _diContainer;
    public AbilityManager(TOwner caster, DiContainer diContainer) {
        _owner = caster ?? throw new ArgumentNullException(nameof(caster));
        _diContainer = _diContainer ?? throw new ArgumentNullException(nameof(diContainer));
    }

    public void AddAbilities(IEnumerable<TAbilityData> cardAbilityDatas) {
        foreach (var data in cardAbilityDatas) {
            AddAbility(data);
        }
    }

    public void AddAbility(TAbilityData abilityData) {
        if (abilityData == null) {
            Debug.LogWarning("Ability data is null");
            return;
        }

        var ability = abilityData.CreateAbility(_owner, _diContainer);

        if (ability == null) {
            Debug.LogError($"Failed to create ability from {abilityData.name}");
            return;
        }

        _abilities.Add(ability);
    }

    public void RemoveAbility(Ability<TAbilityData, TOwner> ability) {
        if (!_abilities.Contains(ability)) {
            Debug.LogWarning("Ability not found in manager");
            return;
        }

        if (ability is IDisposable disposable) {
            disposable.Dispose();
        }
        _abilities.Remove(ability);
    }

    public void ClearAbilities() {
        foreach (var ability in _abilities) {
            if (ability is IPassiveAbility passive)
                passive.Deactivate();

            (ability as IDisposable)?.Dispose();
        }
        _abilities.Clear();
    }

    internal List<Ability<TAbilityData, TOwner>> GetAbilities() {
        return _abilities;
    }

    public void Dispose() {
        ClearAbilities();
    }
}

