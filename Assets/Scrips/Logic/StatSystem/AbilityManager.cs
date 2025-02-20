using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityManager<TAbilityData, TOwner> : IDisposable
    where TAbilityData : AbilityData<TAbilityData, TOwner>
    where TOwner : IAbilityOwner {

    private readonly List<Ability<TAbilityData, TOwner>> _abilities = new();
    private readonly TOwner _owner;
    private readonly GameEventBus _eventBus;

    public AbilityManager(TOwner caster, GameEventBus eventBus) {
        _owner = caster ?? throw new ArgumentNullException(nameof(caster));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public void AddAbilities(IEnumerable<TAbilityData> cardAbilityDatas) {
        foreach (var data in cardAbilityDatas) {
            AddAbility(data);
        }
    }

    public void AddAbility(TAbilityData cardabilityData) {
        if (cardabilityData == null) {
            Debug.LogWarning("Ability data is null");
            return;
        }
        Ability<TAbilityData, TOwner> ability = cardabilityData.CreateAbility(_owner, _eventBus);
        _abilities.Add(ability);
    }

    public void RemoveAbility(Ability<TAbilityData, TOwner> ability) {
        if (ability is IDisposable disposable) {
            disposable.Dispose();
        }
        _abilities.Remove(ability);
    }

    internal List<Ability<TAbilityData, TOwner>> GetAbilities() {
        return _abilities;
    }

    public void Dispose() {
        foreach (var ability in _abilities.OfType<IDisposable>()) {
            ability.Dispose();
        }
        _abilities.Clear();
        GC.SuppressFinalize(this);
    }
}

