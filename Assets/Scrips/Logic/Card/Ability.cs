using System;

public interface IAbility : IDisposable {
    void Register();
    void Deregister();
}

// Ѕазова зд≥бн≥сть без под≥й
public abstract class Ability : IAbility {
    protected IAbilityOwner abilityOwner;
    public AbilitySO AbilityData { get; set; }
    public bool IsActive { get; set; }
    protected readonly GameEventBus EventBus;

    public Ability(AbilitySO abilityData, IAbilityOwner abilityOwner, GameEventBus eventBus) {
        this.abilityOwner = abilityOwner;
        EventBus = eventBus;
        AbilityData = abilityData;
        SubscribeToEvents();
        UpdateAbilityState();
    }

    private void SubscribeToEvents() {
        if (abilityOwner is IHasHealth healthableOwner) {
            healthableOwner.GetHealth().OnDeath += HealthableStateChanged;
            healthableOwner.GetHealth().OnDamageTaken += HealthableStateChanged;
        }

        if (abilityOwner is Card card) {
            card.OnStateChanged += CardStateChanged;
        }
    }

    private void UnsubscribeFromEvents() {
        if (abilityOwner is IHasHealth healthableOwner) {
            healthableOwner.GetHealth().OnDeath -= HealthableStateChanged;
            healthableOwner.GetHealth().OnDamageTaken -= HealthableStateChanged;
        }

        if (abilityOwner is Card card) {
            card.OnStateChanged -= CardStateChanged;
        }
    }
    private void HealthableStateChanged() {
        UpdateAbilityState();
    }

    private void HealthableStateChanged(int currentHealth, IDamageDealer damageDealer) {
        UpdateAbilityState();
    }


    private void CardStateChanged(CardState state) {
        UpdateAbilityState();
    }

    public abstract void Register();
    public abstract void Deregister();

    // Decides Ability can be active in this state or not
    public void UpdateAbilityState() {
        if (AbilityData.ShouldBeActive(abilityOwner) && !IsActive) {
            Register();
            IsActive = true;
        } else if (IsActive) {
            Deregister();
            IsActive = false;
        }
    }

    public virtual void Dispose() {
        Deregister();
        UnsubscribeFromEvents();
        GC.SuppressFinalize(this);
    }
}

public struct AbilityActivatedEvent : IEvent {
    public IAbilityOwner Source { get; }
    public Ability ActivatedAbility { get; }

    public AbilityActivatedEvent(IAbilityOwner source, Ability abilityType) {
        Source = source;
        ActivatedAbility = abilityType;
    }
}