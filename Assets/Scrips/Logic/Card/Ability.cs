using System;

public abstract class Ability {
    public AbilityData AbilityData { get; set; }
    protected readonly GameEventBus EventBus;

    public Ability(AbilityData abilityData, GameEventBus eventBus) {
        EventBus = eventBus;
        AbilityData = abilityData;
    }
}

public abstract class PassiveAbility : Ability {
    protected IAbilitiesCaster abilityOwner; // If you need it, implement it
    public bool IsActive;

    public PassiveAbility(AbilityData abilityData, IAbilitiesCaster abilityCaster, GameEventBus eventBus) : base(abilityData, eventBus) {
        abilityOwner = abilityCaster; // If you need it, implement it
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

    protected abstract void SubscribeToEvents();
    protected abstract void UnsubscribeFromEvents();

    public abstract void RegisterTrigger();
    public abstract void DeregisterTrigger();
    public abstract bool ActivationCondition();
    public virtual void UpdateActivationState() {
        ToggleAbilityTriggering(ActivationCondition());
    }

    public void Dispose() {
        UnsubscribeFromEvents();
        GC.SuppressFinalize(this);
    }
}

public abstract class EntityPassiveAbility : PassiveAbility, IDisposable {
    public IHealthEntity Entity { get; private set; }
    private Health _health;

    public EntityPassiveAbility(EntityAbilityData abilityData, IAbilitiesCaster abilityCaster, GameEventBus eventBus)
        : base(abilityData, abilityCaster, eventBus) {
        if (!(abilityCaster is IHealthEntity healthableEntity)) {
            throw new ArgumentNullException(nameof(abilityCaster));
        }

        Entity = healthableEntity;

        _health = Entity.GetHealth() ?? throw new InvalidOperationException("Entity must have a Health component.");

        _health.OnDeath += UpdateActivationState;

        UpdateActivationState();
    }

    protected override void SubscribeToEvents() {
        _health.OnDeath += HandleHealthChanged;
    }

    protected override void UnsubscribeFromEvents() {
        _health.OnDeath -= HandleHealthChanged;
    }

    private void HandleHealthChanged() => UpdateActivationState();

    public override bool ActivationCondition() {
        return _health.IsAlive();
    }
}

public abstract class CardPassiveAbility : PassiveAbility {
    public Card Card { get; private set; }
    public CardAbilityData CardAbilityData { get; private set; }

    public CardPassiveAbility(CardAbilityData abilityData, IAbilitiesCaster abilityCaster, GameEventBus eventBus)
        : base(abilityData, abilityCaster, eventBus) {
        if (!(abilityCaster is Card card)) {
            throw new ArgumentNullException(nameof(abilityCaster));
        }
        Card = card ?? throw new ArgumentNullException(nameof(card));
        CardAbilityData = abilityData ?? throw new ArgumentNullException(nameof(abilityData));
        SubscribeToEvents();


        UpdateActivationState();
    }

    protected override void SubscribeToEvents() {
        Card.OnStateChanged += OnCardStateChanged;
    }

    protected override void UnsubscribeFromEvents() {
        Card.OnStateChanged += OnCardStateChanged;
    }

    private void OnCardStateChanged(CardState state) {
        UpdateActivationState();
    }

    public override bool ActivationCondition() {
        if (CardAbilityData.ActiveStates.Count == 0) return true;
        return CardAbilityData.ActiveStates.Contains(Card.CurrentState);
    }
}


// Подія активації здібності
public struct AbilityActivatedEvent {
    public IAbilitiesCaster Source { get; }
    public Ability ActivatedAbility { get; }

    public AbilityActivatedEvent(IAbilitiesCaster source, Ability abilityType) {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        ActivatedAbility = abilityType ?? throw new ArgumentNullException(nameof(abilityType));
    }
}
