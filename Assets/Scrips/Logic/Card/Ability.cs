using System;

public abstract class Ability {
    public AbilityData AbilityData { get; set; }
    protected readonly GameEventBus EventBus;

    public Ability(AbilityData abilityData, GameEventBus eventBus) {
        EventBus = eventBus;
        AbilityData = abilityData;
    }
}

// Подія активації здібності поки що не використовується
public struct AbilityActivatedEvent {
    public IAbilitiesCaster Source { get; }
    public Ability ActivatedAbility { get; }

    public AbilityActivatedEvent(IAbilitiesCaster source, Ability abilityType) {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        ActivatedAbility = abilityType ?? throw new ArgumentNullException(nameof(abilityType));
    }
}

