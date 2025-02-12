using System;
using System.Collections.Generic;

public interface IAbilitySource { }

public interface IAbility : IDisposable {
    void Register();
    void Deregister();
}

public abstract class Ability : IAbility {
    protected IAbilitySource abilityOwner;

    protected readonly GameEventBus EventBus;
    protected bool IsActive;

    public Ability(IAbilitySource abilityOwner, GameEventBus eventBus) {
        EventBus = eventBus;
        this.abilityOwner = abilityOwner;
    }

    public abstract void Register();
    public abstract void Deregister();

    public virtual void Dispose() {
        Deregister();
        GC.SuppressFinalize(this);
    }
}

public abstract class CardAbility : Ability {

    public CardAbilitySO abilityData; // Provides configuration data for ability and trigger activation states
    protected Card ownerCard; // Used to determine state of card when it can activate ability
    private readonly HashSet<CardState> _activationStates;

    public CardAbility(CardAbilitySO cardAbilitySO, Card card, GameEventBus eventBus) : base (card, eventBus) {
        ownerCard = card;
        abilityData = cardAbilitySO;
        _activationStates = new HashSet<CardState>(cardAbilitySO.activationStates);

        ownerCard.OnStateChanged += UpdateRegistration;
        UpdateRegistration(card.CurrentState);
    }

    private void UpdateRegistration(CardState state) {
        bool shouldBeActive = abilityData.activationStates.Contains(state);

        if (shouldBeActive && !IsActive) {
            Register();
            IsActive = true;
        } else if (!shouldBeActive && IsActive) {
            Deregister() ;
            IsActive = false;
        }
    }

    public override void Dispose() {
        if (ownerCard != null || ownerCard.OnStateChanged != null) {
            ownerCard.OnStateChanged -= UpdateRegistration;
        }
        base.Dispose();
    }
}

// Creature ability always active when it`s alive so we don`t need check state
public abstract class CreatureAbility : Ability {

    protected CreatureAbilitySO abilityData; // Provides configuration data for ability

    protected bool InTriggerState = false;

    public CreatureAbility(CreatureAbilitySO creatureAbilitySO, IAbilitySource owner, GameEventBus eventBus) : base(owner, eventBus) {
        abilityData = creatureAbilitySO;
        Register();
    }
}

public struct AbilityActivatedEvent : IEvent {
    public IAbilitySource Source { get; }
    public Type AbilityType { get; }

    public AbilityActivatedEvent(IAbilitySource source, Type abilityType) {
        Source = source;
        AbilityType = abilityType;
    }
}