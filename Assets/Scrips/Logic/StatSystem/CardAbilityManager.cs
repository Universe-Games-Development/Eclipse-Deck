using System;
using System.Collections.Generic;
using System.Linq;

public class CardAbilityManager : IDisposable {
    private readonly Card _casterCard;
    private readonly GameEventBus _eventBus;
    private readonly List<CardAbility> _abilities = new();
    private bool _isDisposed;

    public CardAbilityManager(Card casterCard, GameEventBus eventBus) {
        _casterCard = casterCard ?? throw new ArgumentNullException(nameof(casterCard));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public void InitializeAbilities(IEnumerable<CardAbilityData> cardAbilityDatas) {
        ThrowIfDisposed();

        foreach (var data in cardAbilityDatas) {
            if (data == null) continue;
            AddAbility(data);
        }
    }

    public void AddAbility(CardAbilityData cardabilityData) {
        ThrowIfDisposed();

        var ability = cardabilityData.GenerateAbility(_casterCard, _eventBus);
        _abilities.Add(ability);

        if (ability is PassiveCardAbility passiveAbility) {
            passiveAbility.UpdateActivationState(_casterCard.CurrentState);
        }
    }

    public void RemoveAbility(CardAbility cardAbility) {
        ThrowIfDisposed();

        if (cardAbility is IDisposable disposable) {
            disposable.Dispose();
        }
        _abilities.Remove(cardAbility);
    }

    internal List<CardAbility> GetAbilities() {
        return _abilities;
    }

    private void ThrowIfDisposed() {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    public void Dispose() {
        if (_isDisposed) return;

        foreach (var ability in _abilities.OfType<IDisposable>()) {
            ability.Dispose();
        }
        _abilities.Clear();

        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}

