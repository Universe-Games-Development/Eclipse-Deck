using System;
using System.Collections.Generic;
using System.Linq;

public class AbilityManager : IDisposable {
    private readonly IAbilitiesCaster _owner;
    private readonly GameEventBus _eventBus;
    private readonly List<Ability> _activeAbilities = new();
    private bool _isDisposed;

    public AbilityManager(IAbilitiesCaster owner, GameEventBus eventBus) {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public void InitializeAbilities(IEnumerable<AbilityData> abilitiesData) {
        ThrowIfDisposed();

        foreach (var data in abilitiesData) {
            if (data == null) continue;
            var ability = data.GenerateAbility(_owner, _eventBus);

            if (ability is PassiveAbility passiveAbility) {
                passiveAbility.UpdateActivationState();
            }

            _activeAbilities.Add(ability);
        }
    }

    public void AddAbility(AbilityData abilityData) {
        ThrowIfDisposed();

        var ability = abilityData.GenerateAbility(_owner, _eventBus);
        _activeAbilities.Add(ability);

        if (ability is PassiveAbility passive) {
            passive.UpdateActivationState();
        }
    }

    public void RemoveAbility(Ability ability) {
        ThrowIfDisposed();

        if (ability is IDisposable disposable) {
            disposable.Dispose();
        }
        _activeAbilities.Remove(ability);
    }

    public void UpdateAbilities() {
        ThrowIfDisposed();

        foreach (var ability in _activeAbilities.OfType<PassiveAbility>()) {
            ability.UpdateActivationState();
        }
    }

    public void Dispose() {
        if (_isDisposed) return;

        foreach (var ability in _activeAbilities.OfType<IDisposable>()) {
            ability.Dispose();
        }
        _activeAbilities.Clear();

        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed() {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    internal List<Ability> GetAbilities() {
        return _activeAbilities;
    }
}