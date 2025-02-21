using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
    UniTask<bool> PerformAbility(IActionFiller inputter);
}


public abstract class Ability<TData, TOwner> where TOwner : IAbilityOwner {
    public TData Data { get; }
    public TOwner Owner { get; }
    protected GameEventBus EventBus { get; }

    protected Ability(TData data, TOwner owner, GameEventBus eventBus) {
        Data = data;
        Owner = owner;
        EventBus = eventBus;
    }
}

public abstract class ActiveAbility<TData, TOwner>
    : Ability<TData, TOwner>, IActiveAbility
    where TOwner : IAbilityOwner {
    protected ActiveAbility(TData data, TOwner owner, GameEventBus eventBus) : base(data, owner, eventBus) {
    }

    public abstract UniTask<bool> PerformAbility(IActionFiller inputter);
}

public abstract class PassiveAbility<TData, TOwner>
    : Ability<TData, TOwner>, IPassiveAbility
    where TOwner : IAbilityOwner {
    protected bool isActive;

    protected PassiveAbility(TData data, TOwner owner, GameEventBus eventBus)
        : base(data, owner, eventBus) { }

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

public abstract class CardACtiveAbility : ActiveAbility<CardAbilityData, Card> {
    protected Card card;
    public CardACtiveAbility(CardAbilityData data, Card owner, GameEventBus eventBus)
        : base(data, owner, eventBus) {
        card = owner;
    }
}

public abstract class CardPassiveAbility : PassiveAbility<CardAbilityData, Card> {
    protected Card card;
    public CardPassiveAbility(CardAbilityData data, Card owner, GameEventBus eventBus)
        : base(data, owner, eventBus) {
        card = owner;
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
    protected IRequirement<Creature> abilityActivationRequirement;
    public CreaturePassiveAbility(CreatureAbilityData data, Creature owner, GameEventBus eventBus)
        : base(data, owner, eventBus) {
        abilityActivationRequirement = new RequirementBuilder<Creature>()
                .And(new CreatureAliveRequirement())
                .Build();
    }

    protected override void RegisterStateChangeHandlers() {
        creature.GetHealth().OnDeath += HandleStateChange;
    }

    protected override void UnregisterStateChangeHandlers() {
        creature.GetHealth().OnDeath -= HandleStateChange;
    }

    protected override bool CheckActivationConditions() {
        bool result = abilityActivationRequirement.IsMet(creature, out string callbackMessage);
        Debug.Log(callbackMessage);
        return result;
    }
}