using Cysharp.Threading.Tasks;

public abstract class CardAbility : Ability {
    public CardAbility(CardAbilityData abilityData, GameEventBus eventBus) : base(abilityData, eventBus) {
    }
}

public abstract class ActiveCardAbility : CardAbility {
    public ActiveCardAbility(CardAbilityData abilityData, GameEventBus eventBus) : base(abilityData, eventBus) {
    }
    public abstract UniTask<bool> PerformAbility(IAbilityInputter filler);
}

public abstract class PassiveCardAbility : CardAbility, IPassiveAbility {
    protected Card castingCard;
    public bool IsActive { get; private set; }
    public PassiveCardAbility(Card castingCard, CardAbilityData abilityData, GameEventBus eventBus)
        : base(abilityData, eventBus) {
        this.castingCard = castingCard;
    }

    public void ToggleAbilityTriggering(bool enable) {
        if (IsActive != enable) {
            IsActive = enable;
            if (IsActive) {
                RegisterTrigger();
            } else {
                DeregisterTrigger();
            }
        }
    }

    public abstract void RegisterTrigger();
    public abstract void DeregisterTrigger();
    // When this ability should be active
    public abstract bool ActivationCondition();
    // By default we subscribe to card state changes, but this can be overriden
    protected virtual void SubscribeToEvents() {
        castingCard.OnStateChanged += UpdateActivationState;
    }

    protected virtual void UnsubscribeFromEvents() {
        castingCard.OnStateChanged -= UpdateActivationState;
    }

    public virtual void UpdateActivationState(CardState newState) {
        ToggleAbilityTriggering(ActivationCondition());
    }
}

public abstract class CreatureAbility : Ability {
    public CreatureAbility(CreatureAbilityData abilityData, GameEventBus eventBus) : base(abilityData, eventBus) {
    }
}

public abstract class ActiveCreaturebility : CreatureAbility {
    public ActiveCreaturebility(CreatureAbilityData abilityData, GameEventBus eventBus) : base(abilityData, eventBus) {
    }
    public abstract UniTask<bool> PerformAbility(IAbilityInputter filler);
}

public abstract class PassiveCreatureAbility : CreatureAbility, IPassiveAbility {
    protected Creature castingCreature;
    public bool IsActive { get; private set; }
    public PassiveCreatureAbility(Creature castingCreature, CreatureAbilityData abilityData, GameEventBus eventBus)
        : base(abilityData, eventBus) {
        this.castingCreature = castingCreature;
    }

    public void ToggleAbilityTriggering(bool enable) {
        if (IsActive != enable) {
            IsActive = enable;
            if (IsActive) {
                RegisterTrigger();
            } else {
                DeregisterTrigger();
            }
        }
    }

    public abstract void RegisterTrigger();
    public abstract void DeregisterTrigger();
    // When this ability should be active
    public abstract bool ActivationCondition();
    // By default we subscribe to card state changes, but this can be overriden
    protected virtual void SubscribeToEvents() {
    }

    protected virtual void UnsubscribeFromEvents() {
    }

    public virtual void UpdateActivationState() {
        ToggleAbilityTriggering(ActivationCondition());
    }
}