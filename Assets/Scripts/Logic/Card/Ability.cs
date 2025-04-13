using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

// Ability activation event
public struct AbilityActivatedEvent {
    public IAbilityOwner Source { get; }

    public AbilityActivatedEvent(IAbilityOwner source) {
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }
}
public interface IPassiveAbility {
    void Activate();
    void Deactivate();
}

public interface IActiveAbility {
    UniTask<bool> PerformAbility();
}

public abstract class Ability<TData, TOwner> where TOwner : IAbilityOwner {
    public TData Data { get; set; }
    public TOwner Owner { get; set; }

    public Ability(TData data, TOwner owner) {
        Data = data;
        Owner = owner;
    }
}

public abstract class ActiveAbility<TData, TOwner>
    : Ability<TData, TOwner>, IActiveAbility
    where TOwner : IAbilityOwner {
    protected ActiveAbility(TData data, TOwner owner) : base(data, owner) {
    }

    public abstract UniTask<bool> PerformAbility();
}

public abstract class PassiveAbility<TData, TOwner>
    : Ability<TData, TOwner>, IPassiveAbility
    where TOwner : IAbilityOwner {
    protected bool isActive;
    protected IRequirement abilityActivationRequirement;

    protected PassiveAbility(TData data, TOwner owner) : base(data, owner) {
    }

    public void Activate() {
        if (isActive) return;
        isActive = true;

        ActivateAbilityTriggers();
        RegisterStateChangeHandlers();
    }

    public void Deactivate() {
        if (!isActive) return;
        isActive = false;

        DeactivateAbilityTriggers();
        UnregisterStateChangeHandlers();
    }

    protected abstract void ActivateAbilityTriggers();
    protected abstract void DeactivateAbilityTriggers();

    protected abstract void RegisterStateChangeHandlers();
    protected abstract void UnregisterStateChangeHandlers();

    protected virtual void HandleStateChange() {
        UpdateActivation();
    }
    protected abstract bool CheckActivationConditions();
    protected virtual void UpdateActivation() {
        bool shouldBeActive = CheckActivationConditions();
        if (shouldBeActive != isActive) {
            if (shouldBeActive) Activate();
            else Deactivate();
        }
    }
}

public abstract class CardActiveAbility : ActiveAbility<CardAbilityData, Card> {
    protected Card card;

    protected CardActiveAbility(CardAbilityData data, Card owner) : base(data, owner) {
        card = owner;
    }
}

public abstract class CardPassiveAbility : PassiveAbility<CardAbilityData, Card> {
    protected Card card;

    protected CardPassiveAbility(CardAbilityData data, Card owner) : base(data, owner) {
    }

    protected override void RegisterStateChangeHandlers() {
        card.OnStateChanged += OnCardStateChanged;
    }

    protected override void UnregisterStateChangeHandlers() {
        card.OnStateChanged -= OnCardStateChanged;
    }

    private void OnCardStateChanged(CardState state) {
        HandleStateChange();
    }

    protected override bool CheckActivationConditions() {
        return Owner.CurrentState == CardState.InHand;
    }
}

public abstract class CreaturePassiveAbility : PassiveAbility<CreatureAbilityData, Creature> {
    private Creature creature;

    protected CreaturePassiveAbility(CreatureAbilityData data, Creature owner) : base(data, owner) {
        creature = owner;
    }

    protected override void RegisterStateChangeHandlers() {
        creature.Health.OnDeath += HandleStateChange;
    }

    protected override void UnregisterStateChangeHandlers() {
        creature.Health.OnDeath -= HandleStateChange;
    }

    protected override bool CheckActivationConditions() {
        return abilityActivationRequirement.Check(creature).IsValid;
    }
}